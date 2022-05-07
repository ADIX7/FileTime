using DynamicData;
using FileTime.App.Core.Command;
using FileTime.App.Core.Models;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Command;
using FileTime.Core.Command.Copy;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services.CommandHandler
{
    public class ItemManipulationCommandHandler : CommandHandlerBase
    {
        private ITabViewModel? _selectedTab;
        private IItemViewModel? _currentSelectedItem;
        private readonly ICommandHandlerService _commandHandlerService;
        private readonly IClipboardService _clipboardService;
        private BindedCollection<IAbsolutePath>? _markedItems;

        public ItemManipulationCommandHandler(
            IAppState appState,
            ICommandHandlerService commandHandlerService,
            IClipboardService clipboardService) : base(appState)
        {
            _commandHandlerService = commandHandlerService;
            _clipboardService = clipboardService;

            SaveSelectedTab(t =>
            {
                _selectedTab = t;
                _markedItems?.Dispose();
                _markedItems = t == null ? null : new BindedCollection<IAbsolutePath>(t.MarkedItems);
            });
            SaveCurrentSelectedItem(i => _currentSelectedItem = i);

            AddCommandHandlers(new (Commands, Func<Task>)[]
            {
                (Commands.Copy, Copy),
                (Commands.Mark, MarkItem),
                (Commands.PasteMerge, PasteMerge),
                (Commands.PasteOverwrite, PasteOverwrite),
                (Commands.PasteSkip, PasteSkip),
            });
        }

        private async Task MarkItem()
        {
            if (_selectedTab == null || _currentSelectedItem?.BaseItem?.FullName == null) return;

            _selectedTab.ToggleMarkedItem(new AbsolutePath(_currentSelectedItem.BaseItem));
            await _commandHandlerService.HandleCommandAsync(Commands.MoveCursorDown);
        }

        private Task Copy()
        {
            _clipboardService.Clear();
            _clipboardService.SetCommand<CopyCommand>();

            if ((_markedItems?.Collection.Count ?? 0) > 0)
            {
                foreach (var item in _markedItems!.Collection)
                {
                    _clipboardService.AddContent(item);
                }

                _selectedTab?.ClearMarkedItems();
            }
            else if (_currentSelectedItem?.BaseItem != null)
            {
                _clipboardService.AddContent(new AbsolutePath(_currentSelectedItem.BaseItem));
            }

            return Task.CompletedTask;
        }

        private async Task PasteMerge()
        {
            await Paste(TransportMode.Merge);
        }

        private async Task PasteOverwrite()
        {
            await Paste(TransportMode.Overwrite);
        }

        private async Task PasteSkip()
        {
            await Paste(TransportMode.Skip);
        }

        private Task Paste(TransportMode skip)
        {
            if (_clipboardService.CommandType is null) return Task.CompletedTask;
            return Task.CompletedTask;
        }
    }
}