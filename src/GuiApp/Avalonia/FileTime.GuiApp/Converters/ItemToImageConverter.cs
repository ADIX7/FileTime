using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Svg.Skia;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.GuiApp.IconProviders;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.GuiApp.Converters;

public class ItemToImageConverter : IValueConverter
{
    private readonly IIconProvider _iconProvider;

    public ItemToImageConverter()
    {
        _iconProvider = DI.ServiceProvider.GetRequiredService<IIconProvider>();
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return null;

        IItem item = value switch
        {
            IContainerViewModel container => container.Container!,
            IElementViewModel element => element.Element!,
            IItem i => i,
            _ => throw new NotImplementedException()
        };

        SvgSource? source;
        try
        {
            var path = _iconProvider.GetImage(item)!;
            if (path.Type == Models.ImagePathType.Absolute)
            {
                source = SvgSource.Load<SvgSource>(path.Path!, null);
            }
            else if (path.Type == Models.ImagePathType.Raw)
            {
                return path.Image;
            }
            else
            {
                source = SvgSource.Load<SvgSource>("avares://FileTime.GuiApp" + path.Path, null);
            }
        }
        catch
        {
            source = SvgSource.Load<SvgSource>("avares://FileTime.GuiApp/Assets/material/file.svg", null);
        }

        return new SvgImage {Source = source};
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}