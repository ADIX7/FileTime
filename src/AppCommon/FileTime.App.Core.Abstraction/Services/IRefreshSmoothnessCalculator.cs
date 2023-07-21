namespace FileTime.App.Core.Services;

public interface IRefreshSmoothnessCalculator
{
    TimeSpan RefreshDelay { get; }
    void RegisterChange();
    void RegisterChange(DateTime changeTime);
    void RecalculateSmoothness();
}