using System.Threading.Tasks;
using FileTime.Avalonia.Models;

namespace FileTime.Avalonia.ViewModels.ItemPreview
{
    public interface IItemPreviewViewModel
    {
        ItemPreviewMode Mode { get; }
        Task Destroy();
    }
}