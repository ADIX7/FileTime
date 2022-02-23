using FileTime.Core.ContainerSizeScanner;
using FileTime.Core.Models;
using MvvmGen;

namespace FileTime.Avalonia.ViewModels.ItemPreview
{
    [ViewModel]
    public partial class SizeElementViewmodel : ISizeItemViewModel
    {
        private ContainerSizeElement? _sizeElement;
        public string? Name { get; private set; }
        public long? Size { get; private set; }

        public IItem? Item => _sizeElement;

        public void Init(ContainerSizeElement sizeElement)
        {
            _sizeElement = sizeElement;

            Name = sizeElement.DisplayName;
            Size = sizeElement.Size;
        }
    }
}