namespace AsyncEvent
{
    public class AsyncEventHandler<TSender, TArg> where TArg : AsyncEventArgs
    {
        private readonly List<Func<TSender, TArg, Task>> _handlers;
        private readonly Action<Func<TSender, TArg, Task>> _add;
        private readonly Action<Func<TSender, TArg, Task>> _remove;

        public IReadOnlyList<Func<TSender, TArg, Task>> Handlers { get; }

        public AsyncEventHandler(Action<Func<TSender, TArg, Task>>? add = null, Action<Func<TSender, TArg, Task>>? remove = null)
        {
            _handlers = new List<Func<TSender, TArg, Task>>();
            Handlers = _handlers.AsReadOnly();
            _add = add ?? AddInternal;
            _remove = remove ?? RemoveInternal;
        }

        public void Add(Func<TSender, TArg, Task> handler)
        {
            _add.Invoke(handler);
        }

        public void Remove(Func<TSender, TArg, Task> handler)
        {
            _remove.Invoke(handler);
        }

        private void AddInternal(Func<TSender, TArg, Task> handler)
        {
            _handlers.Add(handler);
        }

        private void RemoveInternal(Func<TSender, TArg, Task> handler)
        {
            _handlers.Remove(handler);
        }

        public async Task InvokeAsync(TSender sender, TArg args)
        {
            foreach(var handler in _handlers)
            {
                await handler(sender, args);
            }
        }

        public static AsyncEventHandler<TSender, TArg> operator +(AsyncEventHandler<TSender, TArg> obj, Func<TSender, TArg, Task> handler)
        {
            obj.Add(handler);
            return obj;
        }

        public static AsyncEventHandler<TSender, TArg> operator -(AsyncEventHandler<TSender, TArg> obj, Func<TSender, TArg, Task> handler)
        {
            obj.Remove(handler);
            return obj;
        }
    }
    public class AsyncEventHandler<TArg> : AsyncEventHandler<object?, AsyncEventArgs> where TArg : AsyncEventArgs
    {
        public AsyncEventHandler(Action<Func<object?, TArg, Task>>? add = null, Action<Func<object?, TArg, Task>>? remove = null) : base(add, remove) { }
    }

    public class AsyncEventHandler : AsyncEventHandler<AsyncEventArgs>
    {
        public AsyncEventHandler(Action<Func<object?, AsyncEventArgs, Task>>? add = null, Action<Func<object?, AsyncEventArgs, Task>>? remove = null) : base(add, remove) { }
    }
}
