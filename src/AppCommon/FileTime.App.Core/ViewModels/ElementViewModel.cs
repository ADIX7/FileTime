using DeclarativeProperty;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.Core.Models;

namespace FileTime.App.Core.ViewModels;

public class ElementViewModel : ItemViewModel, IElementViewModel
{
    private readonly DeclarativeProperty<long> _size = new(0);
    public IElement? Element => BaseItem as Element;

    public ElementViewModel(IItemNameConverterService itemNameConverterService, IAppState appState) : base(itemNameConverterService, appState)
    {
    }

    public void Init(IElement item, ITabViewModel parentTab, ItemViewModelType itemViewModelType)
    {
        Init((IItem) item, parentTab, itemViewModelType);
        _size.SetValueSafe(item.Size);
    }

    public IDeclarativeProperty<long> Size => _size;
}