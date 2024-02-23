using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Svg.Skia;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.GuiApp.App.IconProviders;
using FileTime.GuiApp.App.Models;
using FileTime.Providers.Local;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.GuiApp.App.Converters;

public class ItemToImageConverter : IValueConverter
{
    private readonly IIconProvider _iconProvider = DI.ServiceProvider.GetRequiredService<IIconProvider>();
    private readonly ILocalContentProvider _localContentProvider = DI.ServiceProvider.GetRequiredService<ILocalContentProvider>();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return null;

        SvgSource? source;
        try
        {
            var path = GetImageFromPath(value);
            path ??= GetImageFromItem(value);

            if (path is null) return null;

            if (path.Type == ImagePathType.Absolute)
            {
                source = SvgSource.Load(path.Path!, null);
            }
            else if (path.Type == ImagePathType.Raw)
            {
                return path.Image;
            }
            else
            {
                source = SvgSource.Load("avares://FileTime.GuiApp.App" + path.Path, null);
            }
        }
        catch
        {
            source = SvgSource.Load("avares://FileTime.GuiApp.App/Assets/material/file.svg", null);
        }

        return new SvgImage {Source = source};
    }

    private ImagePath? GetImageFromItem(object value)
    {
        var item = value switch
        {
            IContainerViewModel container => container.Container!,
            IElementViewModel element => element.Element!,
            IItem i => i,
            _ => null
        };

        if (item is null) return null;

        return _iconProvider.GetImage(item)!;
    }

    private ImagePath? GetImageFromPath(object value)
    {
        if (value is not NativePath nativePath) return null;

        var canHandlePathTask = _localContentProvider.CanHandlePathAsync(nativePath);
        canHandlePathTask.Wait();
        var isLocal = canHandlePathTask.Result; 
            

        var isDirectory = Directory.Exists(nativePath.Path);

        return _iconProvider.GetImage(nativePath.Path, isDirectory, isLocal);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}