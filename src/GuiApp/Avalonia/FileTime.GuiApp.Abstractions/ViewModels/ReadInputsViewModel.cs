using FileTime.App.Core.ViewModels;
using FileTime.Core.Interactions;
using MvvmGen;

namespace FileTime.GuiApp.ViewModels;

[ViewModel]
[Inject(typeof(Action<ReadInputsViewModel>), "_cancel")]
[Inject(typeof(Action<ReadInputsViewModel>), "_process")]
public partial class ReadInputsViewModel : IModalViewModelBase
{
    public string Name => "ReadInputs";
    public List<IInputElement> Inputs { get; set; }
    public Action SuccessHandler { get; set; }
    public Action? CancelHandler { get; set; }

    [Command]
    public void Process()
    {
        _process.Invoke(this);
    }

    [Command]
    public void Cancel()
    {
        _cancel.Invoke(this);
    }
}