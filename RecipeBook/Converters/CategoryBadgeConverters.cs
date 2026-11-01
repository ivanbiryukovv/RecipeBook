using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RecipeBook.Converters;

public class CategoryToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var category = value as string;
        return category switch
        {
            "Основные блюда" => new SolidColorBrush(Color.FromRgb(224, 231, 255)),
            "Супы" => new SolidColorBrush(Color.FromRgb(254, 243, 199)),
            "Салаты" => new SolidColorBrush(Color.FromRgb(220, 252, 231)),
            "Десерты" => new SolidColorBrush(Color.FromRgb(243, 232, 255)),
            "Выпечка" => new SolidColorBrush(Color.FromRgb(255, 237, 213)),
            "Напитки" => new SolidColorBrush(Color.FromRgb(224, 242, 254)),
            _ => new SolidColorBrush(Color.FromRgb(241, 245, 249))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CategoryToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var category = value as string;
        return category switch
        {
            "Основные блюда" => new SolidColorBrush(Color.FromRgb(67, 56, 202)),
            "Супы" => new SolidColorBrush(Color.FromRgb(180, 83, 9)),
            "Салаты" => new SolidColorBrush(Color.FromRgb(21, 128, 61)),
            "Десерты" => new SolidColorBrush(Color.FromRgb(109, 40, 217)),
            "Выпечка" => new SolidColorBrush(Color.FromRgb(194, 65, 12)),
            "Напитки" => new SolidColorBrush(Color.FromRgb(3, 105, 161)),
            _ => new SolidColorBrush(Color.FromRgb(71, 85, 105))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
