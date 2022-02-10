namespace FileTime.Providers.Smb.Persistence
{
    public class ServersPersistenceRoot
    {
        public string Key { get; set; }
        public List<SmbServer> Servers { get; set; }
    }
}