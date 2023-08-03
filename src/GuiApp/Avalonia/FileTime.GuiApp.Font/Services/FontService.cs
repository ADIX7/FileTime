using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Media;
using FileTime.GuiApp.App.Configuration;
using Microsoft.Extensions.Options;

namespace FileTime.GuiApp.App.Services;

public class FontService : IFontService
{
    private readonly IOptionsMonitor<FontConfiguration> _fontConfiguration;

    private readonly BehaviorSubject<string?> _mainFont = new(null);
    public IObservable<string?> MainFont => _mainFont.DistinctUntilChanged();

    public FontService(IOptionsMonitor<FontConfiguration> fontConfiguration)
    {
        _fontConfiguration = fontConfiguration;
        fontConfiguration.OnChange(UpdateFonts);

        UpdateFonts(fontConfiguration.CurrentValue, null);
    }

    private void UpdateFonts(FontConfiguration newConfiguration, string? _) => _mainFont.OnNext(GetMainFont());

    public string GetMainFont()
    {
        var installedFonts = FontManager.Current.SystemFonts.Select(f => f.Name).ToList();
        return _fontConfiguration.CurrentValue.Main.FirstOrDefault(installedFonts.Contains) 
               ?? FontManager.Current.DefaultFontFamily.Name;
    }
}