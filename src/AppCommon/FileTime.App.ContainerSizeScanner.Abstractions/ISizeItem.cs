using FileTime.Core.Models;
using FileTime.Core.Traits;

namespace FileTime.App.ContainerSizeScanner;

public interface ISizeItem : IItem, ISizeProvider
{
}