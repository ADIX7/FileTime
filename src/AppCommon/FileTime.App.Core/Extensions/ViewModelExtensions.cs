using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;

namespace FileTime.App.Core.Extensions
{
    public static class ViewModelExtensions
    {
        public static IAbsolutePath ToAbsolutePath(this IItemViewModel itemViewModel)
        {
            var item = itemViewModel.BaseItem ?? throw new ArgumentException($"{nameof(itemViewModel)} does not have {nameof(IItemViewModel.BaseItem)}");
            return new AbsolutePath(item.Provider, item.FullName ?? throw new ArgumentException($"Parameter does not have {nameof(IItem.FullName)}"), item.Type);
        }
    }
}