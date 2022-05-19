using System.Reactive.Linq;
using DynamicData;
using FileTime.App.Core.Services;
using FileTime.Core.Interactions;
using FileTime.GuiApp.ViewModels;

namespace FileTime.GuiApp.Services;

public class DialogService : IDialogService
{
    private readonly IModalService _modalService;

    public IObservable<ReadInputsViewModel?> ReadInput { get; }

    public DialogService(IModalService modalService)
    {
        _modalService = modalService;
        ReadInput = modalService
            .OpenModals
            .ToCollection()
            .Select(modals =>
                (ReadInputsViewModel?)modals.FirstOrDefault(m => m is ReadInputsViewModel)
            )
            .Publish(null)
            .RefCount();
    }

    public void ReadInputs(IEnumerable<IInputElement> inputs, Action inputHandler, Action? cancelHandler = null)
    {
        var modalViewModel = new ReadInputsViewModel(HandleReadInputsSuccess, HandleReadInputsCancel)
        {
            Inputs = inputs.ToList(),
            SuccessHandler = inputHandler,
            CancelHandler = cancelHandler
        };

        _modalService.OpenModal(modalViewModel);
    }

    private void HandleReadInputsSuccess(ReadInputsViewModel readInputsViewModel)
    {
        _modalService.CloseModal(readInputsViewModel);
        readInputsViewModel.SuccessHandler.Invoke();
    }

    private void HandleReadInputsCancel(ReadInputsViewModel readInputsViewModel)
    {
        _modalService.CloseModal(readInputsViewModel);
        readInputsViewModel.CancelHandler?.Invoke();
    }

    public Task<bool> ReadInputs(IEnumerable<IInputElement> fields)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();
        ReadInputs(fields, () => taskCompletionSource.SetResult(true), () => taskCompletionSource.SetResult(false));
        
        return taskCompletionSource.Task;
    }
}