using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Providers.Sftp
{
    public class SftpFile : IElement
    {
        public bool IsSpecial => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string? FullName => throw new NotImplementedException();

        public string? NativePath => throw new NotImplementedException();

        public bool IsHidden => throw new NotImplementedException();

        public bool IsDestroyed => throw new NotImplementedException();

        public SupportsDelete CanDelete => throw new NotImplementedException();

        public bool CanRename => throw new NotImplementedException();

        public IContentProvider Provider => throw new NotImplementedException();

        public Task Delete(bool hardDelete = false)
        {
            throw new NotImplementedException();
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetContent(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IContentReader> GetContentReaderAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IContentWriter> GetContentWriterAsync()
        {
            throw new NotImplementedException();
        }

        public Task<long> GetElementSize(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IContainer? GetParent()
        {
            throw new NotImplementedException();
        }

        public string GetPrimaryAttributeText()
        {
            throw new NotImplementedException();
        }

        public Task Rename(string newName)
        {
            throw new NotImplementedException();
        }
    }
}