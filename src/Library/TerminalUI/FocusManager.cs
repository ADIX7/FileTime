using System.Diagnostics.Contracts;
using GeneralInputKey;
using TerminalUI.Controls;
using TerminalUI.Traits;

namespace TerminalUI;

public class FocusManager : IFocusManager
{
    private readonly IRenderEngine _renderEngine;
    private IFocusable? _focused;
    private DateTime _focusLostCandidateTime = DateTime.MinValue;

    public IFocusable? Focused
    {
        get
        {
            if (_focused is not null)
            {
                var visible = _focused.IsVisible;
                var parent = _focused.VisualParent;
                while (parent != null && visible)
                {
                    visible = parent.IsVisible && visible;
                    parent = parent.VisualParent;
                }

                if (!visible)
                {
                    if (_focusLostCandidateTime != DateTime.MinValue)
                    {
                        if (DateTime.Now - _focusLostCandidateTime > TimeSpan.FromMilliseconds(10))
                        {
                            _focused = null;
                            _focusLostCandidateTime = DateTime.MinValue;
                        }
                    }
                    else
                    {
                        _focusLostCandidateTime = DateTime.Now;
                    }
                }
                else
                {
                    _focusLostCandidateTime = DateTime.MinValue;
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

        if (keyEventArgs is {Key: Keys.Tab, SpecialKeysStatus.IsShiftPressed: true})
        {
            FocusLastElement(Focused);
            keyEventArgs.Handled = true;
        }
        else if (keyEventArgs.Key == Keys.Tab)
        {
            FocusFirstElement(Focused);
            keyEventArgs.Handled = true;
        }
    }

    public void FocusFirstElement(IView view, IView? from = null) =>
        FocusElement(
            view,
            (views, fromView) => views.SkipWhile(x => x != fromView).Skip(1),
            c => c.Reverse(),
            from
        );

    public void FocusLastElement(IView view, IView? from = null) =>
        FocusElement(
            view,
            (views, fromView) => views.TakeWhile(x => x != fromView).Reverse(),
            c => c,
            from
        );


    private void FocusElement(
        IView view,
        Func<IEnumerable<IView>, IView, IEnumerable<IView>> fromChildSelector,
        Func<IEnumerable<IView>, IEnumerable<IView>> childSelector,
        IView? from = null
    )
    {
        if (Focused is null) return;

        var element = FindElement(view,
            view,
            fromChildSelector,
            from: from
        );

        if (element is null)
        {
            var topParent = FindLastFocusParent(view);
            element = FindElement(
                topParent,
                view,
                fromChildSelector,
                childSelector,
                from
            );
        }

        if (element is null) return;

        _renderEngine.RequestRerender(element);
        _renderEngine.RequestRerender(view);
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