using FileTime.Core.Models;

namespace FileTime.Core.Services
{
    public class ItemNameConverterService
    {
        public List<ItemNamePart> GetDisplayName(IItem item, string? searchText)
        {
            var nameParts = new List<ItemNamePart>();
            searchText = searchText?.ToLower();

            var name = item is IElement ? GetFileName(item.DisplayName) : item.DisplayName;
            if (!string.IsNullOrEmpty(searchText))
            {
                var nameLeft = name;

                while (nameLeft.ToLower().IndexOf(searchText, StringComparison.Ordinal) is int rapidTextStart && rapidTextStart != -1)
                {
                    var before = rapidTextStart > 0 ? nameLeft.Substring(0, rapidTextStart) : null;
                    var rapidTravel = nameLeft.Substring(rapidTextStart, searchText.Length);

                    nameLeft = nameLeft.Substring(rapidTextStart + searchText.Length);

                    if (before != null)
                    {
                        nameParts.Add(new ItemNamePart(before));
                    }

                    nameParts.Add(new ItemNamePart(rapidTravel, true));
                }

                if (nameLeft.Length > 0)
                {
                    nameParts.Add(new ItemNamePart(nameLeft));
                }
            }
            else
            {
                nameParts.Add(new ItemNamePart(name));
            }
            return nameParts;
        }

        public string GetFileName(string fullName)
        {
            var parts = fullName.Split('.');
            var fileName = string.Join('.', parts[..^1]);
            return string.IsNullOrEmpty(fileName) ? fullName : fileName;
        }

        public string GetFileExtension(string fullName)
        {
            var parts = fullName.Split('.');
            return parts.Length == 1 || (parts.Length == 2 && string.IsNullOrEmpty(parts[0])) ? "" : parts[^1];
        }
    }
}
