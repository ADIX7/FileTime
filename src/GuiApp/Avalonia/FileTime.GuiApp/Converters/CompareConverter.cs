using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTime.GuiApp.Converters
{
    public enum ComparisonCondition
    {
        Equal,
        GreaterThan,
        LessThan,
        LessThanOrEqual,
        NotEqual,
        GreaterThanOrEqual
    }

    public class CompareConverter : IValueConverter
    {
        public ComparisonCondition ComparisonCondition { get; set; } = ComparisonCondition.Equal;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Compare(value, parameter);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private bool Compare(object? value, object? parameter)
        {
            if (ComparisonCondition == ComparisonCondition.GreaterThan)
            {
                if (value is int valueInt && (parameter is int parameterInt || int.TryParse(parameter?.ToString(), out parameterInt))) return valueInt > parameterInt;
                else if (value is double valueDouble && (parameter is double parameterDouble || double.TryParse(parameter?.ToString(), out parameterDouble))) return valueDouble > parameterDouble;
                else throw new NotSupportedException();
            }
            else if (ComparisonCondition == ComparisonCondition.NotEqual)
            {
                if (value is int valueInt && (parameter is int parameterInt || int.TryParse(parameter?.ToString(), out parameterInt))) return valueInt != parameterInt;
                else if (value is double valueDouble && (parameter is double parameterDouble || double.TryParse(parameter?.ToString(), out parameterDouble))) return valueDouble != parameterDouble;
                return value != parameter;
            }
            else if (ComparisonCondition == ComparisonCondition.Equal)
            {
                if (value is int valueInt && (parameter is int parameterInt || int.TryParse(parameter?.ToString(), out parameterInt))) return valueInt == parameterInt;
                else if (value is double valueDouble && (parameter is double parameterDouble || double.TryParse(parameter?.ToString(), out parameterDouble))) return valueDouble == parameterDouble;
                else if (value?.GetType().IsEnum ?? false && Enum.TryParse(value.GetType(), parameter?.ToString(), out var _)) return value.ToString() == parameter?.ToString();
            }

            return value == parameter;
        }
    }
}