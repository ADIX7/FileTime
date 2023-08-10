namespace FileTime.App.Core.ViewModels;

public interface IPossibleCommandEntryViewModel
{
    public string CommandName { get; }
    public string Title { get; }
    public string KeysText { get; }
}