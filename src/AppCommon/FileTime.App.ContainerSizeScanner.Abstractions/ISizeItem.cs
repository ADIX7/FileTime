using FileTime.App.Core.Models.Traits;
using FileTime.Core.Models;

namespace FileTime.App.ContainerSizeScanner;

public interface ISizeItem : IItem, ISizeProvider
{
}