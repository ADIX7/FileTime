using System.Buffers;
using System.Collections.Concurrent;
using TerminalUI.Controls;

namespace TerminalUI;

public class EventLoop : IEventLoop
{
    private readonly IApplicationContext _applicationContext;
    private readonly object _lock = new();
    private readonly ArrayPool<IView> _viewPool = ArrayPool<IView>.Shared;

    private readonly ConcurrentBag<IView> _viewsToRenderInstantly = new();
    private readonly LinkedList<IView> _viewsToRender = new();

    public EventLoop(IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }

    public void Run()
    {
        _applicationContext.IsRunning = true;
        while (_applicationContext.IsRunning)
        {
            Render();
            Thread.Sleep(10);
        }
    }

    public void Render()
    {
        IView[]? viewsToRenderCopy = null;
        IView[]? viewsAlreadyRendered = null;
        try
        {
            int viewsToRenderCopyCount;
            IView[]? viewsToRenderInstantly;

            lock (_lock)
            {
                CleanViewsToRender();

                viewsToRenderCopyCount = _viewsToRender.Count;
                viewsToRenderCopy = _viewPool.Rent(_viewsToRender.Count);
                _viewsToRender.CopyTo(viewsToRenderCopy, 0);

                viewsToRenderInstantly = _viewsToRenderInstantly.ToArray();
                _viewsToRenderInstantly.Clear();
            }

            viewsAlreadyRendered = _viewPool.Rent(viewsToRenderCopy.Length + viewsToRenderInstantly.Length);
            var viewsAlreadyRenderedIndex = 0;

            foreach (var view in viewsToRenderInstantly)
            {
                if (Contains(viewsAlreadyRendered, view, viewsAlreadyRenderedIndex)) continue;

                view.Render();
                viewsAlreadyRendered[viewsAlreadyRenderedIndex++] = view;
            }

            for (var i = 0; i < viewsToRenderCopyCount; i++)
            {
                var view = viewsToRenderCopy[i];
                if (Contains(viewsAlreadyRendered, view, viewsAlreadyRenderedIndex)) continue;

                view.Render();
                viewsAlreadyRendered[viewsAlreadyRenderedIndex++] = view;
            }
        }
        finally
        {
            if (viewsToRenderCopy is not null)
                _viewPool.Return(viewsToRenderCopy);

            if (viewsAlreadyRendered is not null)
                _viewPool.Return(viewsAlreadyRendered);
        }
    }

    private void CleanViewsToRender()
    {
        IView[]? viewsAlreadyProcessed = null;
        try
        {
            viewsAlreadyProcessed = _viewPool.Rent(_viewsToRender.Count);
            var viewsAlreadyProcessedIndex = 0;

            var currentItem = _viewsToRender.First;
            for (var i = 0; i < _viewsToRender.Count && currentItem is not null; i++)
            {
                if (Contains(viewsAlreadyProcessed, currentItem.Value, viewsAlreadyProcessedIndex))
                {
                    var itemToRemove = currentItem;
                    currentItem = currentItem.Next;
                    _viewsToRender.Remove(itemToRemove);
                    continue;
                }

                viewsAlreadyProcessed[viewsAlreadyProcessedIndex++] = currentItem.Value;
            }
        }
        finally
        {
            if (viewsAlreadyProcessed is not null)
            {
                _viewPool.Return(viewsAlreadyProcessed);
            }
        }
    }

    private static bool Contains(IView[] views, IView view, int max)
    {
        for (var i = 0; i < max; i++)
        {
            if (views[i] == view) return true;
        }

        return false;
    }

    public void AddViewToRender(IView view)
    {
        lock (_lock)
        {
            _viewsToRender.AddLast(view);
        }
    }
}