using System.ComponentModel;

namespace TerminalUI;

public interface IPropertyChangeTracker : IDisposable
{
    string Name { get; }
    string Path { get; }
    Dictionary<string, IPropertyChangeTracker> Children { get; }
}

public abstract class PropertyChangeTrackerBase : IPropertyChangeTracker
{
    public string Name { get; }
    public string Path { get; }
    public Dictionary<string, IPropertyChangeTracker> Children { get; } = new();

    protected PropertyChangeTrackerBase(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public virtual void Dispose()
    {
        foreach (var propertyChangeTracker in Children.Values)
        {
            propertyChangeTracker.Dispose();
        }
    }
}

public class PropertyChangeTracker : PropertyChangeTrackerBase
{
    private readonly PropertyTrackTreeItem _propertyTrackTreeItem;
    private readonly INotifyPropertyChanged _target;
    private readonly IEnumerable<string> _propertiesToListen;
    private readonly Action<string> _updateBinding;

    public PropertyChangeTracker(
        string name,
        string path,
        PropertyTrackTreeItem propertyTrackTreeItem,
        INotifyPropertyChanged target,
        IEnumerable<string> propertiesToListen,
        Action<string> updateBinding) : base(name, path)
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

        Children.Remove(propertyName);

        var newChild = PropertyChangeHelper.CreatePropertyTracker(
            Path,
            _propertyTrackTreeItem.Children[propertyName],
            _target.GetType().GetProperty(propertyName)?.GetValue(_target),
            _updateBinding
        );

        if (newChild is not null)
        {
            Children.Add(propertyName, newChild);
        }

        _updateBinding(propertyName);
    }

    public override void Dispose()
    {
        _target.PropertyChanged -= Target_PropertyChanged;

        base.Dispose();
    }
}

public class NonSubscriberPropertyChangeTracker : PropertyChangeTrackerBase
{
    public NonSubscriberPropertyChangeTracker(string name, string path) : base(name, path)
    {
    }
}

public class PropertyTrackTreeItem
{
    public string Name { get; }
    public Dictionary<string, PropertyTrackTreeItem> Children { get; } = new();

    public PropertyTrackTreeItem(string name)
    {
        Name = name;
    }
}

public static class PropertyChangeHelper
{
    internal static IPropertyChangeTracker? CreatePropertyTracker(
        string? path,
        PropertyTrackTreeItem propertyTrackTreeItem,
        object? obj,
        Action<string> updateBinding
    )
    {
        if (obj is null) return null;

        path = path is null ? propertyTrackTreeItem.Name : path + "." + propertyTrackTreeItem.Name;

        IPropertyChangeTracker tracker = obj is INotifyPropertyChanged notifyPropertyChanged
            ? new PropertyChangeTracker(
                propertyTrackTreeItem.Name,
                path,
                propertyTrackTreeItem,
                notifyPropertyChanged,
                propertyTrackTreeItem.Children.Keys,
                updateBinding
            )
            : new NonSubscriberPropertyChangeTracker(
                propertyTrackTreeItem.Name,
                path);

        foreach (var (propertyName, trackerTreeItem) in propertyTrackTreeItem.Children)
        {
            var childTracker = CreatePropertyTracker(
                path,
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