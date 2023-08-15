namespace TerminalUI.Tests.Models;

public class TestCollectionItem
{
    public List<TestItem>? Items1 { get; set; } = new()
    {
        new TestItem() {Name = "Name1"},
        new TestItem() {Name = "Name2"},
    };
}