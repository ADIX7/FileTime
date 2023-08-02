using DeclarativeProperty;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using MvvmGen;

namespace FileTime.App.Core.ViewModels;

[ViewModel(GenerateConstructor = false)]
public partial class ElementViewModel : ItemViewModel, IElementViewModel
{
    public IElement? Element => BaseItem as Element;

    public ElementViewModel(IItemNameConverterService itemNameConverterService, IAppState appState) : base(itemNameConverterService, appState)
    {
    }

    public void Init(IElement item, ITabViewModel parentTab, ItemViewModelType itemViewModelType) 
        => Init((IItem)item, parentTab, itemViewModelType);

    public IDeclarativeProperty<long> Size { get; protected set; } = new DeclarativeProperty<long>(0);
}