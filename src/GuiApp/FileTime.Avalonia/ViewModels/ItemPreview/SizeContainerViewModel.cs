using AsyncEvent;
using FileTime.Core.ContainerSizeScanner;
using FileTime.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace FileTime.Avalonia.ViewModels.ItemPreview
{
    public class SizeContainerViewModel : ISizeItemViewModel
    {
        private bool _initialized;
        private SizeContainerPreview? _parent;
        private ContainerSizeContainer? _sizeContainer;

        private readonly BehaviorSubject<object?> _update = new BehaviorSubject<object?>(null);
        private IEnumerable<ISizeItemViewModel>? _allItems;

        private IObservable<IEnumerable<ISizeItemViewModel>>? _items;
        private IObservable<IEnumerable<ISizeItemViewModel>>? _topItems;

        public IObservable<IEnumerable<ISizeItemViewModel>>? Items
        {
            get
            {
                if(!_initialized)
                {
                    _update.OnNext(null);
                    _initialized = true;
                }
                return _items;
            }
        }
        public IObservable<IEnumerable<ISizeItemViewModel>>? TopItems
        {
            get
            {
                if (!_initialized)
                {
                    _update.OnNext(null);
                    _initialized = true;
                }

                return _topItems;
            }
        }

        public string? Name { get; private set; }
        public long? Size { get; private set; }

        public IItem? Item => _sizeContainer;

        public void Init(SizeContainerPreview parent, ContainerSizeContainer sizeContainer)
        {
            _sizeContainer = sizeContainer;
            _parent = parent;

            Name = sizeContainer.DisplayName;
            Size = sizeContainer.Size;

            sizeContainer.Refreshed.Add(ContainerRefreshed);
            sizeContainer.SizeChanged.Add(ItemsChanged);
            _items = _update.Throttle(TimeSpan.FromMilliseconds(500)).Select((_) => RefreshItems(parent, sizeContainer));
            _topItems = _items.Select(items => items.Take(10));
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

            var items = parent.GetItems(parent, container).ToList();
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
    }
}
