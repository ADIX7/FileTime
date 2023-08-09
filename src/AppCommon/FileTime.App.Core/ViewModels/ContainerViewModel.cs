using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Models;

namespace FileTime.App.Core.ViewModels;

public partial class ContainerViewModel : ItemViewModel, IContainerViewModel
{
    public IContainer? Container => BaseItem as IContainer;

    public ContainerViewModel(IItemNameConverterService itemNameConverterService, IAppState appState) 
        : base(itemNameConverterService, appState)
    {
    }

    public void Init(IContainer item, ITabViewModel parentTab, ItemViewModelType itemViewModelType) 
        => Init((IItem)item, parentTab, itemViewModelType);
}