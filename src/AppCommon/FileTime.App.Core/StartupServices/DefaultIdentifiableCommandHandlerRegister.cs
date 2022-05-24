using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;

namespace FileTime.App.Core.StartupServices;

public class DefaultIdentifiableCommandHandlerRegister : IStartupHandler
{
    private readonly IIdentifiableUserCommandService _service;

    public DefaultIdentifiableCommandHandlerRegister(IIdentifiableUserCommandService service)
    {
        _service = service;

        AddUserCommand(CloseTabCommand.Instance);
        AddUserCommand(CopyCommand.Instance);
        AddUserCommand(CreateContainer.Instance);
        AddUserCommand(CreateElement.Instance);
        AddUserCommand(EnterRapidTravelCommand.Instance);
        AddUserCommand(ExitRapidTravelCommand.Instance);
        AddUserCommand(GoUpCommand.Instance);
        AddUserCommand(MarkCommand.Instance);
        AddUserCommand(MoveCursorDownCommand.Instance);
        AddUserCommand(MoveCursorUpCommand.Instance);
        AddUserCommand(OpenSelectedCommand.Instance);
        AddUserCommand(PasteCommand.Merge);
        AddUserCommand(PasteCommand.Overwrite);
        AddUserCommand(PasteCommand.Skip);
        AddUserCommand(RefreshCommand.Instance);
        AddUserCommand(SwitchToTabCommand.SwitchToLastTab);
        AddUserCommand(SwitchToTabCommand.SwitchToTab1);
        AddUserCommand(SwitchToTabCommand.SwitchToTab2);
        AddUserCommand(SwitchToTabCommand.SwitchToTab3);
        AddUserCommand(SwitchToTabCommand.SwitchToTab4);
        AddUserCommand(SwitchToTabCommand.SwitchToTab5);
        AddUserCommand(SwitchToTabCommand.SwitchToTab6);
        AddUserCommand(SwitchToTabCommand.SwitchToTab7);
        AddUserCommand(SwitchToTabCommand.SwitchToTab8);
    }

    private void AddUserCommand(IIdentifiableUserCommand command)
        => _service.AddIdentifiableUserCommandFactory(command.UserCommandID, () => command);
}