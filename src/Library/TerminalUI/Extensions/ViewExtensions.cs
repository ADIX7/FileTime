﻿using System.Linq.Expressions;
using TerminalUI.Controls;

namespace TerminalUI.Extensions;

public static class ViewExtensions
{
    public static T? GetExtension<T>(this IView view)
        => (T?) view.Extensions.FirstOrDefault(e => e is T);

    public static IView<TDataContext> WithExtension<TDataContext>(this IView<TDataContext> view, object extension)
    {
        view.Extensions.Add(extension);
        return view;
    }

    public static ChildWithDataContextMapper<TSourceDataContext, TTargetDataContext> WithDataContextMapper<TSourceDataContext, TTargetDataContext>(
        this IView<TTargetDataContext> view,
        Func<TSourceDataContext?, TTargetDataContext?> dataContextMapper)
        => new(view, dataContextMapper);

    public static ChildWithDataContextBinding<TSourceDataContext, TTargetDataContext> WithDataContextBinding<TSourceDataContext, TTargetDataContext>(
        this IView<TTargetDataContext> view,
        Expression<Func<TSourceDataContext?, TTargetDataContext?>> dataContextMapper)
        => new(view, dataContextMapper);

    public static TView Setup<TView>(this TView view, Action<TView> action)
    {
        action(view);
        return view;
    }

    public static TItem WithPropertyChangedHandler<TItem, TExpressionResult>(
        this TItem dataSource,
        Expression<Func<TItem, TExpressionResult>> dataSourceExpression,
        Action<bool, TExpressionResult> handler)
    {
        _ = new PropertyChangedHandler<TItem, TExpressionResult>
        (
            dataSource,
            dataSourceExpression,
            handler!
        );

        return dataSource;
    }
}