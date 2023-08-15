namespace TerminalUI.Tests.Models;

public class TestNestedCollectionItem
{
    public List<TestNestedCollectionItem> Items { get; set; } = new();
    public List<TestCollectionItem> OtherItems { get; set; } = new();

    public TestNestedCollectionItem GetNestedItem(int index) => Items[index];
    public TestCollectionItem GetSimpleItem(int index) => OtherItems[index];

    public static TestNestedCollectionItem Create(int otherItemCount, params TestNestedCollectionItem[] items)
    {
        var collection = new TestNestedCollectionItem()
        {
            Items = items.ToList()
        };
        for (var i = 0; i < otherItemCount; i++)
            collection.OtherItems.Add(new TestCollectionItem());

        return collection;
    }
}