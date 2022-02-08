using FileTime.Avalonia.Models;
using FileTime.Core.Models;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTime.Avalonia.ViewModels
{
    [ViewModel]
    public partial class ElementPreviewViewModel
    {
        private const int MAXTEXTPREVIEWSIZE = 1024 * 1024;
        [Property]
        private IElement? _element;

        [Property]
        private string? _textContent;

        [Property]
        private ElementPreviewMode? _mode;

        public async Task Init(IElement element, CancellationToken token = default)
        {
            Element = element;

            var elementSize = await element.GetElementSize(token);
            if (elementSize == 0)
            {
                Mode = ElementPreviewMode.Empty;
            }
            else if (elementSize < MAXTEXTPREVIEWSIZE)
            {
                try
                {
                    TextContent = await element.GetContent();
                }
                catch(Exception e)
                {
                    TextContent = $"Error while getting content of {element.FullName}. " + e.ToString();
                }
                Mode = ElementPreviewMode.Text;
            }
            else
            {
                Mode = ElementPreviewMode.Unknown;
            }
        }
    }
}
