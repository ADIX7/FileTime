using System.Collections.ObjectModel;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Serialization.Container;

public class ContainerDeserializer
{
    private readonly ITimelessContentProvider _timelessContentProvider;

    public ContainerDeserializer(ITimelessContentProvider timelessContentProvider)
    {
        _timelessContentProvider = timelessContentProvider;
    }

    public ContainerDeserializationResult Deserialize(
        SerializedContainer source,
        ContainerDeserializationContext context)
    {
        ObservableCollection<Exception> exceptions = new();
        ExtensionCollection extensions = new();
        ObservableCollection<AbsolutePath> items = new();

        var mappedItems = source.Items
            .Select(x => new AbsolutePath(
                    _timelessContentProvider,
                    x.PointInTime,
                    new FullName(x.Path),
                    x.Type
                )
            );
        
        foreach (var item in
                 mappedItems)
        {
            items.Add(item);
        }
        
        var container = new Models.Container(
            source.Name,
            source.DisplayName,
            new FullName(source.FullName),
            new NativePath(source.NativePath),
            new AbsolutePath(_timelessContentProvider, PointInTime.Present, new FullName(source.Parent), AbsolutePathType.Container),
            source.IsHidden,
            source.IsExists,
            source.CreatedAt,
            source.ModifiedAt,
            source.CanDelete,
            source.CanRename,
            source.Attributes,
            context.ContentProvider,
            source.AllowRecursiveDeletion,
            PointInTime.Present,
            exceptions,
            new ReadOnlyExtensionCollection(extensions),
            items
        );

        return new ContainerDeserializationResult(
            container,
            exceptions,
            extensions,
            items);
    }
}