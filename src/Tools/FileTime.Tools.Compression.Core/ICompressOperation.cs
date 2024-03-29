﻿using FileTime.Core.Models;

namespace FileTime.Tools.Compression;

public interface ICompressOperation : IDisposable
{
    Task<IEnumerable<IDisposable>> CompressElement(IElement element, string key);
    void SaveTo(Stream stream);
}