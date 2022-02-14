using Avalonia.Media;

namespace FileTime.Avalonia.ViewModels
{
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
}