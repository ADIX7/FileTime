using System.ComponentModel;

namespace TerminalUI;

internal interface IPropertyChangeTracker : IDisposable
{
    Dictionary<string, IPropertyChangeTracker> Children { get; }
}

internal abstract class PropertyChangeTrackerBase : IPropertyChangeTracker
{
    public Dictionary<string, IPropertyChangeTracker> Children { get; } = new();

    public virtual void Dispose()
    {
        foreach (var propertyChangeTracker in Children.Values)
        {
            propertyChangeTracker.Dispose();
        }
    }
}

internal class PropertyChangeTracker : PropertyChangeTrackerBase
{
    private readonly PropertyTrackTreeItem _propertyTrackTreeItem;
    private readonly INotifyPropertyChanged _target;
    private readonly IEnumerable<string> _propertiesToListen;
    private readonly Action _updateBinding;

    public PropertyChangeTracker(
        PropertyTrackTreeItem propertyTrackTreeItem,
        INotifyPropertyChanged target,
        IEnumerable<string> propertiesToListen,
        Action updateBinding)
    {
        _propertyTrackTreeItem = propertyTrackTreeItem;
        _target = target;
        _propertiesToListen = propertiesToListen;
        _updateBinding = updateBinding;
        target.PropertyChanged += Target_PropertyChanged;
    }

    private void Target_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var propertyName = e.PropertyName;
        if (propertyName is null || !_propertiesToListen.Contains(propertyName))
        {
            return;
        }

        _updateBinding();
        Children.Remove(propertyName);
        
        var newChild = PropertyChangeHelper.TraverseDataContext(
            _propertyTrackTreeItem.Children[propertyName],
            _target.GetType().GetProperty(propertyName)?.GetValue(_target),
            _updateBinding
        );

        if (newChild is not null)
        {
            Children.Add(propertyName, newChild);
        }
    }

    public override void Dispose()
    {
        _target.PropertyChanged -= Target_PropertyChanged;

        base.Dispose();
    }
}

internal class NonSubscriberPropertyChangeTracker : PropertyChangeTrackerBase
{
}

internal class PropertyTrackTreeItem
{
    public Dictionary<string, PropertyTrackTreeItem> Children { get; } = new();
}

internal static class PropertyChangeHelper
{
    internal static IPropertyChangeTracker? TraverseDataContext(
        PropertyTrackTreeItem propertyTrackTreeItem,
        object? obj,
        Action updateBinding
    )
    {
        if (obj is null) return null;

        IPropertyChangeTracker tracker = obj is INotifyPropertyChanged notifyPropertyChanged
            ? new PropertyChangeTracker(propertyTrackTreeItem, notifyPropertyChanged, propertyTrackTreeItem.Children.Keys, updateBinding)
            : new NonSubscriberPropertyChangeTracker();

        foreach (var (propertyName, trackerTreeItem) in propertyTrackTreeItem.Children)
        {
            var childTracker = TraverseDataContext(
                trackerTreeItem,
                obj.GetType().GetProperty(propertyName)?.GetValue(obj),
                updateBinding
            );

            if (childTracker is not null)
            {
                tracker.Children.Add(propertyName, childTracker);
            }
        }

        return tracker;
    }
}