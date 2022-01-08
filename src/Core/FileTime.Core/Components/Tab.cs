using FileTime.Core.Models;

namespace FileTime.Core.Components
{
    public class Tab
    {
        private IItem? currentSelectedItem;
        private IContainer currentLocation;

        public IContainer CurrentLocation
        {
            get => currentLocation;
            private set
            {
                if (currentLocation != value)
                {
                    if (currentLocation != null)
                    {
                        currentLocation.Refreshed -= HandleCurrentLocationRefresh;
                    }

                    currentLocation = value;
                    CurrentLocationChanged?.Invoke(this, EventArgs.Empty);
                    CurrentSelectedItem = CurrentLocation.Items.Count > 0 ? CurrentLocation.Items[0] : null;
                    currentLocation.Refreshed += HandleCurrentLocationRefresh;
                }
            }
        }
        public IItem? CurrentSelectedItem
        {
            get => currentSelectedItem;
            private set
            {
                if (currentSelectedItem != value && (currentLocation.Items.Contains(value) || value == null))
                {
                    currentSelectedItem = value;
                    CurrentSelectedIndex = GetItemIndex(value);
                }
            }
        }
        public int CurrentSelectedIndex { get; private set; }

        public event EventHandler CurrentLocationChanged;

        public Tab(IContainer currentPath)
        {
            CurrentLocation = currentPath;
            CurrentSelectedItem = CurrentLocation.Items.Count > 0 ? CurrentLocation.Items[0] : null;
        }

        private void HandleCurrentLocationRefresh(object? sender, EventArgs e)
        {
            var currentSelectedName = CurrentSelectedItem?.FullName;
            if (currentSelectedName != null)
            {
                CurrentSelectedItem = CurrentLocation.Items.FirstOrDefault(i => i.FullName == currentSelectedName) ?? currentLocation.Items.FirstOrDefault();
            }
            else if (CurrentLocation.Items.Count > 0)
            {
                CurrentSelectedItem = CurrentLocation.Items[0];
            }
        }

        public void SelectFirstItem()
        {
            if (CurrentLocation.Items.Count > 0)
            {
                CurrentSelectedItem = CurrentLocation.Items[0];
            }
        }

        public void SelectLastItem()
        {
            if (CurrentLocation.Items.Count > 0)
            {
                CurrentSelectedItem = CurrentLocation.Items[CurrentLocation.Items.Count - 1];
            }
        }

        public void SelectPreviousItem(int skip = 0)
        {
            var possibleItemsToSelect = CurrentLocation.Items.Take(CurrentSelectedIndex).Reverse().Skip(skip).ToList();

            if (possibleItemsToSelect.Count == 0) possibleItemsToSelect = CurrentLocation.Items.ToList();
            SelectItem(possibleItemsToSelect);
        }

        public void SelectNextItem(int skip = 0)
        {
            var possibleItemsToSelect = CurrentLocation.Items.Skip(CurrentSelectedIndex + 1 + skip).ToList();

            if (possibleItemsToSelect.Count == 0) possibleItemsToSelect = CurrentLocation.Items.Reverse().ToList();
            SelectItem(possibleItemsToSelect);
        }

        private void SelectItem(IEnumerable<IItem> currentPossibleItems)
        {
            if (!currentPossibleItems.Any()) return;

            if (CurrentSelectedItem != null)
            {
                CurrentLocation.Refresh();

                IItem? newSelectedItem = null;
                foreach (var item in currentPossibleItems)
                {
                    if (CurrentLocation.Items.FirstOrDefault(i => i.Name == item.Name) is var possibleNewSelectedItem
                        && possibleNewSelectedItem is not null)
                    {
                        newSelectedItem = possibleNewSelectedItem;
                        break;
                    }
                }

                CurrentSelectedItem = newSelectedItem ?? (CurrentLocation.Items.Count > 0 ? CurrentLocation.Items[0] : null);
            }
            else
            {
                CurrentSelectedItem = CurrentLocation.Items.Count > 0 ? CurrentLocation.Items[0] : null;
            }
        }

        public void GoUp()
        {
            var lastCurrentLocation = CurrentLocation;
            var parent = CurrentLocation.GetParent();

            if (parent is not null)
            {
                if (lastCurrentLocation is VirtualContainer lastCurrentVirtualContainer)
                {
                    CurrentLocation = lastCurrentVirtualContainer.CloneVirtualChainFor(parent, v => v.IsPermanent);
                    CurrentSelectedItem = lastCurrentVirtualContainer.GetRealContainer();
                }
                else
                {
                    CurrentLocation = parent;
                    CurrentSelectedItem = lastCurrentLocation;
                }
            }
        }

        public void Open()
        {
            if (currentSelectedItem is IContainer childContainer)
            {
                if (CurrentLocation is VirtualContainer currentVirtuakContainer)
                {
                    CurrentLocation = currentVirtuakContainer.CloneVirtualChainFor(childContainer, v => v.IsPermanent);
                }
                else
                {
                    CurrentLocation = childContainer;
                }
            }
        }

        public void OpenContainer(IContainer container) => CurrentLocation = container;

        private int GetItemIndex(IItem? item)
        {
            if (item == null) return -1;

            for (var i = 0; i < CurrentLocation.Items.Count; i++)
            {
                if (CurrentLocation.Items[i] == item) return i;
            }

            return -1;
        }
    }
}