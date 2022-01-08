using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using SMBLibrary;
using SMBLibrary.Client;

namespace FileTime.Providers.Smb
{
    public class SmbShare : IContainer
    {
        private IReadOnlyList<IItem>? _items;
        private IReadOnlyList<IContainer>? _containers;
        private IReadOnlyList<IElement>? _elements;
        private Func<ISMBClient> _getSmbClient;
        private readonly IContainer? _parent;

        public IReadOnlyList<IItem> Items
        {
            get
            {
                if (_items == null) Refresh();
                return _items!;
            }

            private set => _items = value;
        }

        public IReadOnlyList<IContainer> Containers
        {
            get
            {
                if (_containers == null) Refresh();
                return _containers!;
            }

            private set => _containers = value;
        }

        public IReadOnlyList<IElement> Elements
        {
            get
            {
                if (_elements == null) Refresh();
                return _elements!;
            }

            private set => _elements = value;
        }

        public string Name { get; }

        public string? FullName { get; }

        public bool IsHidden => false;

        public SmbContentProvider Provider { get; }
        IContentProvider IItem.Provider => Provider;

        public event EventHandler? Refreshed;

        public SmbShare(string name, SmbContentProvider contentProvider, IContainer parent, Func<ISMBClient> getSmbClient)
        {
            _parent = parent;
            _getSmbClient = getSmbClient;

            Name = name;
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
            Provider = contentProvider;
        }

        public IContainer CreateContainer(string name)
        {
            throw new NotImplementedException();
        }

        public IElement CreateElement(string name)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public IItem? GetByPath(string path)
        {
            var paths = path.Split(Constants.SeparatorChar);

            var item = Items.FirstOrDefault(i => i.Name == paths[0]);

            if (paths.Length == 1)
            {
                return item;
            }

            if (item is IContainer container)
            {
                return container.GetByPath(string.Join(Constants.SeparatorChar, paths.Skip(1)));
            }

            return null;
        }

        public IContainer? GetParent() => _parent;

        public bool IsExists(string name)
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            var containers = new List<IContainer>();
            var elements = new List<IElement>();

            try
            {
                (containers, elements) = ListFolder(this, Name, string.Empty);
            }
            catch { }

            _containers = containers.AsReadOnly();
            _elements = elements.AsReadOnly();

            _items = _containers.Cast<IItem>().Concat(_elements).ToList().AsReadOnly();
            Refreshed?.Invoke(this, EventArgs.Empty);
        }

        public (List<IContainer> containers, List<IElement> elements) ListFolder(IContainer parent, string shareName, string folderName)
        {
            var containers = new List<IContainer>();
            var elements = new List<IElement>();

            var client = _getSmbClient();
            ISMBFileStore fileStore = client.TreeConnect(shareName, out var status);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                status = fileStore.CreateFile(out object directoryHandle, out FileStatus fileStatus, folderName, AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
                if (status == NTStatus.STATUS_SUCCESS)
                {
                    status = fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> fileList, directoryHandle, "*", FileInformationClass.FileDirectoryInformation);
                    status = fileStore.CloseFile(directoryHandle);

                    foreach (var item in fileList)
                    {
                        if (item is FileDirectoryInformation fileDirectoryInformation && fileDirectoryInformation.FileName != "." && fileDirectoryInformation.FileName != "..")
                        {
                            if ((fileDirectoryInformation.FileAttributes & SMBLibrary.FileAttributes.Directory) == SMBLibrary.FileAttributes.Directory)
                            {
                                containers.Add(new SmbFolder(fileDirectoryInformation.FileName, Provider, this, parent, _getSmbClient));
                            }
                            else
                            {
                                elements.Add(new SmbFile(fileDirectoryInformation.FileName, Provider, parent, _getSmbClient));
                            }
                        }
                    }
                }
            }

            containers = containers.OrderBy(c => c.Name).ToList();
            elements = elements.OrderBy(e => e.Name).ToList();

            return (containers, elements);
        }
    }
}