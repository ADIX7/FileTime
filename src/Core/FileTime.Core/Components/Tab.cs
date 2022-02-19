using AsyncEvent;
using FileTime.Core.Models;

namespace FileTime.Core.Components
{
    public class Tab
    {
        private IItem? _currentSelectedItem;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private IContainer _currentLocation;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string? _lastPath;

        private bool _currentlySelecting = false;

        private readonly object _guardSetCurrentSelectedItemCTS = new();
        private CancellationTokenSource? _setCurrentSelectedItemCTS;

        public int CurrentSelectedIndex { get; private set; }

        public bool AutoRefresh { get; set; }

        public AsyncEventHandler CurrentLocationChanged = new();
        public AsyncEventHandler<bool> CurrentSelectedItemChanged = new();

        public async Task Init(IContainer currentPath)
        {
            await SetCurrentLocation(currentPath);
        }

        public Task<IContainer> GetCurrentLocation(CancellationToken token = default)
        {
            return Task.FromResult(_currentLocation);
        }

        public async Task SetCurrentLocation(IContainer value)
        {
            if (_currentLocation != value)
            {
                if (_currentLocation != null)
                {
                    _currentLocation.Refreshed.Remove(HandleCurrentLocationRefresh);
                }

                _currentLocation = value;
                await CurrentLocationChanged.InvokeAsync(this, AsyncEventArgs.Empty);

                var currentLocationItems = await (await GetCurrentLocation()).GetItems();
                if (currentLocationItems == null) throw new Exception("Could not get current location items.");
                await SetCurrentSelectedItem(await GetItemByLastPath() ?? (currentLocationItems.Count > 0 ? currentLocationItems[0] : null), true);
                _currentLocation.Refreshed.Add(HandleCurrentLocationRefresh);
            }
        }

        public Task<IItem?> GetCurrentSelectedItem()
        {
            return Task.FromResult(_currentSelectedItem);
        }

        /*public async Task<bool> SetCurrentSelectedItem(IItem? value, bool secondary = false, CancellationToken token = default)
        {

        }*/
        public async Task<bool> SetCurrentSelectedItem(IItem? value, bool locationChanged = false, CancellationToken token = default)
        {
            if (_currentlySelecting) return false;

            try
            {
                _currentlySelecting = true;

                IItem? itemToSelect = null;
                if (value != null)
                {
                    itemToSelect = (await _currentLocation.GetItems(token))?.FirstOrDefault(i =>
                        i.FullName == null && value?.FullName == null
                        ? i.Name == value?.Name
                        : i.FullName == value?.FullName);
                    if (itemToSelect == null) throw new IndexOutOfRangeException($"Provided item ({value.FullName ?? "unknwon"}) does not exists in the current container ({_currentLocation.FullName ?? "unknwon"}).");
                }

                CancellationToken newToken;
                lock (_guardSetCurrentSelectedItemCTS)
                {
                    if (token.IsCancellationRequested) return false;
                    _setCurrentSelectedItemCTS?.Cancel();
                    if (token.IsCancellationRequested)
                    {
                        _setCurrentSelectedItemCTS = new CancellationTokenSource();
                        newToken = _setCurrentSelectedItemCTS.Token;
                    }
                    else
                    {
                        _setCurrentSelectedItemCTS = new CancellationTokenSource();
                        newToken = CancellationTokenSource.CreateLinkedTokenSource(_setCurrentSelectedItemCTS.Token, token).Token;
                    }
                }

                _currentSelectedItem = itemToSelect;
                _lastPath = GetCommonPath(_lastPath, itemToSelect?.FullName);

                CurrentSelectedIndex = await GetItemIndex(itemToSelect, CancellationToken.None);

                await CurrentSelectedItemChanged.InvokeAsync(this, locationChanged, newToken);

                return !newToken.IsCancellationRequested;
            }
            finally
            {
                _currentlySelecting = false;
            }
        }
        public async Task<IItem?> GetItemByLastPath(IContainer? container = null)
        {
            container ??= _currentLocation;
            var containerFullName = container.FullName;

            if (_lastPath == null
                || (containerFullName != null && !_lastPath.StartsWith(containerFullName))
             )
            {
                return null;
            }


            var itemNameToSelect = _lastPath
                .Split(Constants.SeparatorChar)
                .Skip((containerFullName?.Split(Constants.SeparatorChar).Length) ?? 0)
                .FirstOrDefault();

            return (await container.GetItems())?.FirstOrDefault(i => i.Name == itemNameToSelect);
        }

        private static string GetCommonPath(string? oldPath, string? newPath)
        {
            var oldPathParts = oldPath?.Split(Constants.SeparatorChar) ?? Array.Empty<string>();
            var newPathParts = newPath?.Split(Constants.SeparatorChar) ?? Array.Empty<string>();

            var commonPathParts = new List<string>();

            var max = oldPathParts.Length > newPathParts.Length ? oldPathParts.Length : newPathParts.Length;

            for (var i = 0; i < max; i++)
            {
                if (newPathParts.Length <= i)
                {
                    commonPathParts.AddRange(oldPathParts.Skip(i));
                    break;
                }
                else if (oldPathParts.Length <= i || oldPathParts[i] != newPathParts[i])
                {
                    commonPathParts.AddRange(newPathParts.Skip(i));
                    break;
                }
                else if (oldPathParts[i] == newPathParts[i])
                {
                    commonPathParts.Add(oldPathParts[i]);
                }
            }

            return string.Join(Constants.SeparatorChar, commonPathParts);
        }

        private async Task HandleCurrentLocationRefresh(object? sender, AsyncEventArgs e, CancellationToken token = default)
        {
            var currentSelectedName = (await GetCurrentSelectedItem())?.FullName ?? (await GetItemByLastPath())?.FullName;
            var currentLocationItems = (await (await GetCurrentLocation(token)).GetItems(token))!;
            if (currentSelectedName != null)
            {
                await SetCurrentSelectedItem(currentLocationItems.FirstOrDefault(i => i.FullName == currentSelectedName) ?? (currentLocationItems.Count > 0 ? currentLocationItems[0] : null), token: token);
            }
            else if (currentLocationItems.Count > 0)
            {
                await SetCurrentSelectedItem(currentLocationItems[0], token: token);
            }
        }

        public async Task SelectFirstItem(CancellationToken token = default)
        {
            var currentLocationItems = (await (await GetCurrentLocation(token)).GetItems(token))!;
            if (currentLocationItems.Count > 0)
            {
                await SetCurrentSelectedItem(currentLocationItems[0], token: token);
            }
        }

        public async Task SelectLastItem(CancellationToken token = default)
        {
            var currentLocationItems = (await (await GetCurrentLocation(token)).GetItems(token))!;
            if (currentLocationItems.Count > 0)
            {
                await SetCurrentSelectedItem(currentLocationItems[currentLocationItems.Count - 1], token: token);
            }
        }

        public async Task SelectPreviousItem(int skip = 0, CancellationToken token = default)
        {
            var currentLocationItems = (await (await GetCurrentLocation(token)).GetItems(token))!;
            var possibleItemsToSelect = currentLocationItems.Take(CurrentSelectedIndex).Reverse().Skip(skip).ToList();

            if (possibleItemsToSelect.Count == 0) possibleItemsToSelect = currentLocationItems.ToList();
            await SelectItem(possibleItemsToSelect, token);
        }

        public async Task SelectNextItem(int skip = 0, CancellationToken token = default)
        {
            var currentLocationItems = (await (await GetCurrentLocation(token)).GetItems(token))!;
            var possibleItemsToSelect = currentLocationItems.Skip(CurrentSelectedIndex + 1 + skip).ToList();

            if (possibleItemsToSelect.Count == 0) possibleItemsToSelect = currentLocationItems.Reverse().ToList();
            await SelectItem(possibleItemsToSelect, token);
        }

        private async Task SelectItem(IEnumerable<IItem> currentPossibleItems, CancellationToken token = default)
        {
            if (!currentPossibleItems.Any()) return;

            if (token.IsCancellationRequested) return;
            var currentLocation = await GetCurrentLocation(token);
            var currentLocationItems = (await currentLocation.GetItems(token))!;

            if (await GetCurrentSelectedItem() != null)
            {
                if (token.IsCancellationRequested) return;

                IItem? newSelectedItem = null;
                try
                {
                    _currentlySelecting = true;
                    if (AutoRefresh && currentLocation != null)
                    {
                        await currentLocation.RefreshAsync(token);
                        if (token.IsCancellationRequested) return;
                    }

                    foreach (var item in currentPossibleItems)
                    {
                        if (currentLocationItems.FirstOrDefault(i => i.Name == item.Name) is var possibleNewSelectedItem
                            && possibleNewSelectedItem is not null)
                        {
                            newSelectedItem = possibleNewSelectedItem;
                            break;
                        }
                    }

                    if (newSelectedItem != null)
                    {
                        newSelectedItem = (await (await GetCurrentLocation(token)).GetItems(token))?.FirstOrDefault(i => i.Name == newSelectedItem.Name);
                    }
                }
                finally
                {
                    _currentlySelecting = false;
                }
                await SetCurrentSelectedItem(newSelectedItem ?? (currentLocationItems.Count > 0 ? currentLocationItems[0] : null), token: token);
            }
            else
            {
                await SetCurrentSelectedItem(currentLocationItems.Count > 0 ? currentLocationItems[0] : null, token: token);
            }
        }

        public async Task GoToProvider()
        {
            var currentLocatin = await GetCurrentLocation();
            if (currentLocatin == null) return;

            await SetCurrentLocation(currentLocatin.Provider);
        }

        public async Task GoToRoot()
        {
            var currentLocatin = await GetCurrentLocation();
            if (currentLocatin == null) return;

            var root = currentLocatin;
            while (root!.GetParent() != null)
            {
                root = root.GetParent();
            }

            await SetCurrentLocation(root);
        }

        public async Task GoUp()
        {
            var currentLocationItems = (await (await GetCurrentLocation()).GetItems())!;
            var lastCurrentLocation = await GetCurrentLocation();
            var parent = (await GetCurrentLocation()).GetParent();

            if (parent is not null)
            {
                if (lastCurrentLocation is VirtualContainer lastCurrentVirtualContainer)
                {
                    await SetCurrentLocation(lastCurrentVirtualContainer.CloneVirtualChainFor(parent, v => v.IsPermanent));
                    await SetCurrentSelectedItem(lastCurrentVirtualContainer.GetRealContainer(), true);
                }
                else
                {
                    await SetCurrentLocation(parent);
                    var newCurrentLocation = (await (await GetCurrentLocation()).GetItems())?.FirstOrDefault(i => i.Name == lastCurrentLocation.Name);
                    await SetCurrentSelectedItem(newCurrentLocation, true);
                }

                foreach (var lastLocationItem in currentLocationItems.OfType<IContainer>())
                {
                    lastLocationItem.Destroy();
                }
            }
        }

        public async Task Open()
        {
            var currentLocationItems = (await (await GetCurrentLocation()).GetItems())!;
            if (_currentSelectedItem is IContainer childContainer)
            {
                if (await GetCurrentLocation() is VirtualContainer currentVirtuakContainer)
                {
                    await SetCurrentLocation(currentVirtuakContainer.CloneVirtualChainFor(childContainer, v => v.IsPermanent));
                }
                else
                {
                    await SetCurrentLocation(childContainer);
                }
            }
        }

        public async Task OpenContainer(IContainer container) => await SetCurrentLocation(container);

        private async Task<int> GetItemIndex(IItem? item, CancellationToken token)
        {
            if (item == null) return -1;
            var currentLocationItems = (await (await GetCurrentLocation(token)).GetItems(token))!;

            if (token.IsCancellationRequested) return -1;
            for (var i = 0; i < currentLocationItems.Count; i++)
            {
                if (currentLocationItems[i] == item) return i;
            }

            return -1;
        }
    }
}