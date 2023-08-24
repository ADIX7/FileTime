using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Providers.Local;

public interface ILocalContentProvider : IContentProvider
{
    NativePath GetNativePath(FullName fullName);
}