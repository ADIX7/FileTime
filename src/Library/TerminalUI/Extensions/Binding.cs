using System.Linq.Expressions;
using System.Reflection;
using TerminalUI.Controls;

namespace TerminalUI.Extensions;

public static class Binding
{
    public static Binding<TDataContext, TResult> Bind<TDataContext, TResult, TView>(
        this TView targetView,
        IView<TDataContext> view,
        Expression<Func<TDataContext, TResult>> dataContextExpression,
        Expression<Func<TView, TResult>> propertyExpression)
    {
        var dataContextMapper = dataContextExpression.Compile();

        if (propertyExpression.Body is not MemberExpression memberExpression
            || memberExpression.Member is not PropertyInfo propertyInfo)
            throw new AggregateException(nameof(propertyExpression) + " must be a property expression");

        return new Binding<TDataContext, TResult>(view, dataContextMapper, targetView, propertyInfo);
    }
}