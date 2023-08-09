namespace FileTime.ConsoleUI.App;

public interface IColorSampleProvider
{
    public Type? ForegroundColors { get; }
    public Type? BackgroundColors { get; }
}