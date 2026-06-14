using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace TrainDatabase.UI.Converters;

/// <summary>Converts image <c>byte[]</c> to an Avalonia <see cref="Bitmap"/> for binding to Image.Source.</summary>
public sealed class BytesToBitmapConverter : IValueConverter
{
    public static readonly BytesToBitmapConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte[] { Length: > 0 } bytes)
        {
            try
            {
                using MemoryStream stream = new(bytes);
                return new Bitmap(stream);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
