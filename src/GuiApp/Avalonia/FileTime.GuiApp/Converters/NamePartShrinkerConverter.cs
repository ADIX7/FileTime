using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using FileTime.App.Core.Models;
using FileTime.GuiApp.ViewModels;

namespace FileTime.GuiApp.Converters
{
    public class NamePartShrinkerConverter : IMultiValueConverter
    {
        private const int PixelPerChar = 8;
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count > 0 && values[0] is IList<ItemNamePart> nameParts)
            {
                var attributeWidth = values[2] is bool b && b ? 340 : 0;
                var newNameParts = nameParts;
                if (values.Count > 1 && values[1] is double width && width > 0)
                {
                    newNameParts = GetNamePartsForWidth(nameParts, width - attributeWidth);
                }

                return newNameParts.Select(p => new ItemNamePartViewModel(p.Text, p.IsSpecial ? TextDecorations.Underline : null)).ToList();
            }
            return null;
        }

        private static List<ItemNamePart> GetNamePartsForWidth(IList<ItemNamePart> nameParts, double maxWidth)
        {
            //Best case, we are in the range
            var textLength = nameParts.Select(p => p.Text.Length).Sum();
            if (textLength * PixelPerChar <= maxWidth)
            {
                return nameParts.ToList();
            }

            //Trying at least with the special parts
            var newNameParts = new ItemNamePart?[nameParts.Count];
            for (var i = 0; i < nameParts.Count; i++)
            {
                if (nameParts[i].IsSpecial)
                {
                    newNameParts[i] = nameParts[i];
                }
            }

            return GetNamePartsForWidthOptimistic(nameParts, newNameParts, maxWidth);
        }

        private static List<ItemNamePart> GetNamePartsForWidthOptimistic(IList<ItemNamePart> nameParts, ItemNamePart?[] newNameParts, double maxWidth)
        {
            var trimmedIndexes = new List<int>();
            for (var i = 0; i < newNameParts.Length; i++)
            {
                if (newNameParts[i] == null)
                {
                    trimmedIndexes.Add(i);
                    newNameParts[i] = new ItemNamePart("...");
                }
            }

            var textLength = newNameParts.Select(p => p?.Text.Length ?? 0).Sum();
            if (textLength * PixelPerChar > maxWidth)
            {
                return GetNamePartsForWidthPessimistic(nameParts, maxWidth);
            }

            foreach (var trimmedIndex in trimmedIndexes)
            {
                var baseTextLength = newNameParts.Select((p, i) => i == trimmedIndex ? 0 : (p?.Text.Length ?? 0)).Sum();
                var proposedText = nameParts[trimmedIndex].Text;
                var trimmed = false;
                while ((baseTextLength + proposedText.Length + (trimmed ? 3 : 0)) * PixelPerChar > maxWidth)
                {
                    proposedText = proposedText[0..^1];
                    trimmed = true;
                }
                newNameParts[trimmedIndex] = new ItemNamePart(proposedText + (trimmed ? "..." : ""));
                if (trimmed) break;
            }

            return newNameParts.Where(f => f is not null).ToList()!;
        }

        private static List<ItemNamePart> GetNamePartsForWidthPessimistic(IList<ItemNamePart> nameParts, double maxWidth)
        {
            var newNameParts = new List<ItemNamePart>(nameParts);
            foreach (var namePart in nameParts)
            {
                var baseTextLength = newNameParts.Select(p => p.Text.Length).Sum();
                var proposedText = namePart.Text;
                var trimmed = false;

                while ((baseTextLength + 3) * PixelPerChar > maxWidth && proposedText != "")
                {
                    proposedText = proposedText[0..^1];
                    trimmed = true;
                }

                if (!string.IsNullOrWhiteSpace(proposedText)) newNameParts.Add(new ItemNamePart(proposedText, namePart.IsSpecial));
                if (trimmed) break;
            }
            if (newNameParts.Last().IsSpecial)
            {
                newNameParts.Add(new ItemNamePart("..."));
            }
            else
            {
                var last = newNameParts.Last();
                last.Text += "...";
            }
            return newNameParts;
        }
    }
}