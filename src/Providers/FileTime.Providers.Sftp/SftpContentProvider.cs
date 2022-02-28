using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using Microsoft.Extensions.Logging;

namespace FileTime.Providers.Sftp
{
    public class SftpContentProvider : ContentProviderBase<SftpContentProvider>, IContainer
    {
        private readonly IInputInterface _inputInterface;
        private readonly ILogger<SftpContentProvider> _logger;

        public SftpContentProvider(IInputInterface inputInterface, ILogger<SftpContentProvider> logger)
            : base("sftp", "sftp://", false)
        {
            _logger = logger;
            _inputInterface = inputInterface;
        }

        public override async Task<IContainer> CreateContainerAsync(string name)
        {
            var container = RootContainers?.FirstOrDefault(c => c.Name == name);

            if (container == null)
            {
                container = new SftpServer(name, this, _inputInterface);
                await AddRootContainer(container);
            }

            await RefreshAsync();

            //await SaveServers();

            return container;
        }

        public override Task<IElement> CreateElementAsync(string name) => throw new NotSupportedException();

        async Task<IItem?> IContainer.GetByPath(string path, bool acceptDeepestMatch)
        {
            if (path == null) return this;

            var pathParts = path.TrimStart(Constants.SeparatorChar).Split(Constants.SeparatorChar);

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
    }
}