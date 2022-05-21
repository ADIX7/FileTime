using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Core.Extensions;

public static class ViewModelExtensions
{
    public static AbsolutePath ToAbsolutePath(this IItemViewModel itemViewModel, ITimelessContentProvider timelessContentProvider)
    {
        var item = itemViewModel.BaseItem ?? throw new ArgumentException($"{nameof(itemViewModel)} does not have {nameof(IItemViewModel.BaseItem)}");
        return new AbsolutePath(timelessContentProvider, item);
    }
}