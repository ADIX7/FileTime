using System.Text.RegularExpressions;
using FileTime.Core.Models;

namespace FileTime.App.Search;

public class RegexMatcher : ISearchMatcher
{
    private readonly Regex _regex;

    public RegexMatcher(string pattern)
    {
        _regex = new Regex(pattern);
    }

    public Task<bool> IsItemMatchAsync(IItem item)
        => Task.FromResult(_regex.IsMatch(item.DisplayName));

    public List<ItemNamePart> GetDisplayName(IItem item)
    {
        var displayName = item.DisplayName;

        var match = _regex.Match(item.DisplayName);
        var splitPoints = new List<int>(match.Groups.Count * 2);
        if (match.Groups.Count == 0)
        {
            return new List<ItemNamePart>
            {
                new(displayName)
            };
        }

        var areEvensSpecial = match.Groups[0].Index == 0;
        var isSpecialMatchNumber = areEvensSpecial ? 0 : 1;

        foreach (Group group in match.Groups)
        {
            splitPoints.Add(group.Index);
            splitPoints.Add(group.Index + group.Value.Length);
        }

        if (splitPoints[0] != 0)
            splitPoints.Insert(0, 0);

        var itemNameParts = new List<ItemNamePart>();
        for (var i = 0; i < splitPoints.Count; i++)
        {
            var index = splitPoints[i];
            var nextIndex = i == splitPoints.Count - 1
                ? displayName.Length
                : splitPoints[i + 1];

            if (nextIndex == index) continue;

            var text = displayName.Substring(index, nextIndex - index);
            itemNameParts.Add(new ItemNamePart(text, i % 2 == isSpecialMatchNumber));
        }

        return itemNameParts;
    }
}