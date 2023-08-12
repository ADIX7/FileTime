using System.Diagnostics;

namespace TerminalUI.Models;

[DebuggerDisplay("RenderedViews = {RenderedViews}, RenderedDisplayViews = {RenderedDisplayViews}, ProcessedViews = {ProcessedViews}")]
public class RenderStatistics
{
    public ulong RenderedViews { get; set; }
    public ulong RenderedDisplayViews { get; set; }
    public ulong ProcessedViews { get; set; }
}