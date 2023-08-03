namespace FileTime.GuiApp.DesignPreview.Services;

/*public class TabPreview : ITab
{
    public TabPreview()
    {
        var currentLocation = new BehaviorSubject<IContainer?>(ItemPreview.CurrentContainer);
        CurrentLocation = currentLocation.AsObservable();

        var currentItems = new SourceCache<IItem, string>(i => i.Name);
        var items = GenerateItems();
        currentItems.AddOrUpdate(items);
        CurrentSelectedItemPreview = items[0];
        CurrentItems = new BehaviorSubject<IObservable<IChangeSet<IItem, string>>>(currentItems.Connect());
        CurrentSelectedItem = new BehaviorSubject<AbsolutePath?>(new AbsolutePath(null!, CurrentSelectedItemPreview));
    }
    
    public IItem CurrentSelectedItemPreview { get; }

    private static List<IItem> GenerateItems()
        => Enumerable.Range(1, 10).Select(i => (IItem)ItemPreview.GenerateElement("Element" + i)).ToList();

    public IObservable<IContainer?> CurrentLocation { get; }
    public IObservable<AbsolutePath?> CurrentSelectedItem { get; }
    public IObservable<IObservable<IChangeSet<IItem, string>>?> CurrentItems { get; }
    public FullName? LastDeepestSelectedPath { get; }
    public void SetCurrentLocation(IContainer newLocation) => throw new NotImplementedException();

    public void AddItemFilter(ItemFilter filter) => throw new NotImplementedException();

    public void RemoveItemFilter(ItemFilter filter) => throw new NotImplementedException();

    public void RemoveItemFilter(string name) => throw new NotImplementedException();

    public void SetSelectedItem(AbsolutePath newSelectedItem) => throw new NotImplementedException();

    public void ForceSetCurrentLocation(IContainer newLocation) => throw new NotImplementedException();
    public void Init(IContainer obj1) => throw new NotImplementedException();

    public void Dispose()
    {
    }
}*/