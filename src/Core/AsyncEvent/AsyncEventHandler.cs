namespace AsyncEvent
{
    public class AsyncEventHandler<TSender, TArg>
    {
        private readonly object _guard = new();
        private readonly List<Func<TSender, TArg, CancellationToken, Task>> _handlers;
        private readonly Action<Func<TSender, TArg, CancellationToken, Task>> _add;
        private readonly Action<Func<TSender, TArg, CancellationToken, Task>> _remove;

        public IReadOnlyList<Func<TSender, TArg, CancellationToken, Task>> Handlers { get; }

        public AsyncEventHandler(Action<Func<TSender, TArg, CancellationToken, Task>>? add = null, Action<Func<TSender, TArg, CancellationToken, Task>>? remove = null)
        {
            _handlers = new List<Func<TSender, TArg, CancellationToken, Task>>();
            Handlers = _handlers.AsReadOnly();
            _add = add ?? AddInternal;
            _remove = remove ?? RemoveInternal;
        }

        public void Add(Func<TSender, TArg, CancellationToken, Task> handler)
        {
            lock (_guard)
            {
                _add.Invoke(handler);
            }
        }

        public void Remove(Func<TSender, TArg, CancellationToken, Task> handler)
        {
            lock (_guard)
            {
                _remove.Invoke(handler);
            }
        }

        private void AddInternal(Func<TSender, TArg, CancellationToken, Task> handler)
        {
            _handlers.Add(handler);
        }

        private void RemoveInternal(Func<TSender, TArg, CancellationToken, Task> handler)
        {
            _handlers.Remove(handler);
        }

        public async Task InvokeAsync(TSender sender, TArg args, CancellationToken token = default)
        {
            List<Func<TSender, TArg, CancellationToken, Task>>? handlers;
            lock (_guard)
            {
                handlers = _handlers;
            }

            foreach (var handler in handlers)
            {
                await handler(sender, args, token);
            }
        }

        public static AsyncEventHandler<TSender, TArg> operator +(AsyncEventHandler<TSender, TArg> obj, Func<TSender, TArg, CancellationToken, Task> handler)
        {
            obj.Add(handler);
            return obj;
        }

        public static AsyncEventHandler<TSender, TArg> operator -(AsyncEventHandler<TSender, TArg> obj, Func<TSender, TArg, CancellationToken, Task> handler)
        {
            obj.Remove(handler);
            return obj;
        }
    }
    public class AsyncEventHandler<TArg> : AsyncEventHandler<object?, TArg>
    {
        public AsyncEventHandler(Action<Func<object?, TArg, CancellationToken, Task>>? add = null, Action<Func<object?, TArg, CancellationToken, Task>>? remove = null) : base(add, remove) { }
    }

    public class AsyncEventHandler : AsyncEventHandler<AsyncEventArgs>
    {
        public AsyncEventHandler(Action<Func<object?, AsyncEventArgs, CancellationToken, Task>>? add = null, Action<Func<object?, AsyncEventArgs, CancellationToken, Task>>? remove = null) : base(add, remove) { }
    }
}
