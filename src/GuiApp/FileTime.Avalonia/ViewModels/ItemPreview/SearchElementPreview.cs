using Avalonia.Media;
using Avalonia.Threading;
using FileTime.Avalonia.Models;
using FileTime.Avalonia.Services;
using FileTime.Core.Helper;
using FileTime.Core.Models;
using FileTime.Core.Search;
using MvvmGen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileTime.Avalonia.ViewModels.ItemPreview
{
    [ViewModel]
    public partial class SearchElementPreview : IItemPreviewViewModel
    {

        [Property]
        private ItemPreviewMode _mode = ItemPreviewMode.SearchElement;

        [Property]
        private List<ItemNamePartViewModel>? _itemNameParts;

        [Property]
        private string? _realtiveParentPath;

        public async Task Init(ChildSearchElement element, IContainer currentLocation)
        {
            var pathCommonPath = PathHelper.GetCommonPath(currentLocation.FullName!, element.FullName!);
            RealtiveParentPath = new AbsolutePath(null!, element.FullName!.Substring(pathCommonPath.Length).Trim(Constants.SeparatorChar), AbsolutePathType.Unknown, null).GetParentPath();
            ItemNameParts = await Task.Run(async () => await Dispatcher.UIThread.InvokeAsync(() => element.SearchDisplayName.Select(p => new ItemNamePartViewModel(p.Text, p.IsSpecial ? TextDecorations.Underline : null)).ToList()));
        }
    }
}