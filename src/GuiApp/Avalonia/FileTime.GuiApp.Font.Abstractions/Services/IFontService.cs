namespace FileTime.GuiApp.Services;

public interface IFontService
{
    IObservable<string?> MainFont { get; }
    string? GetMainFont();
}