using FileTime.App.Core.Command;
using FileTime.App.Core.ViewModels;

namespace FileTime.App.Core.Services.CommandHandler
{
    public class ItemManipulationCommandHandler : CommandHanderBase
    {
        private ITabViewModel? _selectedTab;
        private IItemViewModel? _currentSelectedItem;
        private readonly ICommandHandlerService _commandHandlerService;

        public ItemManipulationCommandHandler(IAppState appState, ICommandHandlerService commandHandlerService) : base(appState)
        {
            _commandHandlerService = commandHandlerService;

            SaveSelectedTab(t => _selectedTab = t);
            SaveCurrentSelectedItem(i => _currentSelectedItem = i);

            AddCommandHandlers(new (Commands, Func<Task>)[]
            {
                (Commands.Mark, MarkItem)
            });
        }

        private async Task MarkItem()
        {
            if (_selectedTab == null || _currentSelectedItem?.BaseItem?.FullName == null) return;

            _selectedTab.ToggleMarkedItem(_currentSelectedItem.BaseItem.FullName);
            await _commandHandlerService.HandleCommandAsync(Commands.MoveCursorDown);
        }
    }
}