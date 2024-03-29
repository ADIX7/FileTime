﻿using System.Linq.Expressions;
using System.Reflection;
using TerminalUI.Controls;

namespace TerminalUI.Extensions;

public static class BindingExtensions
{
    public static Binding<TDataContext, TResult, TResult> Bind<TView, TDataContext, TResult>(
        this TView targetView,
        IView<TDataContext> dataSourceView,
        Expression<Func<TDataContext?, TResult>> dataSourceExpression,
        Expression<Func<TView, TResult>> propertyExpression,
        TResult? fallbackValue = default)
    {
        if (propertyExpression.Body is not MemberExpression {Member: PropertyInfo propertyInfo})
            throw new AggregateException(nameof(propertyExpression) + " must be a property expression");

        return new Binding<TDataContext, TResult, TResult>(
            dataSourceView,
            dataSourceExpression,
            targetView,
            propertyInfo,
            value => value,
            fallbackValue
        );
    }

    public static Binding<TDataContext, TExpressionResult, TResult> Bind<TView, TDataContext, TExpressionResult, TResult>(
        this TView targetView,
        IView<TDataContext> dataSourceView,
        Expression<Func<TDataContext?, TExpressionResult>> dataSourceExpression,
        Expression<Func<TView, TResult>> propertyExpression,
        Func<TExpressionResult, TResult> converter,
        TResult? fallbackValue = default)
    {
        if (propertyExpression.Body is not MemberExpression {Member: PropertyInfo propertyInfo})
            throw new AggregateException(nameof(propertyExpression) + " must be a property expression");

        return new Binding<TDataContext, TExpressionResult, TResult>(
            dataSourceView,
            dataSourceExpression,
            targetView,
            propertyInfo,
            converter,
            fallbackValue
        );
    }
}