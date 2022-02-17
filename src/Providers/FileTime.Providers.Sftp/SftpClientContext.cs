using Renci.SshNet;

namespace FileTime.Providers.Sftp
{
    public class SftpClientContext
    {
        private readonly Func<int, Task<SftpClient>> _getSftpClient;
        private readonly Action _disposeClient;
        private bool _isRunning;
        private readonly object _lock = new();

        public SftpClientContext(Func<int, Task<SftpClient>> getSftpClient, Action disposeClient)
        {
            _getSftpClient = getSftpClient;
            _disposeClient = disposeClient;
        }
        public async Task RunWithSftpClientAsync(Action<SftpClient> action, int maxRetries = SftpServer.MAXRETRIES)
        {
            await RunWithSftpClientAsync<object?>((client) => { action(client); return null; }, maxRetries);
        }

        public async Task<T> RunWithSftpClientAsync<T>(Func<SftpClient, T> func, int maxRetries = SftpServer.MAXRETRIES)
        {
            while (true)
            {
                lock (_lock)
                {
                    if (!_isRunning)
                    {
                        _isRunning = true;
                        break;
                    }
                }

                await Task.Delay(1);
            }
            try
            {
                SftpClient client;
                while (true)
                {
                    try
                    {
                        client = await _getSftpClient(maxRetries);
                        return func(client);
                    }
                    //TODO: dispose client on Sftp exception
                    catch (Exception e)
                    {
                        throw new Exception("Exception was thrown while executing method with SftpClient.", e);
                    }
                }
            }
            finally
            {
                lock (_lock)
                {
                    _isRunning = false;
                }
            }
        }
    }
}