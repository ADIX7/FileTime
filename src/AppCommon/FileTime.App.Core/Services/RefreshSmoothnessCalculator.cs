using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileTime.App.Core.Services;

public sealed class RefreshSmoothnessCalculator : IRefreshSmoothnessCalculator, INotifyPropertyChanged
{
    private const int MaxSampleTimeInSeconds = 10;
    private const int SampleWindowInMilliseconds = 1000;
    private const int MaxDelayBetweenRefreshes = 600;
    private const int MinDelayBetweenRefreshes = 10;
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
            var segments = (int)Math.Ceiling((double)MaxSampleTimeInSeconds * 1000 / SampleWindowInMilliseconds);
            Span<int> segmentElementCounts = stackalloc int[segments];
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var segment = segments - 1 - (int)((now - item).TotalMilliseconds / SampleWindowInMilliseconds);
                segmentElementCounts[segment]++;
            }

            var weightSum = 0d;
            var score = 0d;

            //Note: This might not be the best algorithm to calculate the delay, but works okay. 
            //Note: I had an algorithm in mind that uses the delta between neighbour times and and delta between a time and now, but I couldn't implement it.
            //Note: If you are good at math and have a better algorithm, please feel free to implement it/create an issue with it.
            for (var i = 0; i < segments; i++)
            {
                var weight = Math.Pow(i, 2);
                weightSum += weight;
                var pressCount = segmentElementCounts[i];
                //Note: we want the minimum delay even if the user pressed only once in a segment
                if (pressCount > 0) pressCount--;
                score += pressCount * weight;
            }

            const double fraction = (double)(MaxDelayBetweenRefreshes - MinDelayBetweenRefreshes) / 4;

            var weightedAvg = weightSum == 0 ? _defaultRefreshDelay.TotalMilliseconds : Math.Round(score / weightSum);
            var finalDelay = (weightedAvg * fraction) + MinDelayBetweenRefreshes;
            finalDelay = finalDelay > MaxDelayBetweenRefreshes ? MaxDelayBetweenRefreshes : finalDelay;
            RefreshDelay = TimeSpan.FromMilliseconds(finalDelay);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}