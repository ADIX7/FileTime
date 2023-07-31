using FileTime.Core.Models;

namespace FileTime.Core.Behaviors;

public interface IItemNameConverterProvider
{
    Task<IEnumerable<ItemNamePart>> GetItemNamePartsAsync(IItem item);
}