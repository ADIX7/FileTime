using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged.SourceGenerator;

namespace TerminalUI.Tests.Models;

public sealed partial class TestViewModel : INotifyPropertyChanged
{
    [Notify] private List<TestNestedCollectionItem> _items;
    [Notify] private string _text;

    public TestViewModel()
    {
        _text = "Initial text";
        _items = new List<TestNestedCollectionItem>
        {
            TestNestedCollectionItem.Create(
                3,
                TestNestedCollectionItem.Create(
                    1, 
                    TestNestedCollectionItem.Create(2)
                ),
                new()
            )
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}