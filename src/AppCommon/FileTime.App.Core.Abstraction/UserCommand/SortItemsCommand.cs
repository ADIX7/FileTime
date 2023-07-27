using FileTime.App.Core.Models;

namespace FileTime.App.Core.UserCommand;

public class SortItemsCommand : IIdentifiableUserCommand
{
    public const string OrderByNameCommandName = "order_by_name";
    public const string OrderByNameDescCommandName = "order_by_name_desc";
    public const string OrderByDateCommandName = "order_by_date";
    public const string OrderByDateDescCommandName = "order_by_date_desc";


    public static readonly SortItemsCommand OrderByNameCommand =
        new(OrderByNameCommandName, ItemOrdering.Name);

    public static readonly SortItemsCommand OrderByNameDescCommand =
        new(OrderByNameDescCommandName, ItemOrdering.NameDesc);

    public static readonly SortItemsCommand OrderByDateCommand =
        new(OrderByDateCommandName, ItemOrdering.LastModifyDate);

    public static readonly SortItemsCommand OrderByDateDescCommand =
        new(OrderByDateDescCommandName, ItemOrdering.LastModifyDateDesc);

    private SortItemsCommand(string userCommandId, ItemOrdering ordering)
    {
        UserCommandID = userCommandId;
        Ordering = ordering;
    }

    public string UserCommandID { get; }
    public ItemOrdering Ordering { get; }
}