using Avalonia.Controls;
using Avalonia.Data.Converters;
using FileTime.Avalonia.Services;
using FileTime.Avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;

namespace FileTime.Avalonia.Converters
{
    public class ContextMenuGenerator : IValueConverter
    {
        private IContextMenuProvider? _contextMenuProvider;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            _contextMenuProvider ??= App.ServiceProvider.GetService<IContextMenuProvider>() ?? throw new Exception($"No {nameof(IContextMenuProvider)} is registered.");

            if (value is ContainerViewModel containerViewModel)
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
}
