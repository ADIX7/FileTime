﻿using System.Buffers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public delegate string TextTransformer(string text, Position position, Size size);

public abstract partial class View<T> : IView<T>
{
    private readonly List<IDisposable> _disposables = new();
    [Notify] private T? _dataContext;
    [Notify] private int? _minWidth;
    [Notify] private int? _maxWidth;
    [Notify] private int? _width;
    [Notify] private int _actualWidth;
    [Notify] private int? _minHeight;
    [Notify] private int? _maxHeight;
    [Notify] private int? _height;
    [Notify] private int _actualHeight;
    [Notify] private bool _isVisible = true;
    [Notify] private Thickness _margin = 0;
    [Notify] private IColor? _foreground;
    [Notify] private IColor? _background;
    [Notify] private string? _name;
    [Notify] private IApplicationContext? _applicationContext;
    [Notify] private bool _attached;
    [Notify] private IView? _visualParent;

    protected ObservableCollection<IView> VisualChildren { get; } = new();

    public List<object> Extensions { get; } = new();
    public RenderMethod RenderMethod { get; set; }
    public event Action<IView>? Disposed;
    protected List<string> RerenderProperties { get; } = new();

    protected View()
    {
        RenderMethod = DefaultRenderer;

        RerenderProperties.Add(nameof(Width));
        RerenderProperties.Add(nameof(MinWidth));
        RerenderProperties.Add(nameof(MaxWidth));
        RerenderProperties.Add(nameof(Height));
        RerenderProperties.Add(nameof(MinHeight));
        RerenderProperties.Add(nameof(MaxHeight));
        RerenderProperties.Add(nameof(IsVisible));
        RerenderProperties.Add(nameof(Margin));
        RerenderProperties.Add(nameof(Foreground));
        RerenderProperties.Add(nameof(Background));

        ((INotifyPropertyChanged) this).PropertyChanged += Handle_PropertyChanged;
    }

    public virtual Size GetRequestedSize()
    {
        var size = CalculateSize();

        if (MinWidth.HasValue && size.Width < MinWidth.Value)
            size = size with {Width = MinWidth.Value};
        else if (MaxWidth.HasValue && size.Width > MaxWidth.Value)
            size = size with {Width = MaxWidth.Value};

        if (MinHeight.HasValue && size.Height < MinHeight.Value)
            size = size with {Height = MinHeight.Value};
        else if (MaxHeight.HasValue && size.Height > MaxHeight.Value)
            size = size with {Height = MaxHeight.Value};

        if (Margin.Left != 0 || Margin.Right != 0)
            size = size with {Width = size.Width + Margin.Left + Margin.Right};

        if (Margin.Top != 0 || Margin.Bottom != 0)
            size = size with {Height = size.Height + Margin.Top + Margin.Bottom};

        return size;
    }

    protected abstract Size CalculateSize();

    private void Handle_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (Attached
            && e.PropertyName is not null
            && (e.PropertyName == nameof(IView.DataContext)
                || RerenderProperties.Contains(e.PropertyName)
            )
           )
        {
            ApplicationContext?.RenderEngine.RequestRerender(this);
        }

        if (e.PropertyName == nameof(Attached))
        {
            foreach (var visualChild in VisualChildren)
            {
                visualChild.Attached = Attached;
            }
        }
        else if (e.PropertyName == nameof(ApplicationContext))
        {
            foreach (var visualChild in VisualChildren)
            {
                visualChild.ApplicationContext = ApplicationContext;
            }
        }
        else if (e.PropertyName == nameof(IsVisible))
        {
            ApplicationContext?.RenderEngine.VisibilityChanged(this);
        }
    }

    protected abstract bool DefaultRenderer(RenderContext renderContext, Position position, Size size);

    public bool Render(RenderContext renderContext, Position position, Size size)
    {
        if (!Attached)
            throw new InvalidOperationException("Cannot render unattached view");

        if (!IsVisible) return false;

        ActualWidth = size.Width;
        ActualHeight = size.Height;

        if (RenderMethod is null)
        {
            throw new NullReferenceException(
                nameof(RenderMethod)
                + " is null, cannot render content of "
                + GetType().Name
                + " with DataContext of "
                + DataContext?.GetType().Name);
        }

        if (Margin.Left != 0 || Margin.Top != 0 || Margin.Right != 0 || Margin.Bottom != 0)
        {
            position = new Position(
                X: position.X + Margin.Left,
                Y: position.Y + Margin.Top
            );

            size = new Size(
                size.Width - Margin.Left - Margin.Right,
                size.Height - Margin.Top - Margin.Bottom
            );
        }

        return RenderMethod(renderContext, position, size);
    }

    protected void RenderEmpty(RenderContext renderContext, Position position, Size size)
    {
        var driver = renderContext.ConsoleDriver;
        driver.ResetColor();

        var placeHolder = new string(ApplicationContext!.EmptyCharacter, size.Width);
        for (var i = 0; i < size.Height; i++)
        {
            driver.SetCursorPosition(position with {Y = position.Y + i});
            driver.Write(placeHolder);
        }
    }

    protected void RenderText(
        IList<string> textLines,
        IConsoleDriver driver,
        Position position,
        Size size,
        TextTransformer? textTransformer = null)
    {
        for (var i = 0; i < textLines.Count; i++)
        {
            var currentPosition = position with {Y = position.Y + i};
            var text = textLines[i];

            if (textTransformer is not null)
            {
                text = textTransformer(text, currentPosition, size);
            }

            if (text.Length > size.Width)
            {
                text = text[..size.Width];
            }

            driver.SetCursorPosition(currentPosition);
            driver.Write(text);
        }
    }

    protected void RenderText(
        string text,
        IConsoleDriver driver,
        Position position,
        Size size,
        TextTransformer? textTransformer = null)
    {
        for (var i = 0; i < size.Height; i++)
        {
            var currentPosition = position with {Y = position.Y + i};
            var finalText = text;

            if (textTransformer is not null)
            {
                finalText = textTransformer(finalText, currentPosition, size);
            }

            if (finalText.Length > size.Width)
            {
                finalText = finalText[..size.Width];
            }

            driver.SetCursorPosition(currentPosition);
            driver.Write(finalText);
        }
    }

    protected void RenderText(
        char content,
        IConsoleDriver driver,
        Position position,
        Size size)
    {
        var contentString = new string(content, size.Width);

        for (var i = 0; i < size.Height; i++)
        {
            var currentPosition = position with {Y = position.Y + i};

            driver.SetCursorPosition(currentPosition);
            driver.Write(contentString);
        }
    }

    protected void SetColorsForDriver(RenderContext renderContext)
    {
        var driver = renderContext.ConsoleDriver;

        var foreground = Foreground ?? renderContext.Foreground;
        var background = Background ?? renderContext.Background;
        if (foreground is not null)
        {
            driver.SetForegroundColor(foreground);
        }

        if (background is not null)
        {
            driver.SetBackgroundColor(background);
        }
    }

    public TChild CreateChild<TChild>() where TChild : IView<T>, new()
    {
        var child = new TChild();
        return AddChild(child);
    }

    public TChild CreateChild<TChild, TDataContext>(Func<T?, TDataContext?> dataContextMapper)
        where TChild : IView<TDataContext>, new()
    {
        var child = new TChild();
        return AddChild(child, dataContextMapper);
    }

    public virtual TChild AddChild<TChild>(TChild child) where TChild : IView<T>
    {
        child.DataContext = DataContext;
        var mapper = new DataContextMapper<T, T>(this, child, d => d);
        SetupNewChild(child, mapper);

        return child;
    }

    public virtual TChild AddChild<TChild, TDataContext>(TChild child, Func<T?, TDataContext?> dataContextMapper)
        where TChild : IView<TDataContext>
    {
        child.DataContext = dataContextMapper(DataContext);
        var mapper = new DataContextMapper<T, TDataContext>(this, child, dataContextMapper);
        SetupNewChild(child, mapper);

        return child;
    }

    private void SetupNewChild(IView child, IDisposable dataContextmapper)
    {
        child.ApplicationContext = ApplicationContext;
        child.Attached = Attached;
        child.VisualParent = this;
        VisualChildren.Add(child);

        AddDisposable(dataContextmapper);
        child.AddDisposable(dataContextmapper);
    }

    public virtual void RemoveChild<TDataContext>(IView<TDataContext> child)
    {
        var mappers = _disposables
            .Where(d => d is DataContextMapper<T, TDataContext> mapper && mapper.Target == child)
            .ToList();

        foreach (var mapper in mappers)
        {
            mapper.Dispose();
            RemoveDisposable(mapper);
        }

        child.Attached = false;
    }

    public void AddDisposable(IDisposable disposable) => _disposables.Add(disposable);
    public void RemoveDisposable(IDisposable disposable) => _disposables.Remove(disposable);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            var arrayPool = ArrayPool<IDisposable>.Shared;
            var disposablesCount = _disposables.Count;
            var disposables = arrayPool.Rent(disposablesCount);
            _disposables.CopyTo(disposables);
            for (var i = 0; i < disposablesCount; i++)
            {
                disposables[i].Dispose();
            }

            arrayPool.Return(disposables, true);

            _disposables.Clear();
            Disposed?.Invoke(this);
        }
    }
}