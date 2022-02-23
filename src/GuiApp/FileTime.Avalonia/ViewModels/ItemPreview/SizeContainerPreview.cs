using AsyncEvent;
using FileTime.Avalonia.Models;
using FileTime.Core.ContainerSizeScanner;
using FileTime.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace FileTime.Avalonia.ViewModels.ItemPreview
{
    [ViewModel]
    [Inject(typeof(IServiceProvider), PropertyName = "_serviceProvider")]
    public partial class SizeContainerPreview : IItemPreviewViewModel
    {
        private readonly BehaviorSubject<object?> _update = new BehaviorSubject<object?>(null);
        private IEnumerable<ISizeItemViewModel>? _allItems;

        [Property]
        private IObservable<IEnumerable<ISizeItemViewModel>> _items;
        [Property]
        private IObservable<IEnumerable<ISizeItemViewModel>> _topItems;

        public ItemPreviewMode Mode => ItemPreviewMode.SizeContainer;

        public void Init(ContainerSizeContainer container)
        {
            container.Refreshed.Add(ContainerRefreshed);
            container.SizeChanged.Add(ItemsChanged);
            Items = _update.Throttle(TimeSpan.FromMilliseconds(500)).Select((_) => RefreshItems(this, container));
            TopItems = Items.Select(items => items.Take(10));
            _update.OnNext(null);
        }

        private IEnumerable<ISizeItemViewModel> RefreshItems(SizeContainerPreview parent, ContainerSizeContainer container)
        {
            if (_allItems != null)
            {
                foreach (var item in _allItems)
                {
                    if (item is ContainerSizeContainer sizeContainer)
                    {
                        sizeContainer.SizeChanged.Remove(ItemsChanged);
                    }
                }
            }

            var items = GetItems(parent, container).ToList();
            foreach (var item in items)
            {
                if (item is ContainerSizeContainer sizeContainer)
                {
                    sizeContainer.SizeChanged.Add(ItemsChanged);
                }
            }

            _allItems = items;

            return items;
        }

        private Task ItemsChanged(object? sender, long? size, CancellationToken token)
        {
            _update.OnNext(null);
            return Task.CompletedTask;
        }

        private Task ContainerRefreshed(object? sender, AsyncEventArgs e, CancellationToken token)
        {
            _update.OnNext(null);
            return Task.CompletedTask;
        }

        public IEnumerable<ISizeItemViewModel> GetItems(SizeContainerPreview parent, ContainerSizeContainer container)
        {
            var items = new List<ISizeItemViewModel>();
            var itemsWithSize = container.GetItemsWithSize().OrderByDescending(i => i.Size).ToList();

            foreach (var itemWithSize in itemsWithSize)
            {
                if (itemWithSize is ContainerSizeContainer sizeContainer)
                {
                    var containerVm = _serviceProvider.GetService<SizeContainerViewModel>()!;
                    containerVm.Init(parent, sizeContainer);
                    items.Add(containerVm);
                }
                else if (itemWithSize is ContainerSizeElement sizeElement)
                {
                    var elementVm = _serviceProvider.GetService<SizeElementViewmodel>()!;
                    elementVm.Init(sizeElement);
                    items.Add(elementVm);
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            return items;
        }
        public Task Destroy() => Task.CompletedTask;
    }
}
