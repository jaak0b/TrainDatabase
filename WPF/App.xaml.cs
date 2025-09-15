using Helper;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Composition;
using Z21;

namespace Shell.WPF
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private IServiceProvider? serviceProvider;

    private void OnStartup(object sender, StartupEventArgs e)
    {
      try
      {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_OnUnobservedTaskException;

        string logFilePath = Path.Combine(Configuration.ApplicationData.LogDirectory.FullName, "log.txt");
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                              .Enrich.FromLogContext()
                                              .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                                              .WriteTo.Console(LogEventLevel.Debug, theme: AnsiConsoleTheme.Sixteen)
                                              .CreateLogger();

        serviceProvider = Bootstrapper.Initialize(new ShellIocModule(), Log.Logger);

        if (Configuration.OpenDebugConsoleOnStart || Debugger.IsAttached)
        {
          AllocConsole();
        }

        Client client = serviceProvider.GetRequiredService<Client>();
        client.Connect(Configuration.ClientIP);
        serviceProvider.GetRequiredService<MainWindow>().Show();
      }
      catch (Exception ex)
      {
        Log.Logger.Fatal(ex, $"Failed to initialize the application!");
        MessageBox.Show(ex.Message, "Fatal error");
        Environment.Exit(1);
      }
    }

    private static void TaskScheduler_OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
      Log.Logger.Fatal(e.Exception, "Unhandled exception: {ExceptionMessage}", e.Exception?.Message);
      e.SetObserved();
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      Log.Logger.Fatal(e.Exception, "Unhandled exception: {ExMessage}", e.Exception?.Message);
      e.Handled = true;
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      if (e.ExceptionObject is Exception exception)
        Log.Logger.Fatal(exception, "Unhandled exception: {ExMessage}", exception?.Message);
      else
        Log.Logger.Fatal("Unhandled error: {ErrorMessage}", e.ExceptionObject);
    }

    [DllImport("Kernel32")]
    public extern static void AllocConsole();

    [DllImport("Kernel32")]
    public extern static void FreeConsole();
  }
}