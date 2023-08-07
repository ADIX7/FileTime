using System.Collections;
using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.App.Core.ViewModels;
using Terminal.Gui;

namespace FileTime.ConsoleUI.App.Controls;

public class ItemRenderer : IListDataSource
{
    private readonly IDeclarativeProperty<ObservableCollection<IItemViewModel>?> _source;

    public ItemRenderer(
        IDeclarativeProperty<ObservableCollection<IItemViewModel>?> source,
        Action update
    )
    {
        _source = source;
        source.Subscribe((_, _) =>
        {
            update();
        });
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
    {
        var itemViewModel = _source.Value?[item];
        container.Move(col, line);
        driver.AddStr(itemViewModel?.DisplayNameText ?? string.Empty);
    }

    public bool IsMarked(int item) => false;

    public void SetMark(int item, bool value)
    {
    }

    public IList ToList() => _source.Value!;

    public int Count => _source.Value?.Count ?? 0;
    public int Length => 20;
}