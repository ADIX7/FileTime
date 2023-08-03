using FileTime.Core.Models;

namespace FileTime.App.Core.Services;

public class ItemNameConverterService : IItemNameConverterService
{
    public List<ItemNamePart> GetDisplayName(string name, string? searchText)
    {
        var nameParts = new List<ItemNamePart>();
        searchText = searchText?.ToLower();

        if (!string.IsNullOrEmpty(searchText))
        {
            var nameLeft = name;

            while (nameLeft.ToLower().IndexOf(searchText, StringComparison.Ordinal) is var rapidTextStart && rapidTextStart != -1)
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