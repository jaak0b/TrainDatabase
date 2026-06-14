namespace TrainDatabase.Infrastructure.Extensions;

internal static class StringExtensions
{
    public static bool IsDecimal(this string e) => decimal.TryParse(e, out _);

    public static bool IsInt(this string e) => int.TryParse(e, out _);
}
