using DynamicData;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;

namespace FileTime.GuiApp.Services;

public class ModalService : IModalService
{
    private readonly SourceList<IModalViewModelBase> _openModals = new();
    public IObservable<IChangeSet<IModalViewModelBase>> OpenModals { get; }

    public ModalService()
    {
        OpenModals = _openModals.Connect();
    }
    
    public void OpenModal(IModalViewModelBase modalToOpen) => _openModals.Add(modalToOpen);

    public void CloseModal(IModalViewModelBase modalToClose) => _openModals.Remove(modalToClose);
}