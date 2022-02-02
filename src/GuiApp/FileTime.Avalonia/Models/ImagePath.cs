namespace FileTime.Avalonia.Models
{
    public class ImagePath
    {
        public string? Path { get; }
        public ImagePathType Type { get; }
        public object? Image { get; }

        public ImagePath(ImagePathType type, string path)
        {
            Path = path;
            Type = type;
        }

        public ImagePath(ImagePathType type, object image)
        {
            Image = image;
            Type = type;
        }
    }
}