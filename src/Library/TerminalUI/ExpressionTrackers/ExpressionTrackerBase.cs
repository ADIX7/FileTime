using System.Collections.Specialized;
using System.ComponentModel;

namespace TerminalUI.ExpressionTrackers;

public abstract class ExpressionTrackerBase : IExpressionTracker
{
    private object? _currentValue;
    public List<string> TrackedPropertyNames { get; } = new();

    protected bool SubscribeToValueChanges { get; set; } = true;

    public event Action<string>? PropertyChanged;
    public event Action<bool>? Update;
    public object? GetValue() => _currentValue;
    protected abstract object? ComputeValue();

    protected void SubscribeToTracker(IExpressionTracker? expressionTracker)
    {
        if (expressionTracker is null) return;
        expressionTracker.Update += UpdateValueAndChangeTrackers;
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

        if (TrackedPropertyNames.Contains(e.PropertyName))
        {
            PropertyChanged?.Invoke(e.PropertyName);
        }
    }
}