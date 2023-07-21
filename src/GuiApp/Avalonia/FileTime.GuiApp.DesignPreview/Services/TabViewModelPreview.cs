using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Services;

namespace FileTime.GuiApp.DesignPreview.Services;

/*public class TabViewModelPreview : ITabViewModel
{
    private static readonly ItemNameConverterService _itemNameConverterService = new();

    public TabViewModelPreview(IAppState appState)
    {
        var tab = new TabPreview();
        Tab = tab;
        TabNumber = 1;
        IsSelected = new BehaviorSubject<bool>(true);
        CurrentLocation = tab.CurrentLocation;

        IItemViewModel currentSelectedItem = tab.CurrentSelectedItemPreview is IContainer
            ? CreateCurrentSelectedItemContainer()
            : CreateCurrentSelectedItemElement();

        CurrentSelectedItem = new BehaviorSubject<IItemViewModel>(currentSelectedItem);

        CurrentItems = tab.CurrentItems
            .Select<IObservable<IChangeSet<IItem, string>>?, IObservable<IChangeSet<IItemViewModel, string>>?>(
                items => 
                    items!.Transform(i => MapItemToViewModel(i, ItemViewModelType.Main)));

        ContainerViewModel CreateCurrentSelectedItemContainer()
        {
            var vm = new ContainerViewModel(
                _itemNameConverterService,
                appState
            );
            vm.Init(
                (IContainer) tab.CurrentSelectedItemPreview,
                this,
                ItemViewModelType.Main
            );

            return vm;
        }

        ElementViewModel CreateCurrentSelectedItemElement()
        {
            var vm = new ElementViewModel(
                _itemNameConverterService,
                appState
            );

            vm.Init(
                (IElement) tab.CurrentSelectedItemPreview,
                this,
                ItemViewModelType.Main
            );

            return vm;
        }

        IItemViewModel MapItemToViewModel(IItem item, ItemViewModelType type)
        {
            if (item is IContainer container)
            {
                var containerViewModel = new ContainerViewModel(_itemNameConverterService, appState);
                containerViewModel.Init(container, this, type);

                return containerViewModel;
            }
            else if (item is IElement element)
            {
                var elementViewModel = new ElementViewModel(_itemNameConverterService, appState);
                elementViewModel.Init(element, this, type);

                return elementViewModel;
            }

            throw new Exception();
        }
    }

    public ITab? Tab { get; }
    public int TabNumber { get; }
    public IObservable<bool> IsSelected { get; }
    public IObservable<IContainer?> CurrentLocation { get; }
    public IObservable<IItemViewModel?> CurrentSelectedItem { get; }
    public IObservable<IObservable<IChangeSet<IItemViewModel, string>>?> CurrentItems { get; }
    public IObservable<IChangeSet<FullName>> MarkedItems { get; }
    public IObservable<IObservable<IChangeSet<IItemViewModel, string>>?> SelectedsChildren { get; }
    public IObservable<IObservable<IChangeSet<IItemViewModel, string>>?> ParentsChildren { get; }
    public BindedCollection<IItemViewModel, string>? CurrentItemsCollection { get; }
    public BindedCollection<IItemViewModel, string>? SelectedsChildrenCollection { get; }
    public BindedCollection<IItemViewModel, string>? ParentsChildrenCollection { get; }
    public IObservable<IReadOnlyCollection<IItemViewModel>?> CurrentItemsCollectionObservable { get; }
    public IObservable<IReadOnlyCollection<IItemViewModel>?> ParentsChildrenCollectionObservable { get; }
    public IObservable<IReadOnlyCollection<IItemViewModel>?> SelectedsChildrenCollectionObservable { get; }
    public IContainer? CachedCurrentLocation { get; }
    public void ClearMarkedItems() => throw new NotImplementedException();

    public void RemoveMarkedItem(FullName fullName) => throw new NotImplementedException();

    public void AddMarkedItem(FullName fullName) => throw new NotImplementedException();

    public void ToggleMarkedItem(FullName fullName) => throw new NotImplementedException();

    public void Init(ITab obj1, int obj2)
    {
    }

    public void Dispose()
    {
    }
}*/