using System.Text;
using FileTime.App.Core.Models;
using FileTime.Core.Models;
using InitableService;
using MvvmGen;

namespace FileTime.App.Core.ViewModels.ItemPreview;

[ViewModel]
public partial class ElementPreviewViewModel : IItemPreviewViewModel, IAsyncInitable<IElement>
{
    private const int MaxTextPreviewSize = 1024 * 1024;
    
    public ItemPreviewMode Mode { get; private set; }

    [Property] private string? _textContent;
    
    public async Task InitAsync(IElement element)
    {
        try
        {
            var content = await element.Provider.GetContentAsync(element, MaxTextPreviewSize);

            TextContent = content is null
                ? "Could not read any data from file " + element.Name
                : Encoding.UTF8.GetString(content);
        }
        catch(Exception ex)
        {
            TextContent = $"Error while getting content of {element.FullName}. " + ex.ToString();
        }

        Mode = (TextContent?.Length ?? 0) switch
        {
            0 => ItemPreviewMode.Empty,
            _ => ItemPreviewMode.Text
        };
    }
}