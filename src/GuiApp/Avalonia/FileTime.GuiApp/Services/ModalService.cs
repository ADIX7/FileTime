using DynamicData;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.GuiApp.Services;

public class ModalService : IModalService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SourceList<IModalViewModel> _openModals = new();
    public IObservable<IChangeSet<IModalViewModel>> OpenModals { get; }
    public event EventHandler? AllModalClosed;

    public ModalService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        OpenModals = _openModals.Connect().StartWithEmpty();
    }

    public void OpenModal(IModalViewModel modalToOpen) => _openModals.Add(modalToOpen);

    public void CloseModal(IModalViewModel modalToClose)
    {
        _openModals.Remove(modalToClose);
        if (_openModals.Count == 0)
        {
            AllModalClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    public T OpenModal<T>() where T : IModalViewModel
    {
        var modal = _serviceProvider.GetRequiredService<T>();
        OpenModal(modal);

        return modal;
    }
}