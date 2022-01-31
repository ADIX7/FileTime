using System.Threading.Tasks;
using FileTime.Avalonia.ViewModels;

namespace FileTime.Avalonia.Application
{
    public interface INewItemProcessor
    {
        Task UpdateMarkedItems(ContainerViewModel containerViewModel);
    }
}