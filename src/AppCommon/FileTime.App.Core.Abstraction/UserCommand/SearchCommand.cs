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

public class IdentifiableSearchCommand : SearchCommand, IIdentifiableUserCommand
{
    public const string SearchByNameContainsCommandName = "search_name_contains";

    public static readonly IdentifiableSearchCommand SearchByNameContains =
        new(null, SearchType.NameContains, SearchByNameContainsCommandName);

    private IdentifiableSearchCommand(
        string? searchText,
        SearchType searchType,
        string commandId)
        : base(searchText, searchType)
    {
        UserCommandID = commandId;
    }

    public string UserCommandID { get; }
}