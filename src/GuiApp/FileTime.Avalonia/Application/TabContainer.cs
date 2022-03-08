using AsyncEvent;
using FileTime.Core.Components;
using FileTime.Core.Models;
using FileTime.Providers.Local;
using FileTime.Avalonia.ViewModels;
using MvvmGen;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileTime.App.Core.Tab;
using System.Threading;
using FileTime.Core.Timeline;
using FileTime.Avalonia.ViewModels.ItemPreview;
using FileTime.Core.Search;
using Microsoft.Extensions.DependencyInjection;
using FileTime.Core.Services;
using FileTime.Core.ContainerSizeScanner;
using System.Collections.Generic;
using FileTime.Core.Providers;
using Avalonia.Threading;

namespace FileTime.Avalonia.Application
{
    [ViewModel]
    [Inject(typeof(AppState))]
    [Inject(typeof(ItemNameConverterService))]
    [Inject(typeof(LocalContentProvider))]
    [Inject(typeof(Tab))]
    [Inject(typeof(TimeRunner), propertyName: "_timeRunner")]
    [Inject(typeof(IServiceProvider), PropertyName = "_serviceProvider")]
    public partial class TabContainer : INewItemProcessor
    {
        private bool _updateFromCode;
        private CancellationTokenSource? _moveCancellationTokenSource;

        [Property]
        private TabState _tabState;

        [Property]
        private ContainerViewModel _parent;

        [Property]
        private ContainerViewModel _currentLocation;

        [Property]
        private ContainerViewModel? _childContainer;

        [Property]
        private int _tabNumber;

        [Property]
        private bool _isSelected;

        private IItemViewModel? _selectedItem;

        [Obsolete($"Use {nameof(SetSelectedItemAsync)} instead.")]
        public IItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (!_updateFromCode && value != null)
                {
                    try
                    {
                        Task.Run(async () => await SetSelectedItemAsync(value)).Wait();
                    }
                    catch (AggregateException e) when (e.InnerExceptions.Count == 1 && e.InnerExceptions[0] is IndexOutOfRangeException) { }
                }
            }
        }

        [Property]
        private IItemPreviewViewModel? _itemPreview;

        [Property]
        private List<HistoryItemViewModel> _history = new();

        public async Task SetSelectedItemAsync(IItemViewModel? value)
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;

                await Tab.SetCurrentSelectedItem(SelectedItem?.Item);
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        partial void OnInitialize()
        {
            _tabState = new TabState(Tab);
            _timeRunner.RefreshContainer.Add(TimeRunnerContainerRefreshed);
            Tab.HistoryChanged.Add(TabHistoryChanged);
        }

        private async Task TabHistoryChanged(object? sender, AsyncEventArgs e, CancellationToken token)
        {
            await RefreshHistory();
        }

        private async Task RefreshHistory()
        {
            var newHistory = new List<HistoryItemViewModel>();

            var counter = 0;
            AbsolutePath? lastPath = null;
            foreach (var history in Tab.History.Reverse())
            {
                try
                {
                    var historyItemVM = await _serviceProvider.GetAsyncInitableResolver(history).GetServiceAsync<HistoryItemViewModel>();
                    if (historyItemVM.Container != null && historyItemVM.Container is not IContentProvider && lastPath != history)
                    {
                        lastPath = history;
                        newHistory.Add(historyItemVM);
                        counter++;

                        if (counter == 10) break;
                    }
                }
                catch { }
            }

            await Dispatcher.UIThread.InvokeAsync(() => History = newHistory);
        }

        private async Task TimeRunnerContainerRefreshed(object? sender, AbsolutePath container, CancellationToken token = default)
        {
            var currentLocation = await Tab.GetCurrentLocation();
            if (currentLocation != null)
            {
                var currentLocationPath = new AbsolutePath(currentLocation);
                if (currentLocationPath == container)
                {
                    await currentLocation.RefreshAsync();
                }
            }
        }

        public async Task Init(int tabNumber)
        {
            TabNumber = tabNumber;
            Tab.CurrentLocationChanged.Add(Tab_CurrentLocationChanged);
            Tab.CurrentSelectedItemChanged.Add(Tab_CurrentSelectedItemChanged);

            var currentLocation = await Tab.GetCurrentLocation();
            var parent = GenerateParent(currentLocation);
            CurrentLocation = new ContainerViewModel(this, parent, currentLocation, ItemNameConverterService, AppState);
            await CurrentLocation.Init();

            if (parent != null)
            {
                parent.ChildrenToAdopt.Add(CurrentLocation);
                Parent = parent;
                await Parent.Init();
            }
            else
            {
                Parent = null;
            }

            await UpdateCurrentSelectedItem();
        }

        private ContainerViewModel? GenerateParent(IContainer? container, bool recursive = false)
        {
            var parentContainer = container?.GetParent();
            if (parentContainer == null) return null;
            var parentParent = recursive ? GenerateParent(parentContainer.GetParent(), recursive) : null;

            var parent = new ContainerViewModel(this, parentParent, parentContainer, ItemNameConverterService, AppState);
            parentParent?.ChildrenToAdopt.Add(parent);
            return parent;
        }

        private async Task Tab_CurrentLocationChanged(object? sender, AsyncEventArgs e, CancellationToken token = default)
        {
            var oldCurrentLocation = CurrentLocation;
            try
            {
                var currentLocation = await Tab.GetCurrentLocation(token);
                var parent = GenerateParent(currentLocation);
                CurrentLocation = new ContainerViewModel(this, parent, currentLocation, ItemNameConverterService, AppState);
                await CurrentLocation.Init(token: token);

                if (token.IsCancellationRequested) return;

                if (parent != null)
                {
                    parent.ChildrenToAdopt.Add(CurrentLocation);
                    Parent = parent;
                    await Parent.Init(token: token);
                }
                else
                {
                    Parent = null;
                }
            }
            finally
            {
                oldCurrentLocation.Unload(unloadEvents: true);
            }
        }

        private async Task Tab_CurrentSelectedItemChanged(object? sender, bool locationChanged, CancellationToken token = default)
        {
            await UpdateCurrentSelectedItem(skipCancellationWaiting: locationChanged, token: token);
        }

        public async Task UpdateCurrentSelectedItem(bool skipCancellationWaiting = false, CancellationToken token = default)
        {
            /*try
            {*/

            if (token.IsCancellationRequested) return;

            var tabCurrentSelectenItem = await Tab.GetCurrentSelectedItem();
            ContainerViewModel? newChildContainer = null;

            IItemViewModel? currentSelectenItem = null;
            if (tabCurrentSelectenItem == null)
            {
                await SetSelectedItemAsync(null);
            }
            else
            {
                currentSelectenItem = (await _currentLocation.GetItems(token)).FirstOrDefault(i => i.Item.Name == tabCurrentSelectenItem.Name);
                if (currentSelectenItem is ContainerViewModel currentSelectedContainer)
                {
                    await SetSelectedItemAsync(currentSelectedContainer);
                    newChildContainer = currentSelectenItem is ChildSearchContainer ? null : currentSelectedContainer;
                }
                else if (currentSelectenItem is ElementViewModel element)
                {
                    await SetSelectedItemAsync(element);
                }
                else
                {
                    await SetSelectedItemAsync(null);
                }
            }

            await UpdateParents(token);

            if (!skipCancellationWaiting)
            {
                try
                {
                    await Task.Delay(500, token);
                }
                catch { }
                if (token.IsCancellationRequested) return;
            }

            ChildContainer = newChildContainer;

            IItemPreviewViewModel? preview = null;
            if (currentSelectenItem == null)
            {

            }
            else if (currentSelectenItem.Item is ContainerSizeContainer sizeContainer)
            {
                var sizeContainerPreview = _serviceProvider.GetService<SizeContainerPreview>()!;
                sizeContainerPreview.Init(sizeContainer);
                preview = sizeContainerPreview;
            }
            else if (currentSelectenItem.Item is ChildSearchContainer searchContainer)
            {
                var searchContainerPreview = _serviceProvider.GetService<SearchContainerPreview>()!;
                searchContainerPreview.Init(searchContainer, _currentLocation.Container);
                preview = searchContainerPreview;
            }
            else if (currentSelectenItem.Item is ChildSearchElement searchElement)
            {
                var searchElementPreview = _serviceProvider.GetService<SearchElementPreview>()!;
                searchElementPreview.Init(searchElement, _currentLocation.Container);
                preview = searchElementPreview;
            }
            else if (currentSelectenItem is ElementViewModel elementViewModel)
            {
                var elementPreview = new ElementPreviewViewModel();
                await elementPreview.Init(elementViewModel.Element);
                preview = elementPreview;
            }

            if (ItemPreview != null)
            {
                await ItemPreview.Destroy();
            }
            ItemPreview = preview;
            /*}
            catch
            {
                //INFO collection modified exception on: currentSelectenItem = (await _currentLocation.GetItems()).FirstOrDefault(i => i.Item.Name == tabCurrentSelectenItem.Name);
                //TODO: handle or error message
            }*/

            async Task UpdateParents(CancellationToken token = default)
            {
                var items = await _currentLocation.GetItems(token);
                if (items?.Count > 0)
                {
                    foreach (var item in items)
                    {
                        var isSelected = item == currentSelectenItem;
                        item.IsSelected = isSelected;

                        if (isSelected)
                        {
                            var parent = item.Parent;
                            while (parent != null)
                            {
                                parent.IsSelected = true;
                                parent = parent.Parent;
                            }

                            try
                            {
                                var child = item;
                                while (child is ContainerViewModel containerViewModel && containerViewModel.Container.IsLoaded)
                                {
                                    if (token.IsCancellationRequested) return;
                                    var activeChildItem = await Tab.GetItemByLastPath(containerViewModel.Container);
                                    child = (await containerViewModel.GetItems(token)).FirstOrDefault(i => i.Item == activeChildItem);
                                    if (child != null)
                                    {
                                        child.IsSelected = true;
                                    }
                                }
                            }
                            catch
                            {
                                //INFO collection modified exception on: child = (await containerViewModel.GetItems()).FirstOrDefault(i => i.Item == activeChildItem);
                                //TODO: handle or error message
                            }
                        }
                    }
                }
                else
                {
                    var parent = _currentLocation;
                    while (parent != null)
                    {
                        parent.IsSelected = true;
                        parent = parent.Parent;
                    }
                }
            }
        }

        public async Task SetCurrentSelectedItem(IItem newItem)
        {
            try
            {
                await Tab.SetCurrentSelectedItem(newItem);
            }
            catch { }
        }

        private async Task RunFromCode(Func<Task> task)
        {
            _updateFromCode = true;
            try
            {
                await task();
            }
            catch
            {
                throw;
            }
            finally
            {
                _updateFromCode = false;
            }
        }

        private CancellationToken CancelAndGenerateNextMovementToken()
        {
            if (_moveCancellationTokenSource != null) _moveCancellationTokenSource.Cancel();
            _moveCancellationTokenSource = new CancellationTokenSource();
            return _moveCancellationTokenSource.Token;
        }

        public async Task Open()
        {
            await RunFromCode(Tab.Open);
        }

        public async Task GoUp()
        {
            await RunFromCode(Tab.GoUp);
        }

        public async Task MoveCursorDown()
        {
            await RunFromCode(async () => await Tab.SelectNextItem(token: CancelAndGenerateNextMovementToken()));
        }

        public async Task MoveCursorDownPage()
        {
            await RunFromCode(async () => await Tab.SelectNextItem(10, token: CancelAndGenerateNextMovementToken()));
        }

        public async Task MoveCursorUp()
        {
            await RunFromCode(async () => await Tab.SelectPreviousItem(token: CancelAndGenerateNextMovementToken()));
        }

        public async Task MoveCursorUpPage()
        {
            await RunFromCode(async () => await Tab.SelectPreviousItem(10, token: CancelAndGenerateNextMovementToken()));
        }

        public async Task MoveCursorToFirst()
        {
            await RunFromCode(async () => await Tab.SelectFirstItem(token: CancelAndGenerateNextMovementToken()));
        }

        public async Task MoveCursorToLast()
        {
            await RunFromCode(async () => await Tab.SelectLastItem(token: CancelAndGenerateNextMovementToken()));
        }

        public async Task GotToProvider()
        {
            await RunFromCode(Tab.GoToProvider);
        }

        public async Task GotToRoot()
        {
            await RunFromCode(Tab.GoToRoot);
        }

        public async Task GotToHome()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace(Path.DirectorySeparatorChar, Constants.SeparatorChar);
            var resolvedPath = await LocalContentProvider.GetByPath(path);
            if (resolvedPath is IContainer homeFolder)
            {
                await OpenContainer(homeFolder);
            }
        }

        public async Task CreateContainer(string name)
        {
            await RunFromCode(async () =>
            {
                var currentLocation = await Tab.GetCurrentLocation();
                if (currentLocation != null)
                {
                    await currentLocation.CreateContainerAsync(name);
                }
            });
        }

        public async Task CreateElement(string name)
        {
            await RunFromCode(async () =>
            {
                var currentLocation = await Tab.GetCurrentLocation();
                if (currentLocation != null)
                {
                    await currentLocation.CreateElementAsync(name);
                }
            });
        }

        public async Task OpenContainer(IContainer container)
        {
            await RunFromCode(async () => await Tab.OpenContainer(container));
        }

        public async Task MarkCurrentItem()
        {
            await _tabState.MarkCurrentItem();
        }

        public async Task UpdateMarkedItems(ContainerViewModel containerViewModel, CancellationToken token = default)
        {
            if (containerViewModel == CurrentLocation && containerViewModel.Container.IsLoaded)
            {
                if (token.IsCancellationRequested) return;
                var selectedItems = TabState.GetCurrentMarkedItems(containerViewModel.Container);

                foreach (var item in await containerViewModel.GetItems(token))
                {
                    item.IsMarked = selectedItems.Any(c => c.Path == item.Item.FullName);
                }
            }
        }
    }
}
