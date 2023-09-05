using DynamicData;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Interactions;
using ObservableComputations;

namespace FileTime.App.Core.Services;

public abstract class DialogServiceBase : IDialogServiceBase
{
    private readonly IModalService _modalService;
    private readonly OcConsumer _readInputConsumer = new();
    private readonly OcConsumer _lastMessageBoxConsumer = new();
    public ScalarComputing<ReadInputsViewModel?> ReadInput { get; }
    public ScalarComputing<MessageBoxViewModel?> LastMessageBox { get; }

    protected DialogServiceBase(IModalService modalService)
    {
        _modalService = modalService;
        ReadInput = modalService
            .OpenModals
            .OfTypeComputing<ReadInputsViewModel>()
            .FirstComputing()
            .For(_readInputConsumer)!;

        LastMessageBox =
            modalService
                .OpenModals
                .OfTypeComputing<MessageBoxViewModel>()
                .LastComputing()
                .For(_lastMessageBoxConsumer)!;
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

    public abstract void ShowToastMessage(string text);

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