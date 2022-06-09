using DynamicData;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;

namespace FileTime.GuiApp.Services;

public class ModalService : IModalService
{
    private readonly SourceList<IModalViewModel> _openModals = new();
    public IObservable<IChangeSet<IModalViewModel>> OpenModals { get; }

    public ModalService()
    {
        OpenModals = _openModals.Connect().StartWithEmpty();
    }

    public void OpenModal(IModalViewModel modalToOpen) => _openModals.Add(modalToOpen);

    public void CloseModal(IModalViewModel modalToClose) => _openModals.Remove(modalToClose);
}