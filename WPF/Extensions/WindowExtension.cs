using System;
using System.Linq;
using System.Windows;

namespace Shell.WPF.Extensions
{
  internal static class WindowExtension
  {
    internal static void OpenOneInstance<T>(this T window) where T : Window, new()
    {
      if (Application.Current.Windows.OfType<T>().FirstOrDefault() is T settings)
      {
        settings.WindowState = WindowState.Normal;
        settings.Activate();
      }
      else
      {
        ((T)Activator.CreateInstance(typeof(T))).Show();
      }
    }
  }
}