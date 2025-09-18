using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Shell.WPF.Converters
{
  public class TrainSpeedToForegroundColorConvert : IValueConverter
  {

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (targetType != typeof(Brush))
        throw new ArgumentException($@"Expected {nameof(targetType)} to be of type {typeof(Brush)}", nameof(targetType));

      int speed = value as int? ?? 0;
      return speed == 0 ? Brushes.Gray : Brushes.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      throw new InvalidOperationException();
    }
  }
}