using MvvmGen;

namespace FileTime.App.Core.ViewModels
{
    [ViewModel]
    public partial class ElementViewModel : ItemViewModel, IElementViewModel
    {
        [Property]
        private long? _size;
    }
}