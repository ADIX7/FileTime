using FileTime.App.Core.ViewModels;
using FileTime.Core.Interactions;
using MvvmGen;

namespace FileTime.GuiApp.ViewModels;

[ViewModel]
public partial class MessageBoxViewModel : IModalViewModel
{
    private readonly Action<MessageBoxViewModel, MessageBoxResult> _handler;
    public string Text { get; }
    public string Name => "MessageBoxViewModel";

    public MessageBoxViewModel(string text, Action<MessageBoxViewModel, MessageBoxResult> handler)
    {
        _handler = handler;
        Text = text;
    }

    [Command]
    public void Ok()
    {
        _handler.Invoke(this, MessageBoxResult.Ok);
    }

    [Command]
    public void Cancel()
    {
        _handler.Invoke(this, MessageBoxResult.Cancel);
    }
}