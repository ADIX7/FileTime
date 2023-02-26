using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FileTime.App.FrequencyNavigation.ViewModels;

namespace FileTime.GuiApp.Views;

public partial class FrequencyNavigation : UserControl
{
    public FrequencyNavigation()
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
    }
}