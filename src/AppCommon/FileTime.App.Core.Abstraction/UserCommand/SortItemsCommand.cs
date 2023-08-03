using FileTime.App.Core.Models;

namespace FileTime.App.Core.UserCommand;

public sealed class SortItemsCommand : IIdentifiableUserCommand
{
    public const string OrderByNameCommandName = "order_by_name";
    public const string OrderByNameDescCommandName = "order_by_name_desc";
    public const string OrderByCreatedAtCommandName = "order_by_created_at";
    public const string OrderByCreatedAtDescCommandName = "order_by_created_at_desc";
    public const string OrderByModifiedAtCommandName = "order_by_modified_at";
    public const string OrderByModifiedAtDescCommandName = "order_by_modified_at_desc";

    public static readonly SortItemsCommand OrderByNameCommand =
        new(OrderByNameCommandName, ItemOrdering.Name, "Order by name");

    public static readonly SortItemsCommand OrderByNameDescCommand =
        new(OrderByNameDescCommandName, ItemOrdering.NameDesc, "Order by name (descending)");

    public static readonly SortItemsCommand OrderByCreatedAtCommand =
        new(OrderByCreatedAtCommandName, ItemOrdering.CreationDate, "Order by created");

    public static readonly SortItemsCommand OrderByCreatedAtDescCommand =
        new(OrderByCreatedAtDescCommandName, ItemOrdering.CreationDateDesc, "Order by created (descending)");

    public static readonly SortItemsCommand OrderByLastModifiedCommand =
        new(OrderByModifiedAtCommandName, ItemOrdering.LastModifyDate, "Order by last modified");

    public static readonly SortItemsCommand OrderByLastModifiedDescCommand =
        new(OrderByModifiedAtDescCommandName, ItemOrdering.LastModifyDateDesc, "Order by last modified (descending)");

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