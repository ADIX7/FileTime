namespace FileTime.App.Core.ViewModels
{
    public interface IElementViewModel : IItemViewModel
    {
        long? Size { get; set; }
    }
}