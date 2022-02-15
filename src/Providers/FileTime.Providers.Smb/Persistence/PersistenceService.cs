using System.Text.Json;
using FileTime.Core.Persistence;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Smb.Persistence
{
    public class PersistenceService
    {
        private const string smbFolderName = "smb";
        private const string serverFileName = "servers.json";
        private readonly PersistenceSettings _persistenceSettings;
        private readonly JsonSerializerOptions _jsonOptions;
        private static readonly byte[] _encryptionKey = {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
        };
        private readonly ILogger<PersistenceService> _logger;

        public PersistenceService(PersistenceSettings persistenceSettings, ILogger<PersistenceService> logger)
        {
            _persistenceSettings = persistenceSettings;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }
        public async Task SaveServers(IEnumerable<Smb.SmbServer> servers)
        {
            ServersPersistenceRoot root;
            string? encodedIV = null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = _encryptionKey;

                encodedIV = Convert.ToBase64String(aes.IV);

                root = new ServersPersistenceRoot()
                {
                    Key = encodedIV,
                    Servers = servers.Select(s => SaveServer(s, aes)).ToList()
                };
            }

            var smbDirectory = new DirectoryInfo(Path.Combine(_persistenceSettings.RootAppDataPath, smbFolderName));
            if (!smbDirectory.Exists) smbDirectory.Create();

            var serversPath = Path.Combine(_persistenceSettings.RootAppDataPath, smbFolderName, serverFileName);

            using var stream = File.Create(serversPath);
            await JsonSerializer.SerializeAsync(stream, root, _jsonOptions);
        }

        public async Task<List<SmbServer>> LoadServers()
        {
            var serverFilePath = Path.Combine(_persistenceSettings.RootAppDataPath, smbFolderName, serverFileName);
            var servers = new List<SmbServer>();

            if (!new FileInfo(serverFilePath).Exists) return servers;

            using var stream = File.OpenRead(serverFilePath);
            var serversRoot = await JsonSerializer.DeserializeAsync<ServersPersistenceRoot>(stream);

            if (serversRoot == null) return servers;

            if (!string.IsNullOrEmpty(serversRoot.Key))
            {
                var iv = Convert.FromBase64String(serversRoot.Key);

                using Aes aes = Aes.Create();
                foreach (var server in serversRoot.Servers)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(server.Password)) continue;

                        using var memoryStream = new MemoryStream();
                        memoryStream.Write(Convert.FromBase64String(server.Password));
                        memoryStream.Position = 0;

                        using CryptoStream cryptoStream = new(
                           memoryStream,
                           aes.CreateDecryptor(_encryptionKey, iv),
                           CryptoStreamMode.Read);
                        using StreamReader decryptReader = new(cryptoStream);
                        server.Password = await decryptReader.ReadToEndAsync();
                    }
                    catch(Exception e)
                    {
                        _logger.LogError(e, "Unkown error while decrypting password for {ServerName}", server.Name);
                    }
                }
            }

            servers.AddRange(serversRoot.Servers);

            return servers;
        }

        private static SmbServer SaveServer(Smb.SmbServer server, Aes aes)
        {
            var encryptedPassword = "";
            using (var memoryStream = new MemoryStream())
            {
                using CryptoStream cryptoStream = new(
                    memoryStream,
                    aes.CreateEncryptor(),
                    CryptoStreamMode.Write);
                using StreamWriter encryptWriter = new(cryptoStream);
                {
                    encryptWriter.Write(server.Password);
                    encryptWriter.Flush();
                    cryptoStream.FlushFinalBlock();
                }

                var a = memoryStream.ToArray();
                encryptedPassword = Convert.ToBase64String(a);
            }

            return new SmbServer()
            {
                Path = server.Name,
                Name = server.Name,
                UserName = server.Username,
                Password = encryptedPassword
            };
        }
    }
}