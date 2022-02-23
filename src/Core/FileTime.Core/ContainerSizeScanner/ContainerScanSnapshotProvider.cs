using System.Threading.Tasks;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Core.ContainerSizeScanner
{
    public class ContainerScanSnapshotProvider : ContentProviderBase<ContainerScanSnapshotProvider>
    {

        public ContainerScanSnapshotProvider() : base("size", null, "size://", false)
        {
        }

        public override Task<bool> CanHandlePath(string path) => Task.FromResult(path.StartsWith(Protocol));

        public override Task<IContainer> CreateContainerAsync(string name) => throw new NotSupportedException();

        public override Task<IElement> CreateElementAsync(string name) => throw new NotSupportedException();

        public async Task AddSnapshotAsync(ContainerSizeContainer snapshot)
        {
            if (RootContainers != null)
            {
                RootContainers.Add(snapshot);
                while (RootContainers.Count > 10)
                {
                    RootContainers.RemoveAt(0);
                }
                await RefreshAsync();
            }
        }
    }
}