using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace TrainDatabase.UI.Converters;

/// <summary>Greys the speed readout when the vehicle is stopped, mirroring the old control window.</summary>
public sealed class SpeedToForegroundConverter : IValueConverter
{
    public static readonly SpeedToForegroundConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int and not 0 ? AvaloniaProperty.UnsetValue : Brushes.Gray;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
