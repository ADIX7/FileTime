using DynamicData;
using FileTime.App.Core.ViewModels;

namespace FileTime.App.Core.Services;

public interface IModalService
{
    IObservable<IChangeSet<IModalViewModel>> OpenModals { get; }

    void OpenModal(IModalViewModel modalToOpen);
    void CloseModal(IModalViewModel modalToClose);
}