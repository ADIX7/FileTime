using FileTime.Core.Models;

namespace FileTime.Core.Serialization.Container;

public class ContainerSerializer : ISerializer<IContainer>
{
    Task<ISerialized> ISerializer<IContainer>.SerializeAsync(int id, IContainer item) => Task.FromResult(Serialize(id, item));

    private ISerialized Serialize(int id, IContainer container)
    {
        var items = container.Items.Select(AbsolutePathSerializer.Serialize).ToArray();
        var serialized = new SerializedContainer
        {
            Id = id,
            Name = container.Name,
            DisplayName = container.DisplayName,
            FullName = container.FullName!.Path,
            NativePath = container.NativePath!.Path,
            Parent = container.Parent!.Path.Path,
            IsHidden = container.IsHidden,
            IsExists = container.IsExists,
            CreatedAt = container.CreatedAt,
            ModifiedAt = container.ModifiedAt,
            CanDelete = container.CanDelete,
            CanRename = container.CanRename,
            Attributes = container.Attributes,
            AllowRecursiveDeletion = container.AllowRecursiveDeletion,
            Items = items
        };
        return serialized;
    }
}