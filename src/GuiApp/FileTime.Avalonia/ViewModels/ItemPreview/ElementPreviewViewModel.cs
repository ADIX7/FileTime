using FileTime.Avalonia.Models;
using FileTime.Core.Models;
using MvvmGen;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FileTime.Avalonia.ViewModels.ItemPreview
{
    [ViewModel]
    public partial class ElementPreviewViewModel : IItemPreviewViewModel
    {
        private const int MAXTEXTPREVIEWSIZE = 1024 * 1024;
        [Property]
        private IElement? _element;

        [Property]
        private string? _textContent;

        [Property]
        private ItemPreviewMode _mode = ItemPreviewMode.Unknown;

        public async Task Init(IElement element, CancellationToken token = default)
        {
            Element = element;

            long? elementSize = null;

            try
            {
                elementSize = await element.GetElementSize(token);
            }
            catch { }

            if (elementSize == 0)
            {
                Mode = ItemPreviewMode.Empty;
            }
            else if (elementSize < MAXTEXTPREVIEWSIZE)
            {
                try
                {
                    TextContent = await element.GetContent();
                }
                catch (Exception e)
                {
                    TextContent = $"Error while getting content of {element.FullName}. " + e.ToString();
                }
                Mode = ItemPreviewMode.Text;
            }
            else
            {
                Mode = ItemPreviewMode.Unknown;
            }
        }
    }
}
