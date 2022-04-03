using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTime.GuiApp.Converters
{
    public class SplitStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path && parameter is string separator)
            {
                return path.Split(separator);
            }
            else if (value is string path2 && parameter is char separator2)
            {
                return path2.Split(separator2);
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}