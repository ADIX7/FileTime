using System.Collections.Specialized;
using System.ComponentModel;

namespace TerminalUI.ExpressionTrackers;

public abstract class ExpressionTrackerBase : IExpressionTracker
{
    private object? _currentValue;
    private readonly List<IExpressionTracker> _propertyProxyTrackers = new();
    private readonly List<string> _trackedPropertyNames = new();

    protected bool SubscribeToValueChanges { get; set; } = true;

    public event Action<string>? PropertyChanged;
    public event Action<bool>? Update;
    public object? GetValue() => _currentValue;
    protected abstract object? ComputeValue();

    protected void SubscribeToTracker(IExpressionTracker? expressionTracker, bool proxyPropertyChanged = false)
    {
        if (expressionTracker is null) return;

        expressionTracker.Update += UpdateValueAndChangeTrackers;

        if (proxyPropertyChanged)
        {
            expressionTracker.PropertyChanged += OnPropertyChanged;
            _propertyProxyTrackers.Add(expressionTracker);

            foreach (var propertyName in _trackedPropertyNames)
            {
                expressionTracker.TrackProperty(propertyName);
            }
        }
    }

    protected void UpdateValueAndChangeTrackers() => UpdateValueAndChangeTrackers(true);

    private void UpdateValueAndChangeTrackers(bool couldCompute)
    {
        if (SubscribeToValueChanges)
        {
            if (_currentValue is INotifyPropertyChanged oldNotifyPropertyChanged)
                oldNotifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
            if (_currentValue is INotifyCollectionChanged collectionChanged)
                collectionChanged.CollectionChanged -= OnCollectionChanged;
        }

        var useNull = false;
        try
        {
            if (couldCompute)
            {
                _currentValue = ComputeValue();

                if (SubscribeToValueChanges)
                {
                    if (_currentValue is INotifyPropertyChanged notifyPropertyChanged)
                        notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
                    if (_currentValue is INotifyCollectionChanged collectionChanged)
                        collectionChanged.CollectionChanged += OnCollectionChanged;
                }

                Update?.Invoke(true);
            }
            else
            {
                useNull = true;
            }
        }
        catch (Exception e)
        {
            useNull = true;
        }

        if (useNull)
        {
            _currentValue = null;
            Update?.Invoke(false);
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => UpdateValueAndChangeTrackers();

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null) return;

        if (_trackedPropertyNames.Contains(e.PropertyName))
        {
            PropertyChanged?.Invoke(e.PropertyName);
        }
    }

    protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(propertyName);

    public void TrackProperty(string propertyName)
    {
        if (!_trackedPropertyNames.Contains(propertyName))
        {
            _trackedPropertyNames.Add(propertyName);
        }

        foreach (var propertyProxyTracker in _propertyProxyTrackers)
        {
            propertyProxyTracker.TrackProperty(propertyName);
        }
    }
}