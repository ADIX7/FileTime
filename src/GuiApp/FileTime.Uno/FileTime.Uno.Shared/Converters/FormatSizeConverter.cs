using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Uno.Converters
{
    public class FormatSizeConverter : IValueConverter
    {
        private const long OneKiloByte = 1024;
        private const long OneMegaByte = OneKiloByte * 1024;
        private const long OneGigaByte = OneMegaByte * 1024;
        private const long OneTerraByte = OneGigaByte * 1024;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value, int.TryParse(parameter.ToString(), out var prec)) switch
            {
                (long size, true) => ToSizeString(size, prec),
                (long size, false) => ToSizeString(size),
                _ => value
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static string ToSizeString(long fileSize, int precision = 1)
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

            var result = string.Format("{0:N" + precision + "}", size).Replace(',', '.');

            if (result.Contains('.')) result = result.TrimEnd('0').TrimEnd('.');
            return result + " " + suffix;
        }
    }
}
