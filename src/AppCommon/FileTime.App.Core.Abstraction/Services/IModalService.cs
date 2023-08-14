using System.Collections.ObjectModel;
using FileTime.App.Core.ViewModels;

namespace FileTime.App.Core.Services;

public interface IModalService
{
    ReadOnlyObservableCollection<IModalViewModel> OpenModals { get; }

    void OpenModal(IModalViewModel modalToOpen);
    void CloseModal(IModalViewModel modalToClose);
    T OpenModal<T>() where T : IModalViewModel;
    event EventHandler? AllModalClosed;
}