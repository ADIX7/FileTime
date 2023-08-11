using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FileTime.App.CommandPalette.ViewModels;
using FileTime.App.Core.Services;
using FileTime.GuiApp.App.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.GuiApp.App.Views;

public partial class CommandPalette : UserControl
{
    private readonly Lazy<IAppKeyService<Key>> _appKeyService = new(() => DI.ServiceProvider.GetRequiredService<IAppKeyService<Key>>());

    public CommandPalette()
    {
        InitializeComponent();
        PropertyChanged += CommandPalette_PropertyChanged;
    }

    private async void CommandPalette_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(IsVisible) && IsVisible)
        {
            await Task.Delay(10);
            SearchTextBox.Focus();
        }
    }

    private void Search_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled) return;
        if (DataContext is not ICommandPaletteViewModel viewModel) return;

        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            viewModel.Close();
        }
        else
        {
            if (e.ToGeneralKeyEventArgs(_appKeyService.Value, e.KeyModifiers) is not { } eventArgs) return;
            
            viewModel.HandleKeyDown(eventArgs);
        }
    }

    private void Search_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Handled
            || DataContext is not ICommandPaletteViewModel viewModel) return;
        
        if (e.ToGeneralKeyEventArgs(_appKeyService.Value, e.KeyModifiers) is not { } eventArgs) return;
        viewModel.HandleKeyUp(eventArgs);
    }
}