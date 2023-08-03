using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FileTime.App.FrequencyNavigation.ViewModels;

namespace FileTime.GuiApp.App.Views;

public partial class FrequencyNavigation : UserControl
{
    public FrequencyNavigation()
    {
        InitializeComponent();
        PropertyChanged += FrequencyNavigation_PropertyChanged;
    }

    private async void FrequencyNavigation_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(IsVisible) && IsVisible)
        {
            await Task.Delay(100);
            SearchTextBox.Focus();
        }
    }

    private void Search_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled) return;

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

    private void Search_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Handled) return;
        if (DataContext is not IFrequencyNavigationViewModel viewModel) return;
        viewModel.HandleKeyUp(e);
    }
}