using System.Reactive.Linq;
using Avalonia.Threading;
using DynamicData;
using FileTime.App.Core.Services;
using FileTime.Core.Interactions;
using FileTime.GuiApp.ViewModels;

namespace FileTime.GuiApp.Services;

public class DialogService : IDialogService
{
    private readonly IModalService _modalService;
    private readonly IGuiAppState _guiAppState;

    public IObservable<ReadInputsViewModel?> ReadInput { get; }
    public IObservable<MessageBoxViewModel?> LastMessageBox { get; }

    public DialogService(IModalService modalService, IGuiAppState guiAppState)
    {
        _modalService = modalService;
        _guiAppState = guiAppState;
        ReadInput = modalService
            .OpenModals
            .ToCollection()
            .Select(modals =>
                (ReadInputsViewModel?) modals.FirstOrDefault(m => m is ReadInputsViewModel)
            )
            .Publish(null)
            .RefCount();

        LastMessageBox =
            modalService
                .OpenModals
                .Filter(m => m is MessageBoxViewModel)
                .Transform(m => (MessageBoxViewModel) m)
                .ToCollection()
                .Select(m => m.LastOrDefault());
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

    public void ShowToastMessage(string text)
    {
        Task.Run(async () =>
        {
            await Dispatcher.UIThread.InvokeAsync(() => _guiAppState.PopupTexts.Add(text));
            await Task.Delay(5000);
            await Dispatcher.UIThread.InvokeAsync(() => _guiAppState.PopupTexts.Remove(text));
        });
    }

    public Task<MessageBoxResult> ShowMessageBox(string text)
    {
        var taskCompletionSource = new TaskCompletionSource<MessageBoxResult>();
        _modalService.OpenModal(new MessageBoxViewModel(text, (vm, result) =>
        {
            _modalService.CloseModal(vm);
            taskCompletionSource.SetResult(result);
        }));

        return taskCompletionSource.Task;
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

    public Task<bool> ReadInputs(params IInputElement[] fields)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();
        ReadInputs(fields, () => taskCompletionSource.SetResult(true), () => taskCompletionSource.SetResult(false));

        return taskCompletionSource.Task;
    }
}