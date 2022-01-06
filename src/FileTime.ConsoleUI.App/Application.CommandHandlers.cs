using FileTime.ConsoleUI.App.UI.Color;
using FileTime.Core.Command;
using FileTime.Core.Models;

namespace FileTime.ConsoleUI.App
{
    public partial class Application
    {
        private void ClosePane()
        {
            var currentPaneIndex = _panes.IndexOf(_selectedPane!);
            RemovePane(_selectedPane!);

            if (_panes.Count > 0)
            {
                _selectedPane = _panes[currentPaneIndex == 0 ? 0 : currentPaneIndex - 1];
            }
            else
            {
                _selectedPane = null;
                IsRunning = false;
            }
        }

        private void MoveCursorUp() => _selectedPane!.SelectPreviousItem();
        private void MoveCursorDown() => _selectedPane!.SelectNextItem();
        private void GoUp() => _selectedPane!.GoUp();
        private void Open() => _selectedPane!.Open();

        private void MoveCursorUpPage() => _selectedPane!.SelectPreviousItem(_renderers[_selectedPane].PageSize);
        private void MoveCursorDownPage() => _selectedPane!.SelectNextItem(_renderers[_selectedPane].PageSize);
        private void MoveCursorToTop() => _selectedPane!.SelectFirstItem();
        private void MoveCursorToBottom() => _selectedPane!.SelectLastItem();

        private void ToggleHidden()
        {
            const string hiddenFilterName = "filter_showhiddenelements";

            IContainer containerToOpen = _selectedPane!.CurrentLocation;

            if (_selectedPane.CurrentLocation is VirtualContainer oldVirtualContainer)
            {
                containerToOpen = oldVirtualContainer.HasWithName(hiddenFilterName)
                ? oldVirtualContainer.ExceptWithName(hiddenFilterName)
                : GenerateHiddenFilterVirtualContainer(_selectedPane.CurrentLocation);
            }
            else
            {
                containerToOpen = GenerateHiddenFilterVirtualContainer(_selectedPane.CurrentLocation);
            }

            _selectedPane.OpenContainer(containerToOpen);

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
            if (_selectedPane!.CurrentSelectedItem != null)
            {
                var currentSelectedItem = _selectedPane.CurrentSelectedItem;
                if (_paneStates[_selectedPane].ContainsSelectedItem(currentSelectedItem.Provider, _selectedPane.CurrentLocation, currentSelectedItem.FullName!))
                {
                    _paneStates[_selectedPane].RemoveSelectedItem(currentSelectedItem.Provider, _selectedPane.CurrentLocation, currentSelectedItem.FullName!);
                }
                else
                {
                    _paneStates[_selectedPane].AddSelectedItem(currentSelectedItem.Provider, _selectedPane.CurrentLocation, currentSelectedItem.FullName!);
                }

                _selectedPane.SelectNextItem();
            }
        }

        public void Copy()
        {
            _clipboard.Clear();
            _clipboard.SetCommand<CopyCommand>();

            if (_paneStates[_selectedPane!].GetCurrentSelectedItems().Count > 0)
            {
                foreach (var selectedItem in _paneStates[_selectedPane!].GetCurrentSelectedItems())
                {
                    _clipboard.AddContent(selectedItem.ContentProvider, selectedItem.Path);
                }
            }
            else
            {
                _clipboard.AddContent(_selectedPane!.CurrentSelectedItem!.Provider, _selectedPane.CurrentSelectedItem.FullName!);
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

                command.Target = _selectedPane.CurrentLocation is VirtualContainer virtualContainer
                    ? virtualContainer.BaseContainer
                    : _selectedPane.CurrentLocation;

                _commandExecutor.ExecuteCommand(command);

                _clipboard.Clear();
            }
        }

        private void CreateContainer()
        {
            if (_selectedPane?.CurrentLocation != null)
            {
                _coloredConsoleRenderer.ResetColor();
                MoveToIOLine(2);
                _coloredConsoleRenderer.Write("New container name: ");
                var newContainerName = _consoleReader.ReadText(validator: Validator);

                if (!string.IsNullOrWhiteSpace(newContainerName))
                {
                    _selectedPane.CurrentLocation.CreateContainer(newContainerName);
                }
            }

            void Validator(string newPath)
            {
                if (_selectedPane!.CurrentLocation.IsExists(newPath))
                {
                    _coloredConsoleRenderer.ForegroundColor = AnsiColor.From8bit(1);
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

            if (_paneStates[_selectedPane!].GetCurrentSelectedItems().Count > 0)
            {
                var delete = true;

                if (_paneStates[_selectedPane!].GetCurrentSelectedItems().Count == 1
                    && _paneStates[_selectedPane!].GetCurrentSelectedItems()[0] is IContainer container
                    && container.Items.Count > 0)
                {
                    delete = AskForApprove($"The container '{container.Name}' is not empty.");
                }

                if (delete)
                {
                    itemsToDelete = _paneStates[_selectedPane].GetCurrentSelectedItems().Cast<IAbsolutePath>().ToList();
                }
            }
            else if (_selectedPane?.CurrentSelectedItem != null)
            {
                bool delete = true;
                if (_selectedPane?.CurrentSelectedItem is IContainer container && container.Items.Count > 0)
                {
                    delete = AskForApprove($"The container '{container.Name}' is not empty.");
                }

                if (delete)
                {
                    itemsToDelete = new List<IAbsolutePath>()
                    {
                        new AbsolutePath(_selectedPane.CurrentSelectedItem.Provider, _selectedPane.CurrentSelectedItem.FullName!)
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