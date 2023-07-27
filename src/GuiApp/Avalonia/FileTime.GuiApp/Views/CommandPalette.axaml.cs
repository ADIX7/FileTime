using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FileTime.App.CommandPalette.ViewModels;

namespace FileTime.GuiApp.Views;

public partial class CommandPalette : UserControl
{
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
        if (DataContext is not ICommandPaletteViewModel viewModel) return;

        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            viewModel.Close();
        }
        else
        {
            viewModel.HandleKeyDown(e);
        }
    }
}