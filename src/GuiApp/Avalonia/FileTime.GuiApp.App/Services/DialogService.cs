using System.Reactive.Linq;
using Avalonia.Threading;
using DynamicData;
using FileTime.App.Core.Services;
using FileTime.Core.Interactions;
using FileTime.GuiApp.App.ViewModels;

namespace FileTime.GuiApp.App.Services;

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

    private void ReadInputs(
        IEnumerable<IInputElement> inputs,
        Action inputHandler,
        Action? cancelHandler = null,
        IEnumerable<IPreviewElement>? previews = null)
    {
        var modalViewModel = new ReadInputsViewModel
        {
            Inputs = inputs.ToList(),
            SuccessHandler = HandleReadInputsSuccess,
            CancelHandler = HandleReadInputsCancel
        };

        if (previews is not null)
        {
            modalViewModel.Previews.AddRange(previews);
        }

        _modalService.OpenModal(modalViewModel);

        void HandleReadInputsSuccess(ReadInputsViewModel readInputsViewModel)
        {
            _modalService.CloseModal(readInputsViewModel);
            inputHandler();
        }

        void HandleReadInputsCancel(ReadInputsViewModel readInputsViewModel)
        {
            _modalService.CloseModal(readInputsViewModel);
            cancelHandler?.Invoke();
        }
    }

    public Task<bool> ReadInputs(IEnumerable<IInputElement> fields, IEnumerable<IPreviewElement>? previews = null)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();
        ReadInputs(
            fields,
            () => taskCompletionSource.SetResult(true),
            () => taskCompletionSource.SetResult(false),
            previews
        );

        return taskCompletionSource.Task;
    }


    public Task<bool> ReadInputs(params IInputElement[] fields)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();
        ReadInputs(
            fields,
            () => taskCompletionSource.SetResult(true),
            () => taskCompletionSource.SetResult(false)
        );

        return taskCompletionSource.Task;
    }

    public Task<bool> ReadInputs(IInputElement field, IEnumerable<IPreviewElement>? previews = null)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();
        ReadInputs(
            new[] {field},
            () => taskCompletionSource.SetResult(true),
            () => taskCompletionSource.SetResult(false),
            previews
        );

        return taskCompletionSource.Task;
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

    public Task<MessageBoxResult> ShowMessageBox(
        string text,
        bool showCancel = true,
        string? okText = null,
        string? cancelText = null)
    {
        var taskCompletionSource = new TaskCompletionSource<MessageBoxResult>();
        _modalService.OpenModal(
            new MessageBoxViewModel(
                text, 
                (vm, result) =>
                {
                    _modalService.CloseModal(vm);
                    taskCompletionSource.SetResult(result);
                },
                showCancel,
                okText,
                cancelText
            )
        );

        return taskCompletionSource.Task;
    }
}