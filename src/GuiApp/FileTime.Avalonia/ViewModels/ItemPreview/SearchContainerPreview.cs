using Avalonia.Media;
using Avalonia.Threading;
using FileTime.Avalonia.Models;
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
    public partial class SearchContainerPreview : IItemPreviewViewModel
    {
        [Property]
        private ItemPreviewMode _mode = ItemPreviewMode.SearchContainer;

        [Property]
        private List<ItemNamePartViewModel>? _itemNameParts;

        [Property]
        private string? _realtiveParentPath;

        public void Init(ChildSearchContainer container, IContainer currentLocation)
        {
            var pathCommonPath = PathHelper.GetCommonPath(currentLocation.FullName!, container.FullName!);
            RealtiveParentPath = new AbsolutePath(null!, container.FullName!.Substring(pathCommonPath.Length).Trim(Constants.SeparatorChar), AbsolutePathType.Unknown, null).GetParentPath();
            Task.Run(async () => ItemNameParts = await Dispatcher.UIThread.InvokeAsync(() => container.SearchDisplayName.ConvertAll(p => new ItemNamePartViewModel(p.Text, p.IsSpecial ? TextDecorations.Underline : null))));
        }
    }
}