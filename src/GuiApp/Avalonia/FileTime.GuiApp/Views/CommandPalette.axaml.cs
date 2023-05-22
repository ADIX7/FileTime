using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FileTime.App.FrequencyNavigation.ViewModels;

namespace FileTime.GuiApp.Views;

public partial class CommandPalette : UserControl
{
    public CommandPalette()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Search_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not IFrequencyNavigationViewModel viewModel) return;
        
        if (e.Key == Key.Escape)
        {
            viewModel.Close();
        }
        else
        {
            viewModel.HandleKeyDown(e);
        }
    }
}