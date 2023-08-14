using System.Diagnostics.Contracts;
using GeneralInputKey;
using TerminalUI.Controls;
using TerminalUI.Traits;

namespace TerminalUI;

public class FocusManager : IFocusManager
{
    private readonly IRenderEngine _renderEngine;
    private IFocusable? _focused;

    public IFocusable? Focused
    {
        get
        {
            if (_focused is not null)
            {
                var visible = _focused.IsVisible;
                var parent = _focused.VisualParent;
                while (parent != null)
                {
                    visible &= parent.IsVisible;
                    parent = parent.VisualParent;
                }

                if (!visible)
                {
                    _focused = null;
                }
            }

            return _focused;
        }

        private set => _focused = value;
    }

    public FocusManager(IRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    public void SetFocus(IFocusable focusable) => Focused = focusable;

    public void UnFocus(IFocusable focusable)
    {
        if (Focused == focusable)
            Focused = null;
    }

    public void HandleKeyInput(GeneralKeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Handled || Focused is null) return;

        if (keyEventArgs.Key == Keys.Tab && keyEventArgs.SpecialKeysStatus.IsShiftPressed)
        {
            FocusElement(
                (views, from) => views.TakeWhile(x => x != from).Reverse(),
                c => c.Reverse()
            );
            keyEventArgs.Handled = true;
        }
        else if (keyEventArgs.Key == Keys.Tab)
        {
            FocusElement(
                (views, from) => views.SkipWhile(x => x != from).Skip(1),
                c => c
            );
            keyEventArgs.Handled = true;
        }
    }


    private void FocusElement(
        Func<IEnumerable<IView>, IView, IEnumerable<IView>> fromChildSelector,
        Func<IEnumerable<IView>, IEnumerable<IView>> childSelector
    )
    {
        if (Focused is null) return;

        var element = FindElement(Focused,
            Focused,
            fromChildSelector
        );

        if (element is null)
        {
            var topParent = FindLastFocusParent(Focused);
            element = FindElement(
                topParent,
                Focused,
                fromChildSelector,
                childSelector
            );
        }

        if (element is null) return;

        _renderEngine.RequestRerender(element);
        _renderEngine.RequestRerender(Focused);
        Focused = element;
    }

    [Pure]
    private static IFocusable? FindElement(IView view, IView original,
        Func<IEnumerable<IView>, IView, IEnumerable<IView>> fromChildSelector,
        Func<IEnumerable<IView>, IEnumerable<IView>>? childSelector = null,
        IView? from = null)
    {
        if (!view.IsVisible) return null;
        childSelector ??= views => views;

        if (view != original && view is IFocusable focusable)
            return focusable;

        var visualChildren = from != null && view.VisualChildren.Contains(from)
            ? fromChildSelector(view.VisualChildren, from)
            : childSelector(view.VisualChildren);

        foreach (var viewVisualChild in visualChildren)
        {
            var result = FindElement(viewVisualChild, original, fromChildSelector, childSelector);
            if (result is not null)
            {
                return result;
            }
        }

        if (view.VisualParent is null || view.IsFocusBoundary)
            return null;

        return FindElement(view.VisualParent, original, fromChildSelector, childSelector, view);
    }

    [Pure]
    private static IView FindLastFocusParent(IView view)
    {
        if (view.IsFocusBoundary || view.VisualParent is null) return view;
        return FindLastFocusParent(view.VisualParent);
    }
}