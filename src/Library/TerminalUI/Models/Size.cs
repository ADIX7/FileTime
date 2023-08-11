using System.Diagnostics;

namespace TerminalUI.Models;

[DebuggerDisplay("Width = {Width}, Height = {Height}")]
public readonly record struct Size(int Width, int Height);