using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using MvvmGen;

namespace FileTime.App.Core.ViewModels;

[ViewModel(GenerateConstructor = false)]
public partial class ContainerSizeContainerViewModel : ItemViewModel, IContainerSizeContainerViewModel
{
    [Property]
    private long _size;

    public ContainerSizeContainerViewModel(IItemNameConverterService itemNameConverterService, IAppState appState) : base(itemNameConverterService, appState)
    {
    }

    public void Init(IContainer item, ITabViewModel parentTab, ItemViewModelType itemViewModelType) 
        => Init((IItem)item, parentTab, itemViewModelType);
}