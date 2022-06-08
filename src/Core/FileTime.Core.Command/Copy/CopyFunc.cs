using FileTime.Core.Models;

namespace FileTime.Core.Command.Copy;

public delegate Task CopyFunc(AbsolutePath from, AbsolutePath to, CopyCommandContext context);