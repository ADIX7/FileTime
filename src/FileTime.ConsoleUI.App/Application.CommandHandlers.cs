using FileTime.ConsoleUI.App.UI.Color;
using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.ConsoleUI.App
{
    public partial class Application
    {
        private void CloseTab()
        {
            var currentTabIndex = _panes.IndexOf(_selectedTab!);
            RemoveTab(_selectedTab!);

            if (_panes.Count > 0)
            {
                _selectedTab = _panes[currentTabIndex == 0 ? 0 : currentTabIndex - 1];
            }
            else
            {
                _selectedTab = null;
                IsRunning = false;
            }
        }

        private void MoveCursorUp() => _selectedTab!.SelectPreviousItem();
        private void MoveCursorDown() => _selectedTab!.SelectNextItem();
        private void GoUp() => _selectedTab!.GoUp();
        private void Open() => _selectedTab!.Open();

        private void MoveCursorUpPage() => _selectedTab!.SelectPreviousItem(_renderers[_selectedTab].PageSize);
        private void MoveCursorDownPage() => _selectedTab!.SelectNextItem(_renderers[_selectedTab].PageSize);
        private void MoveCursorToTop() => _selectedTab!.SelectFirstItem();
        private void MoveCursorToBottom() => _selectedTab!.SelectLastItem();

        private void ToggleHidden()
        {
            const string hiddenFilterName = "filter_showhiddenelements";

            IContainer containerToOpen = _selectedTab!.CurrentLocation;

            if (_selectedTab.CurrentLocation is VirtualContainer oldVirtualContainer)
            {
                containerToOpen = oldVirtualContainer.HasWithName(hiddenFilterName)
                ? oldVirtualContainer.ExceptWithName(hiddenFilterName)
                : GenerateHiddenFilterVirtualContainer(_selectedTab.CurrentLocation);
            }
            else
            {
                containerToOpen = GenerateHiddenFilterVirtualContainer(_selectedTab.CurrentLocation);
            }

            _selectedTab.OpenContainer(containerToOpen);

            static VirtualContainer GenerateHiddenFilterVirtualContainer(IContainer container)
            {
                return new VirtualContainer(
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
            }
        }

        public void Select()
        {
            if (_selectedTab!.CurrentSelectedItem != null)
            {
                var currentSelectedItem = _selectedTab.CurrentSelectedItem;
                if (_paneStates[_selectedTab].ContainsSelectedItem(currentSelectedItem.Provider, _selectedTab.CurrentLocation, currentSelectedItem.FullName!))
                {
                    _paneStates[_selectedTab].RemoveSelectedItem(currentSelectedItem.Provider, _selectedTab.CurrentLocation, currentSelectedItem.FullName!);
                }
                else
                {
                    _paneStates[_selectedTab].AddSelectedItem(currentSelectedItem.Provider, _selectedTab.CurrentLocation, currentSelectedItem.FullName!);
                }

                _selectedTab.SelectNextItem();
            }
        }

        public void Copy()
        {
            _clipboard.Clear();
            _clipboard.SetCommand<CopyCommand>();

            if (_paneStates[_selectedTab!].GetCurrentSelectedItems().Count > 0)
            {
                foreach (var selectedItem in _paneStates[_selectedTab!].GetCurrentSelectedItems())
                {
                    _clipboard.AddContent(selectedItem.ContentProvider, selectedItem.Path);
                }
            }
            else
            {
                _clipboard.AddContent(_selectedTab!.CurrentSelectedItem!.Provider, _selectedTab.CurrentSelectedItem.FullName!);
            }
        }

        public void Cut()
        {
            _clipboard.Clear();
            _clipboard.SetCommand<MoveCommand>();
        }

        public void PasteMerge()
        {
            Paste(TransportMode.Merge);
        }
        public void PasteOverwrite()
        {
            Paste(TransportMode.Overwrite);
        }

        public void PasteSkip()
        {
            Paste(TransportMode.Skip);
        }

        private void Paste(TransportMode transportMode)
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

                command.Target = _selectedTab.CurrentLocation is VirtualContainer virtualContainer
                    ? virtualContainer.BaseContainer
                    : _selectedTab.CurrentLocation;

                _commandExecutor.ExecuteCommand(command);

                _clipboard.Clear();
            }
        }

        private void CreateContainer()
        {
            if (_selectedTab?.CurrentLocation != null)
            {
                _coloredConsoleRenderer.ResetColor();
                MoveToIOLine(2);
                _coloredConsoleRenderer.Write("New container name: ");
                var newContainerName = _consoleReader.ReadText(validator: Validator);

                if (!string.IsNullOrWhiteSpace(newContainerName))
                {
                    _selectedTab.CurrentLocation.CreateContainer(newContainerName);
                }
            }

            void Validator(string newPath)
            {
                if (_selectedTab!.CurrentLocation.IsExists(newPath))
                {
                    _coloredConsoleRenderer.ForegroundColor = _styles.ErrorColor;
                }
                else
                {
                    _coloredConsoleRenderer.ResetColor();
                }
            }
        }

        private void HardDelete()
        {
            IList<IAbsolutePath> itemsToDelete = null;

            if (_paneStates[_selectedTab!].GetCurrentSelectedItems().Count > 0)
            {
                var delete = true;

                if (_paneStates[_selectedTab!].GetCurrentSelectedItems().Count == 1
                    && _paneStates[_selectedTab!].GetCurrentSelectedItems()[0] is IContainer container
                    && container.Items.Count > 0)
                {
                    delete = AskForApprove($"The container '{container.Name}' is not empty.");
                }

                if (delete)
                {
                    itemsToDelete = _paneStates[_selectedTab].GetCurrentSelectedItems().Cast<IAbsolutePath>().ToList();
                }
            }
            else if (_selectedTab?.CurrentSelectedItem != null)
            {
                bool delete = true;
                if (_selectedTab?.CurrentSelectedItem is IContainer container && container.Items.Count > 0)
                {
                    delete = AskForApprove($"The container '{container.Name}' is not empty.");
                }

                if (delete)
                {
                    itemsToDelete = new List<IAbsolutePath>()
                    {
                        new AbsolutePath(_selectedTab.CurrentSelectedItem.Provider, _selectedTab.CurrentSelectedItem.FullName!)
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

                _commandExecutor.ExecuteCommand(deleteCommand);
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