using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged.SourceGenerator;

namespace TerminalUI.Tests.Models;

public sealed partial class TestViewModel : INotifyPropertyChanged
{
    [Notify] private List<TestNestedCollectionItem> _items = new()
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
    [Notify] private string _text = "Initial text";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}