namespace FileTime.App.Core.UserCommand;

public interface IIdentifiableUserCommand : IUserCommand
{
    string UserCommandID { get; }
}