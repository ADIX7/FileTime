namespace FileTime.Providers.Smb.Persistence
{
    public class SmbServer
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}