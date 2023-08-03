namespace FileTime.GuiApp.App.Services;

public interface IFontService
{
    IObservable<string?> MainFont { get; }
    string? GetMainFont();
}