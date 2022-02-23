using FileTime.Core.Models;

namespace FileTime.Core.ContainerSizeScanner
{
    public class ScanSizeTask
    {
        private readonly object _scanGuard = new();
        private bool _scannig;
        private CancellationTokenSource? _cancellationTokenSource;

        public IContainer ContainerToScan { get; }
        public ContainerSizeContainer Snapshot { get; }

        public bool Scanning
        {
            get
            {
                lock (_scanGuard)
                {
                    return _scannig;
                }
            }
        }

        public ScanSizeTask(ContainerScanSnapshotProvider provider, IContainer containerToScan)
        {
            ContainerToScan = containerToScan;
            Snapshot = new ContainerSizeContainer(provider, provider, containerToScan, "Size scan on " + containerToScan.DisplayName);
            Task.Run(async () => await provider.AddSnapshotAsync(Snapshot)).Wait();
        }

        public void Start()
        {
            lock (_scanGuard)
            {
                if (_scannig) return;
                _scannig = true;
            }

            new Thread(BootstrapScan).Start();

            void BootstrapScan()
            {
                try
                {
                    Task.Run(async () => await Snapshot.RunWithLazyLoading(async (token) => await ScanAsync(ContainerToScan, Snapshot, token))).Wait();
                }
                finally
                {
                    lock (_scanGuard)
                    {
                        _scannig = false;
                        _cancellationTokenSource = null;
                    }
                }
            }
        }

        private async Task ScanAsync(IContainer container, ContainerSizeContainer targetContainer, CancellationToken token)
        {
            if (IsScanCancelled(targetContainer)) return;

            var childElements = await container.GetElements(token);
            if (childElements != null)
            {
                foreach (var childElement in childElements)
                {
                    if (token.IsCancellationRequested
                        || IsScanCancelled(targetContainer))
                    {
                        return;
                    }
                    var newSizeElement = new ContainerSizeElement(targetContainer.Provider, targetContainer, childElement, await childElement.GetElementSize(token) ?? 0);
                    await targetContainer.AddElementAsync(newSizeElement);
                }
            }

            var childContainers = await container.GetContainers(token);
            if (childContainers != null)
            {
                var newSizeContainers = new List<(ContainerSizeContainer, IContainer)>();
                foreach (var childContainer in childContainers)
                {
                    var newSizeContainer = new ContainerSizeContainer(targetContainer.Provider, targetContainer, childContainer);
                    await targetContainer.AddContainerAsync(newSizeContainer);
                    newSizeContainers.Add((newSizeContainer, childContainer));
                }

                foreach (var (newSizeContainer, childContainer) in newSizeContainers)
                {
                    if (token.IsCancellationRequested
                        || IsScanCancelled(newSizeContainer))
                    {
                        return;
                    }
                    await newSizeContainer.RunWithLazyLoading(async (token) => await ScanAsync(childContainer, newSizeContainer, token), token);
                }
            }

            await targetContainer.UpdateSize();
        }

        private static bool IsScanCancelled(ContainerSizeContainer container)
        {
            IContainer? parent = container;
            while (parent is ContainerSizeContainer sizeContainer)
            {
                if (!sizeContainer.AllowSizeScan)
                {
                    return true;
                }
                parent = parent.GetParent();
            }

            return false;
        }

        public void Cancel()
        {
            lock (_scanGuard)
            {
                if (_scannig || _cancellationTokenSource == null) return;
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
        }
    }
}