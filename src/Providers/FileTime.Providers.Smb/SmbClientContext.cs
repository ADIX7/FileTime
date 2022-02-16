using SMBLibrary.Client;

namespace FileTime.Providers.Smb
{
    public class SmbClientContext
    {
        private readonly Func<int, Task<ISMBClient>> _getSmbClient;
        private readonly Action _disposeClient;
        private bool _isRunning;
        private readonly object _lock = new();

        public SmbClientContext(Func<int, Task<ISMBClient>> getSmbClient, Action disposeClient)
        {
            _getSmbClient = getSmbClient;
            _disposeClient = disposeClient;
        }

        public async Task RunWithSmbClientAsync(Action<ISMBClient> action, int maxRetries = SmbServer.MAXRETRIES)
        {
            await RunWithSmbClientAsync<object?>((client) => { action(client); return null; }, maxRetries);
        }

        public async Task<T> RunWithSmbClientAsync<T>(Func<ISMBClient, T> func, int maxRetries = SmbServer.MAXRETRIES)
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
                ISMBClient client;
                while (true)
                {
                    try
                    {
                        client = await _getSmbClient(maxRetries);
                        return func(client);
                    }
                    catch (Exception e) when (e.Source == "SMBLibrary")
                    {
                        _disposeClient();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Exception was thrown while executing method with SmbClient.", e);
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