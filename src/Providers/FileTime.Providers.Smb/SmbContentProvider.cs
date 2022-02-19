using System.Runtime.InteropServices;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Smb
{
    public class SmbContentProvider : ContentProviderBase<SmbContentProvider>, IContainer
    {
        private readonly IInputInterface _inputInterface;
        private readonly Persistence.PersistenceService _persistenceService;
        private readonly ILogger<SmbContentProvider> _logger;

        public SmbContentProvider(IInputInterface inputInterface, Persistence.PersistenceService persistenceService, ILogger<SmbContentProvider> logger)
            : base("smb", null, "smb://", true)
        {
            _inputInterface = inputInterface;
            _persistenceService = persistenceService;
            _logger = logger;
        }

        public override async Task<IContainer> CreateContainerAsync(string name)
        {
            var container = RootContainers?.FirstOrDefault(c => c.Name == name);

            if (container == null)
            {
                container = new SmbServer(name, this, _inputInterface);
                await AddRootContainer(container);
            }

            await RefreshAsync();

            await SaveServers();

            return container;
        }

        public override Task<IElement> CreateElementAsync(string name) => throw new NotSupportedException();

        async Task<IItem?> IContainer.GetByPath(string path, bool acceptDeepestMatch)
        {
            if (path == null) return this;

            path = path.TrimStart(Constants.SeparatorChar);
            if (path.StartsWith("\\\\")) path = path[2..];
            else if (path.StartsWith("smb://")) path = path[6..];
            var pathParts = path.Split(Constants.SeparatorChar);

            var rootContainer = (await GetContainers())?.FirstOrDefault(c => c.Name == pathParts[0]);

            if (rootContainer == null)
            {
                return null;
            }

            var remainingPath = string.Join(Constants.SeparatorChar, pathParts.Skip(1));
            try
            {
                return remainingPath.Length == 0 ? rootContainer : await rootContainer.GetByPath(remainingPath, acceptDeepestMatch);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting path {Path}", path);
                if (acceptDeepestMatch)
                {
                    return rootContainer ?? this;
                }
                else
                {
                    throw;
                }
            }
        }

        public override Task<bool> CanHandlePath(string path) => Task.FromResult(path.StartsWith("smb://") || path.StartsWith(@"\\"));

        public async Task SaveServers()
        {
            try
            {
                await _persistenceService.SaveServers(RootContainers?.OfType<SmbServer>() ?? Enumerable.Empty<SmbServer>());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unkown error while saving smb server states.");
            }
        }

        protected override async Task Init()
        {
            var servers = await _persistenceService.LoadServers();
            SetRootContainers(servers.Select(s => new SmbServer(s.Path, this, _inputInterface, s.UserName, s.Password)));
        }

        public static string GetNativePathSeparator() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/";
    }
}