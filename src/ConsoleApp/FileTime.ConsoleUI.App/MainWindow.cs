using DeclarativeProperty;
using FileTime.ConsoleUI.App.Controls;
using FileTime.ConsoleUI.App.Extensions;
using Terminal.Gui;

namespace FileTime.ConsoleUI.App;

public class MainWindow
{
    private readonly IConsoleAppState _consoleAppState;
    private View[] _views;
    private const int ParentColumnWidth = 20;

    public MainWindow(IConsoleAppState consoleAppState)
    {
        _consoleAppState = consoleAppState;
    }

    public void Initialize() =>
        _views = new View[]
        {
            GetSelectedItemsView(),
            GetParentsChildren(),
            GetSelectedsChildren()
        };

    private ListView GetSelectedItemsView()
    {
        ListView selectedItemsView = new() {X = ParentColumnWidth, Y = 1, Width = Dim.Percent(60) - 20, Height = Dim.Fill()};
        var selectedsItems = _consoleAppState
            .SelectedTab
            .Map(t => t.CurrentItems)
            .Switch();

        var selectedItem = _consoleAppState.SelectedTab
            .Map(t => t.CurrentSelectedItem)
            .Switch();

        DeclarativePropertyHelpers.CombineLatest(
                selectedItem,
                selectedsItems,
                (selected, items) => Task.FromResult(items.IndexOf(selected)))
            .Subscribe((index, _) =>
            {
                if (index == -1) return;
                selectedItemsView.SelectedItem = index;
                selectedItemsView.Update();
            });

        var renderer = new ItemRenderer(selectedsItems, selectedItemsView.Update);
        selectedItemsView.Source = renderer;
        return selectedItemsView;
    }

    private ListView GetParentsChildren()
    {
        ListView parentsChildrenView = new() {X = 0, Y = 1, Width = ParentColumnWidth, Height = Dim.Fill()};
        var parentsChildren = _consoleAppState
            .SelectedTab
            .Map(t => t.ParentsChildren)
            .Switch();

        var renderer = new ItemRenderer(parentsChildren, parentsChildrenView.Update);
        parentsChildrenView.Source = renderer;
        return parentsChildrenView;
    }

    private ListView GetSelectedsChildren()
    {
        ListView selectedsChildrenView = new() {X = Pos.Percent(60), Y = 1, Width = Dim.Percent(40), Height = Dim.Fill()};
        var selectedsChildren = _consoleAppState
            .SelectedTab
            .Map(t => t.SelectedsChildren)
            .Switch();

        var renderer = new ItemRenderer(selectedsChildren, selectedsChildrenView.Update);
        selectedsChildrenView.Source = renderer;
        return selectedsChildrenView;
    }

    public IEnumerable<View> GetElements() => _views;
}