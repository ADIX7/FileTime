using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using FileTime.App.Core.ViewModels;
using FileTime.GuiApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.GuiApp.Converters;

public class ContextMenuGenerator : IValueConverter
{
    private IContextMenuProvider? _contextMenuProvider;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        _contextMenuProvider ??= DI.ServiceProvider.GetRequiredService<IContextMenuProvider>();

        if (value is IContainerViewModel containerViewModel)
        {
            return _contextMenuProvider.GetContextMenuForFolder(containerViewModel.Container);
        }

        return new object[] { new MenuItem() { Header = "asd" } };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}