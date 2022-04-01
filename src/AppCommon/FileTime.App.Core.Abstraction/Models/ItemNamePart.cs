namespace FileTime.App.Core.Models
{
    public class ItemNamePart
    {
        public string Text { get; set; }
        public bool IsSpecial { get; set; }

        public ItemNamePart(string text, bool isSpecial = false)
        {
            Text = text;
            IsSpecial = isSpecial;
        }
    }
}