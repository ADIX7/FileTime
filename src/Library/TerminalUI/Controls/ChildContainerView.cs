using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TerminalUI.Controls;

public abstract class ChildContainerView<T> : View<T>, IChildContainer<T>
{
    private readonly ObservableCollection<IView> _children = new();
    public ReadOnlyObservableCollection<IView> Children { get; }
    public ChildInitializer<T> ChildInitializer { get; }

    protected ChildContainerView()
    {
        ChildInitializer = new ChildInitializer<T>(this);
        Children = new ReadOnlyObservableCollection<IView>(_children);
        _children.CollectionChanged += (o, args) =>
        {
            if (Attached)
            {
                if (args.NewItems?.OfType<IView>() is { } newItems)
                {
                    foreach (var newItem in newItems)
                    {
                        newItem.Attached = true;
                    }
                }

                ApplicationContext?.EventLoop.RequestRerender();
            }
        };
        
        ((INotifyPropertyChanged)this).PropertyChanged += (o, args) =>
        {
            if (args.PropertyName == nameof(ApplicationContext))
            {
                foreach (var child in Children)
                {
                    child.ApplicationContext = ApplicationContext;
                }
            }
        };
    }

    protected override void AttachChildren()
    {
        foreach (var child in Children)
        {
            child.Attached = true;
        }
    }

    public override TChild AddChild<TChild>(TChild child)
    {
        child = base.AddChild(child);
        _children.Add(child);
        return child;
    }

    public override TChild AddChild<TChild, TDataContext>(TChild child, Func<T?, TDataContext?> dataContextMapper)
        where TDataContext : default
    {
        child = base.AddChild(child, dataContextMapper);
        _children.Add(child);
        return child;
    }
}