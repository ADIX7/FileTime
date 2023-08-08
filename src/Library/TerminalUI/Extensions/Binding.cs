using System.Linq.Expressions;
using System.Reflection;
using TerminalUI.Controls;

namespace TerminalUI.Extensions;

public static class Binding
{
    public static Binding<TDataContext, TResult> Bind<TView, TDataContext, TResult>(
        this TView targetView,
        IView<TDataContext> dataSourceView,
        Expression<Func<TDataContext?, TResult>> dataContextExpression,
        Expression<Func<TView, TResult>> propertyExpression)
    {
        if (propertyExpression.Body is not MemberExpression {Member: PropertyInfo propertyInfo})
            throw new AggregateException(nameof(propertyExpression) + " must be a property expression");

        return new Binding<TDataContext, TResult>(
            dataSourceView, 
            dataContextExpression, 
            targetView, 
            propertyInfo
        );
    }
}