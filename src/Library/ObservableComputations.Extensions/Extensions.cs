using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace ObservableComputations;

public static class Extensions
{
    [ObservableComputationsCall]
    public static Selecting<TSourceItem, TResultItem> Selecting<TSourceItem, TResultItem>(
        this ReadOnlyObservableCollection<TSourceItem> source,
        Expression<Func<TSourceItem, TResultItem>> selectorExpression)
        => new(source, selectorExpression);
}