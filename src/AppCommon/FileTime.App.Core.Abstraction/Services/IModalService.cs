using DynamicData;
using FileTime.App.Core.ViewModels;

namespace FileTime.App.Core.Services;

public interface IModalService
{
    IObservable<IChangeSet<IModalViewModelBase>> OpenModals { get; }

    void OpenModal(IModalViewModelBase modalToOpen);
    void CloseModal(IModalViewModelBase modalToClose);
}