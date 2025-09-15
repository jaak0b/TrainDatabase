using System;

namespace Persistence.Extensions
{
  public static class StringExtensions
  {
    public static bool IsNullOrWhiteSpace(this string e)
    {
      return string.IsNullOrWhiteSpace(e);
    }

    public static bool IsNullOrWhiteSpace(this string e, out string value)
    {
      value = e;
      return string.IsNullOrEmpty(e);
    }

    public static bool IsDecimal(this string e)
    {
      return decimal.TryParse(e, out _);
    }

    public static bool IsInt(this string e)
    {
      return int.TryParse(e, out _);
    }
  }
}