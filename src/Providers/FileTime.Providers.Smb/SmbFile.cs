using FileTime.Core.Models;
using FileTime.Core.Providers;
using SMBLibrary.Client;

namespace FileTime.Providers.Smb
{
    public class SmbFile : IElement
    {
        public bool IsSpecial => false;

        public string Name { get; }

        public string? FullName { get; }

        public bool IsHidden => false;

        public IContentProvider Provider { get; }

        private readonly Func<ISMBClient> _getSmbClient;

        public SmbFile(string name, SmbContentProvider provider, IContainer parent, Func<ISMBClient> getSmbClient)
        {
            Name = name;
            FullName = parent.FullName + Constants.SeparatorChar + Name;

            Provider = provider;
            _getSmbClient = getSmbClient;
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public string GetPrimaryAttributeText()
        {
            return "";
        }
    }
}