namespace FileTime.Core.Models
{
    public interface IContainer : IItem
    {
        IReadOnlyList<IItem> Items { get; }
        IReadOnlyList<IContainer> Containers { get; }
        IReadOnlyList<IElement> Elements { get; }

        void Refresh();
        IContainer? GetParent();
        IItem? GetByPath(string path);
        IContainer CreateContainer(string name);
        IElement CreateElement(string name);

        bool IsExists(string name);

        event EventHandler? Refreshed;
    }
}