using DeclarativeProperty;

namespace FileTime.App.Core.ViewModels.Timeline;

public interface ICommandTimeStateViewModel
{
    IDeclarativeProperty<int> TotalProgress { get; }
    IDeclarativeProperty<int> CurrentProgress { get; }
    IDeclarativeProperty<string> DisplayLabel { get; }
    IDeclarativeProperty<string> DisplayDetailLabel { get; }
    IDeclarativeProperty<bool> IsSelected { get; }
    void Cancel();
}