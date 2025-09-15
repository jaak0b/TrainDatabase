using System.Windows;

namespace Shell.WPF.Extensions
{
  internal static class WindowExtension
  {

    /// <summary>
    /// Shows the window if it's not visible, or activates and restores it if already open.
    /// </summary>
    public static void ShowOrActivate(this Window? window)
    {
      if (window == null)
        return;

      if (window.IsVisible)
      {
        if (window.WindowState == WindowState.Minimized)
          window.WindowState = WindowState.Normal;

        window.Activate();

        // Optional: force focus reliably
        window.Topmost = true;
        window.Topmost = false;
      }
      else
      {
        window.Show();
      }
    }
    
    /// <summary>
    /// Shows the window if it's not visible, or activates and restores it if already open.
    /// </summary>
    public static void ShowDialogOrActivate(this Window? window)
    {
      if (window == null)
        return;

      if (window.IsVisible)
      {
        if (window.WindowState == WindowState.Minimized)
          window.WindowState = WindowState.Normal;

        window.Activate();

        // Optional: force focus reliably
        window.Topmost = true;
        window.Topmost = false;
      }
      else
      {
        window.ShowDialog();
      }
    }
  }
}