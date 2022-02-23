using FileTime.Core.Models;

namespace FileTime.Avalonia.ViewModels.ItemPreview
{
    public interface ISizeItemViewModel
    {
        string? Name { get; }
        long? Size { get; }
        IItem? Item { get; }
    }
}