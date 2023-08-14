using FileTime.Core.Interactions;

namespace FileTime.App.Core.ViewModels;

public class MessageBoxViewModel : IModalViewModel
{
    private readonly Action<MessageBoxViewModel, MessageBoxResult> _handler;
    public string Text { get; }
    public bool ShowCancel { get; }
    public string OkText { get; }
    public string CancelText { get; }
    public string Name => "MessageBoxViewModel";

    public MessageBoxViewModel(
        string text, 
        Action<MessageBoxViewModel, MessageBoxResult> handler, 
        bool showCancel = true, 
        string? okText = null, 
        string? cancelText = null)
    {
        _handler = handler;
        Text = text;
        ShowCancel = showCancel;
        OkText = okText ?? "Yes";
        CancelText = cancelText ?? "No";
    }

    public void Ok() => _handler.Invoke(this, MessageBoxResult.Ok);

    public void Cancel() => _handler.Invoke(this, MessageBoxResult.Cancel);
}