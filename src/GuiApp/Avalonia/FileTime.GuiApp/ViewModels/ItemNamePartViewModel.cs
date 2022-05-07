using Avalonia.Media;

namespace FileTime.GuiApp.ViewModels;

public class ItemNamePartViewModel
{
    public string Text { get; set; }
    public TextDecorationCollection? TextDecorations { get; set; }

    public ItemNamePartViewModel(string text, TextDecorationCollection? textDecorations)
    {
        Text = text;
        TextDecorations = textDecorations;
    }
}