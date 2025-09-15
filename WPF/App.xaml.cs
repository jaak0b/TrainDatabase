using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Autofac.Extensions.DependencyInjection;
using Composition;
using Shell.WPF;
using Z21;

namespace Shell.WPF
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private IServiceProvider? serviceProvider;

    public App()
    {
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

      string logFilePath = Path.Combine(Configuration.ApplicationData.LogDirectory.FullName, "log.txt");
      Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                            .Enrich.FromLogContext()
                                            .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                                            .WriteTo.Console(LogEventLevel.Debug, theme: AnsiConsoleTheme.Sixteen)
                                            .CreateLogger();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      Exception? ex = e.ExceptionObject as Exception;
      Log.Logger.Error($"{ex?.Message}");
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
      try
      {
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

    [DllImport("Kernel32")]
    public extern static void AllocConsole();

    [DllImport("Kernel32")]
    public extern static void FreeConsole();
  }
}