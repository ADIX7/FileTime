using FileTime.Core.Command;
using FileTime.Core.Extensions;
using FileTime.Core.Models;

namespace FileTime.ConsoleUI.App
{
    public partial class Application
    {
        private void CloseTab()
        {
            var currentTabIndex = _tabs.IndexOf(_selectedTab!);
            RemoveTab(_selectedTab!);

            if (_tabs.Count > 0)
            {
                _selectedTab = _tabs[currentTabIndex == 0 ? 0 : currentTabIndex - 1];
            }
            else
            {
                _selectedTab = null;
                IsRunning = false;
            }
        }

        private async Task MoveCursorUp() => await _selectedTab!.SelectPreviousItem();
        private async Task MoveCursorDown() => await _selectedTab!.SelectNextItem();
        private async Task GoUp() => await _selectedTab!.GoUp();
        private async Task Open() => await _selectedTab!.Open();

        private async Task MoveCursorUpPage() => await _selectedTab!.SelectPreviousItem(UI.Render.PageSize);
        private async Task MoveCursorDownPage() => await _selectedTab!.SelectNextItem(UI.Render.PageSize);
        private async Task MoveCursorToTop() => await _selectedTab!.SelectFirstItem();
        private async Task MoveCursorToBottom() => await _selectedTab!.SelectLastItem();

        private async Task ToggleHidden()
        {
            const string hiddenFilterName = "filter_showhiddenelements";

            var currentLocation = await _selectedTab!.GetCurrentLocation();
            var containerToOpen = await currentLocation.ToggleVirtualContainerInChain(hiddenFilterName, GenerateHiddenFilterVirtualContainer);
            await _selectedTab.OpenContainer(containerToOpen);

            static async Task<VirtualContainer> GenerateHiddenFilterVirtualContainer(IContainer container)
            {
                var newContainer = new VirtualContainer(
                    container,
                    new List<Func<IEnumerable<IContainer>, IEnumerable<IContainer>>>()
                    {
                        container => container.Where(c => !c.IsHidden)
                    },
                    new List<Func<IEnumerable<IElement>, IEnumerable<IElement>>>()
                    {
                        element => element.Where(e => !e.IsHidden)
                    },
                    true,
                    true,
                    hiddenFilterName
                );

                await newContainer.Init();

                return newContainer;
            }
        }

        public async Task Select()
        {
            if (_selectedTab != null)
            {
                await _tabStates[_selectedTab].MarkCurrentItem();
            }
        }

        public async Task Copy()
        {
            _clipboard.Clear();
            _clipboard.SetCommand<CopyCommand>();

            var currentSelectedItems = await _tabStates[_selectedTab!].GetCurrentMarkedItems();
            if (currentSelectedItems.Count > 0)
            {
                foreach (var selectedItem in currentSelectedItems)
                {
                    _clipboard.AddContent(new AbsolutePath(selectedItem));
                }
            }
            else
            {
                var currentSelectedItem = (await _selectedTab!.GetCurrentSelectedItem())!;
                _clipboard.AddContent(new AbsolutePath(currentSelectedItem));
            }
        }

        public void Cut()
        {
            _clipboard.Clear();
            _clipboard.SetCommand<MoveCommand>();
        }

        public async Task PasteMerge()
        {
            await Paste(TransportMode.Merge);
        }
        public async Task PasteOverwrite()
        {
            await Paste(TransportMode.Overwrite);
        }

        public async Task PasteSkip()
        {
            await Paste(TransportMode.Skip);
        }

        private async Task Paste(TransportMode transportMode)
        {
            if (_clipboard.CommandType != null)
            {
                var command = (ITransportationCommand)Activator.CreateInstance(_clipboard.CommandType!)!;
                command.TransportMode = transportMode;

                command.Sources.Clear();

                foreach (var item in _clipboard.Content)
                {
                    command.Sources.Add(item);
                }

                var currentLocation = await _selectedTab!.GetCurrentLocation();
                command.Target = currentLocation is VirtualContainer virtualContainer
                    ? virtualContainer.BaseContainer
                    : currentLocation;

                await _timeRunner.AddCommand(command);

                _clipboard.Clear();
            }
        }

        private async Task CreateContainer()
        {
            var currentLocation = await _selectedTab?.GetCurrentLocation();
            if (currentLocation != null)
            {
                _coloredConsoleRenderer.ResetColor();
                MoveToIOLine(2);
                _coloredConsoleRenderer.Write("New container name: ");
                var newContainerName = await _consoleReader.ReadText(validator: Validator);

                if (!string.IsNullOrWhiteSpace(newContainerName))
                {
                    await currentLocation.CreateContainerAsync(newContainerName);
                }
            }

            async Task Validator(string newPath)
            {
                if (await currentLocation.IsExistsAsync(newPath))
                {
                    _coloredConsoleRenderer.ForegroundColor = _styles.ErrorColor;
                }
                else
                {
                    _coloredConsoleRenderer.ResetColor();
                }
            }
        }

        private async Task HardDelete()
        {
            IList<AbsolutePath>? itemsToDelete = null;

            var currentSelectedItems = new List<IItem>();
            foreach (var item in await _tabStates[_selectedTab!].GetCurrentMarkedItems())
            {
                var resolvedItem = await item.ResolveAsync();
                if (resolvedItem != null) currentSelectedItems.Add(resolvedItem);
            }

            IItem? currentSelectedItem = null;
            if (_selectedTab != null) currentSelectedItem = await _selectedTab.GetCurrentSelectedItem();

            if (currentSelectedItems.Count > 0)
            {
                var delete = true;

                //FIXME: check 'is Container'
                if (currentSelectedItems.Count == 1
                    && currentSelectedItems[0] is IContainer container
                    && (await container.GetItems())?.Count > 0)
                {
                    delete = AskForApprove($"The container '{container.Name}' is not empty.");
                }

                if (delete)
                {
                    itemsToDelete = currentSelectedItems.Cast<AbsolutePath>().ToList();
                }
            }
            else if (currentSelectedItem != null)
            {
                bool delete = true;
                if (currentSelectedItem is IContainer container && (await container.GetItems())?.Count > 0)
                {
                    delete = AskForApprove($"The container '{container.Name}' is not empty.");
                }

                if (delete)
                {
                    itemsToDelete = new List<AbsolutePath>()
                    {
                        new AbsolutePath(currentSelectedItem)
                    };
                }
            }

            if (itemsToDelete != null)
            {
                var deleteCommand = new DeleteCommand();

                foreach (var itemToDelete in itemsToDelete)
                {
                    deleteCommand.ItemsToDelete.Add(itemToDelete);
                }

                await _timeRunner.AddCommand(deleteCommand);
                _clipboard.Clear();
            }

            bool AskForApprove(string name)
            {
                MoveToIOLine(2);
                _coloredConsoleRenderer.Write(name + " Proceed to delete? (Y/N)");

                while (true)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Y)
                    {
                        break;
                    }
                    else if (key.Key == ConsoleKey.N)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}