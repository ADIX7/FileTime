using System.Collections.ObjectModel;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Interactions;

namespace FileTime.GuiApp.App.ViewModels;

public class ReadInputsViewModel : IModalViewModel
{
    public string Name => "ReadInputs";
    public required List<IInputElement> Inputs { get; init; }
    public required Action<ReadInputsViewModel> SuccessHandler { get; init; }
    public required Action<ReadInputsViewModel>? CancelHandler { get; init; }
    public ObservableCollection<IPreviewElement> Previews { get; } = new();

    public ReadInputsViewModel()
    {
    }

    public ReadInputsViewModel(
        List<IInputElement> inputs, 
        Action<ReadInputsViewModel> successHandler, 
        Action<ReadInputsViewModel>? cancelHandler = null)
    {
        Inputs = inputs;
        SuccessHandler = successHandler;
        CancelHandler = cancelHandler;
    }

    public void Process() => SuccessHandler.Invoke(this);

    public void Cancel() => CancelHandler?.Invoke(this);
}