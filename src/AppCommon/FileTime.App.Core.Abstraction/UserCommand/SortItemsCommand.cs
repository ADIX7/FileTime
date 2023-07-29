using FileTime.App.Core.Models;

namespace FileTime.App.Core.UserCommand;

public sealed class SortItemsCommand : IIdentifiableUserCommand
{
    public const string OrderByNameCommandName = "order_by_name";
    public const string OrderByNameDescCommandName = "order_by_name_desc";
    public const string OrderByDateCommandName = "order_by_date";
    public const string OrderByDateDescCommandName = "order_by_date_desc";

    public static readonly SortItemsCommand OrderByNameCommand =
        new(OrderByNameCommandName, ItemOrdering.Name, "Order by name");

    public static readonly SortItemsCommand OrderByNameDescCommand =
        new(OrderByNameDescCommandName, ItemOrdering.NameDesc, "Order by name (descending)");

    public static readonly SortItemsCommand OrderByDateCommand =
        new(OrderByDateCommandName, ItemOrdering.LastModifyDate, "Order by date");

    public static readonly SortItemsCommand OrderByDateDescCommand =
        new(OrderByDateDescCommandName, ItemOrdering.LastModifyDateDesc, "Order by date (descending)");

    private SortItemsCommand(string userCommandId, ItemOrdering ordering, string title)
    {
        UserCommandID = userCommandId;
        Ordering = ordering;
        Title = title;
    }

    public string UserCommandID { get; }
    public ItemOrdering Ordering { get; }
    public string Title { get; }
}