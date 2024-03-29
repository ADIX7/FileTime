namespace FileTime.App.Core.UserCommand;

public enum SearchType
{
    NameContains,
    NameRegex
}

public class SearchCommand : IUserCommand
{
    public string? SearchText { get; }
    public SearchType SearchType { get; }

    public SearchCommand(string? searchText, SearchType searchType)
    {
        SearchText = searchText;
        SearchType = searchType;
    }
}

public sealed class IdentifiableSearchCommand : SearchCommand, IIdentifiableUserCommand
{
    public const string SearchByNameContainsCommandName = "search_name_contains";
    public const string SearchByRegexCommandName = "search_name_regex";

    public static readonly IdentifiableSearchCommand SearchByNameContains =
        new(null, SearchType.NameContains, SearchByNameContainsCommandName, "Search by name");

    public static readonly IdentifiableSearchCommand SearchByRegex =
        new(null, SearchType.NameRegex, SearchByRegexCommandName, "Search by name (Regex)");

    private IdentifiableSearchCommand(
        string? searchText,
        SearchType searchType,
        string commandId,
        string title)
        : base(searchText, searchType)
    {
        UserCommandID = commandId;
        Title = title;
    }

    public string UserCommandID { get; }
    public string Title { get; }
}