using MvvmGen;

namespace FileTime.App.Core.ViewModels
{
    [ViewModel]
    public partial class ContainerSizeContainerViewModel : ItemViewModel, IContainerSizeContainerViewModel
    {
        [Property]
        private long _size;
    }
}