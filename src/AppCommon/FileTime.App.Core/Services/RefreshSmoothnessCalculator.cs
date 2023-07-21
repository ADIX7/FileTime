using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileTime.App.Core.Services;

public sealed class RefreshSmoothnessCalculator : IRefreshSmoothnessCalculator, INotifyPropertyChanged
{
    private const int MaxSampleTimeInSeconds = 10;
    private const int MaxDelayBetweenRefreshes = 400;
    private readonly TimeSpan _maxDelay = TimeSpan.FromSeconds(MaxSampleTimeInSeconds);
    private readonly TimeSpan _defaultRefreshDelay = TimeSpan.FromMilliseconds(200);
    private readonly Queue<DateTime> _changeTimes = new();
    private readonly object _lock = new();
    private TimeSpan _refreshDelay;

    public TimeSpan RefreshDelay
    {
        get => _refreshDelay;
        private set
        {
            if (value.Equals(_refreshDelay)) return;
            _refreshDelay = value;
            OnPropertyChanged();
        }
    }

    public RefreshSmoothnessCalculator()
    {
        _refreshDelay = _defaultRefreshDelay;
    }

    public void RegisterChange() => RegisterChange(DateTime.Now);

    public void RegisterChange(DateTime changeTime)
    {
        lock (_lock)
        {
            if (_changeTimes.Count > 0 && Math.Abs((_changeTimes.Last() - changeTime).TotalMilliseconds) < 5) return;
            CleanList(DateTime.Now);
            _changeTimes.Enqueue(changeTime);
        }
    }

    private void CleanList(DateTime now)
    {
        while (_changeTimes.Count > 0)
        {
            var item = _changeTimes.Peek();
            if (now - item < _maxDelay) return;
            _changeTimes.Dequeue();
        }
    }

    public void RecalculateSmoothness()
    {
        lock (_lock)
        {
            if (_changeTimes.Count < 2)
            {
                RefreshDelay = _defaultRefreshDelay;
                return;
            }

            var now = DateTime.Now;
            CleanList(now);

            var queue = new Queue<DateTime>(_changeTimes);
            var values = new List<(double score, double weight)>(queue.Count - 1);

            var previousChangeTime = queue.Dequeue();
            var biggestDelay = now - previousChangeTime;
            while (queue.Count > 0)
            {
                var changeTime = queue.Dequeue();

                var (score, weight) = CalculateScoreAndWeight(changeTime, previousChangeTime, biggestDelay);

                values.Add((score, weight));

                previousChangeTime = changeTime;
            }

            var combinedScore = values.Sum(i => i.weight * i.score) / values.Sum(i => i.weight);

            var normalizedCombinedScore = (combinedScore * 1.2 - 0.1);

            if (normalizedCombinedScore < 0) normalizedCombinedScore = 0;
            else if (normalizedCombinedScore > 1) normalizedCombinedScore = 1;

            var finalDelay = normalizedCombinedScore * MaxDelayBetweenRefreshes;

            RefreshDelay = TimeSpan.FromMilliseconds(finalDelay);

            (double score, double weight) CalculateScoreAndWeight(DateTime changeTime, DateTime previousChangeTime, TimeSpan biggestDelay)
            {
                var delayToPrevious = changeTime - previousChangeTime;
                var delayToNow = now - changeTime;

                var toNowRatio = (delayToNow.TotalMilliseconds / biggestDelay.TotalMilliseconds);
                var score = 1 - (delayToPrevious.TotalMilliseconds / biggestDelay.TotalMilliseconds);
                var weight = 1 - toNowRatio;

                if (score < 0) score = 0;
                else if (score > 1) score = 1;

                return (score, weight);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}