using FileTime.Core.Models;

namespace FileTime.App.Core.Models
{
    public interface IHaveContainer
    {
        IContainer Container { get; }
    }
}