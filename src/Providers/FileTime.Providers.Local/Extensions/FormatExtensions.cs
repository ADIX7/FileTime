namespace FileTime.Providers.Local.Extensions
{
    public static class FormatExtensions
    {
        private const long OneKiloByte = 1024;
        private const long OneMegaByte = OneKiloByte * 1024;
        private const long OneGigaByte = OneMegaByte * 1024;
        private const long OneTerraByte = OneGigaByte * 1024;

        public static string ToSizeString(this long fileSize, int precision = 1)
        {
            var fileSizeD = (decimal)fileSize;
            var (size, suffix) = fileSize switch
            {
                > OneTerraByte => (fileSizeD / OneTerraByte, "T"),
                > OneGigaByte => (fileSizeD / OneGigaByte, "G"),
                > OneMegaByte => (fileSizeD / OneMegaByte, "M"),
                > OneKiloByte => (fileSizeD / OneKiloByte, "K"),
                _ => (fileSizeD, "B")
            };

            return string.Format("{0:N" + precision + "}", size).TrimEnd('0').Replace(',', '.').TrimEnd('.') + " " + suffix;
        }
    }
}