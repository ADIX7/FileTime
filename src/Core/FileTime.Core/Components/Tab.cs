using AsyncEvent;
using FileTime.Core.Models;

namespace FileTime.Core.Components
{
    public class Tab
    {
        private IItem? _currentSelectedItem;
        private IContainer _currentLocation;

        /* public IContainer CurrentLocation
        {
            get => _currentLocation;
            private set
            {
                if (_currentLocation != value)
                {
                    if (_currentLocation != null)
                    {
                        _currentLocation.Refreshed -= HandleCurrentLocationRefresh;
                    }

                    _currentLocation = value;
                    CurrentLocationChanged?.Invoke(this, EventArgs.Empty);
                    CurrentSelectedItem = CurrentLocation.Items.Count > 0 ? CurrentLocation.Items[0] : null;
                    _currentLocation.Refreshed += HandleCurrentLocationRefresh;
                }
            }
        }
        public IItem? CurrentSelectedItem
        {
            get => _currentSelectedItem;
            set
            {
                if (_currentSelectedItem != value && (_currentLocation.Items.Contains(value) || value == null))
                {
                    _currentSelectedItem = value;
                    CurrentSelectedIndex = GetItemIndex(value);
                    CurrentSelectedItemChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        } */
        public int CurrentSelectedIndex { get; private set; }

        public AsyncEventHandler CurrentLocationChanged = new();
        public AsyncEventHandler CurrentSelectedItemChanged = new();

        public async Task Init(IContainer currentPath)
        {
            await SetCurrentLocation(currentPath);
        }

        public Task<IContainer> GetCurrentLocation()
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
                await CurrentLocationChanged?.InvokeAsync(this, AsyncEventArgs.Empty);

                var currentLocationItems = (await (await GetCurrentLocation()).GetItems())!;
                await SetCurrentSelectedItem(currentLocationItems.Count > 0 ? currentLocationItems[0] : null);
                _currentLocation.Refreshed.Add(HandleCurrentLocationRefresh);
            }
        }

        public Task<IItem?> GetCurrentSelectedItem()
        {
            return Task.FromResult(_currentSelectedItem);
        }

        public async Task SetCurrentSelectedItem(IItem? value)
        {
            if (_currentSelectedItem != value)
            {
                var contains = (await _currentLocation.GetItems())?.Contains(value) ?? false;
                if(value != null && !contains) throw new IndexOutOfRangeException("Provided item does not exists in the current container.");

                _currentSelectedItem = value;
                CurrentSelectedIndex = await GetItemIndex(value);
                await CurrentSelectedItemChanged?.InvokeAsync(this, AsyncEventArgs.Empty);
            }
        }

        private async Task HandleCurrentLocationRefresh(object? sender, AsyncEventArgs e)
        {
            var currentSelectedName = (await GetCurrentSelectedItem())?.FullName;
            var currentLocationItems = (await (await GetCurrentLocation()).GetItems())!;
            if (currentSelectedName != null)
            {
                await SetCurrentSelectedItem(currentLocationItems.FirstOrDefault(i => i.FullName == currentSelectedName) ?? currentLocationItems.FirstOrDefault());
            }
            else if (currentLocationItems.Count > 0)
            {
                await SetCurrentSelectedItem(currentLocationItems[0]);
            }
        }

        public async Task SelectFirstItem()
        {
            var currentLocationItems = (await (await GetCurrentLocation()).GetItems())!;
            if (currentLocationItems.Count > 0)
            {
                await SetCurrentSelectedItem(currentLocationItems[0]);
            }
        }

        public async Task SelectLastItem()
        {
            var currentLocationItems = (await (await GetCurrentLocation()).GetItems())!;
            if (currentLocationItems.Count > 0)
            {
                await SetCurrentSelectedItem(currentLocationItems[currentLocationItems.Count - 1]);
            }
        }

        public async Task SelectPreviousItem(int skip = 0)
        {
            var currentLocationItems = (await (await GetCurrentLocation()).GetItems())!;
            var possibleItemsToSelect = currentLocationItems.Take(CurrentSelectedIndex).Reverse().Skip(skip).ToList();

            if (possibleItemsToSelect.Count == 0) possibleItemsToSelect = currentLocationItems.ToList();
            await SelectItem(possibleItemsToSelect);
        }

        public async Task SelectNextItem(int skip = 0)
        {
            var currentLocationItems = (await (await GetCurrentLocation()).GetItems())!;
            var possibleItemsToSelect = currentLocationItems.Skip(CurrentSelectedIndex + 1 + skip).ToList();

            if (possibleItemsToSelect.Count == 0) possibleItemsToSelect = currentLocationItems.Reverse().ToList();
            await SelectItem(possibleItemsToSelect);
        }

        private async Task SelectItem(IEnumerable<IItem> currentPossibleItems)
        {
            if (!currentPossibleItems.Any()) return;

            var currentLocationItems = (await (await GetCurrentLocation()).GetItems())!;

            if (await GetCurrentSelectedItem() != null)
            {
                (await GetCurrentLocation())?.Refresh();

                IItem? newSelectedItem = null;
                foreach (var item in currentPossibleItems)
                {
                    if (currentLocationItems.FirstOrDefault(i => i.Name == item.Name) is var possibleNewSelectedItem
                        && possibleNewSelectedItem is not null)
                    {
                        newSelectedItem = possibleNewSelectedItem;
                        break;
                    }
                }

                if(newSelectedItem != null)
                {
                    newSelectedItem = (await (await GetCurrentLocation()).GetItems())?.FirstOrDefault(i => i.Name == newSelectedItem.Name);
                }

                await SetCurrentSelectedItem(newSelectedItem ?? (currentLocationItems.Count > 0 ? currentLocationItems[0] : null));
            }
            else
            {
                await SetCurrentSelectedItem(currentLocationItems.Count > 0 ? currentLocationItems[0] : null);
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
                    await SetCurrentSelectedItem(lastCurrentVirtualContainer.GetRealContainer());
                }
                else
                {
                    await SetCurrentLocation(parent);
                    var newCurrentLocation = (await (await GetCurrentLocation()).GetItems())?.FirstOrDefault(i => i.Name == lastCurrentLocation.Name);
                    await SetCurrentSelectedItem(newCurrentLocation);
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

        private async Task<int> GetItemIndex(IItem? item)
        {
            if (item == null) return -1;
            var currentLocationItems = (await (await GetCurrentLocation()).GetItems())!;

            for (var i = 0; i < currentLocationItems.Count; i++)
            {
                if (currentLocationItems[i] == item) return i;
            }

            return -1;
        }
    }
}