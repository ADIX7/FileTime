using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileTime.Uno.Converters
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

    public class EqualityToVisibilityConverter : IValueConverter
    {
        public ComparisonCondition ComparisonCondition { get; set; } = ComparisonCondition.Equal;

        public object Convert(object value, Type targetType, object parameter, string language) => Compare(value, parameter) ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private bool Compare(object value, object parameter)
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
            if (ComparisonCondition == ComparisonCondition.Equal)
            {
                if (value is int valueInt && (parameter is int parameterInt || int.TryParse(parameter?.ToString(), out parameterInt))) return valueInt == parameterInt;
                else if (value is double valueDouble && (parameter is double parameterDouble || double.TryParse(parameter?.ToString(), out parameterDouble))) return valueDouble == parameterDouble;
                else if (value.GetType().IsEnum && Enum.TryParse(value.GetType(), parameter.ToString(), out var _)) return value.ToString() == parameter.ToString();
            }

            return value == parameter;
        }
    }
}
