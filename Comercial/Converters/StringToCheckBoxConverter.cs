using System.Globalization;
using System.Windows.Data;

namespace Comercial.Converters;

public class StringToCheckBoxConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return false;

        // Remove espaços e converte para maiúscula
        string stringValue = value.ToString().Trim();

        // Trata string vazia ou só com espaços
        if (string.IsNullOrWhiteSpace(stringValue))
            return false;

        // "0", "00", "000" = false
        if (stringValue.All(c => c == '0'))
            return false;

        // "1", "-1" = true
        if (stringValue == "1" || stringValue == "-1")
            return true;

        // Qualquer outro número diferente de 0
        if (int.TryParse(stringValue, out int intValue))
        {
            return intValue != 0;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // true = "1"
            // false = "0"
            return boolValue ? "1" : "0";
        }

        return "0";
    }
}
