using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence.Database;
using Persistence.Entities;
using Shell.WPF.DatabaseImport;
using Shell.WPF.Extensions;
using Shell.WPF.ViewModels;
using Z21;

namespace Shell.WPF.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    private readonly DatabaseImportView databaseImportView;
    private readonly ILogger logger;
    private readonly VehicleTileViewFactory vehicleTileViewFactory;
    private readonly static Mutex mutex = new(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

    public MainWindow(IServiceProvider serviceProvider, DatabaseImportView databaseImportView, ILogger<MainWindow> logger, VehicleTileViewFactory vehicleTileViewFactory, MainWindowViewModel mainWindowViewModel)
    {
      this.databaseImportView = databaseImportView;
      this.logger = logger;
      this.vehicleTileViewFactory = vehicleTileViewFactory;
      try
      {
        InitializeComponent();
        DataContext = mainWindowViewModel;

        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Db = ServiceProvider.GetService<Database>()!;
        Client = ServiceProvider.GetService<Client>()!;

        if (!mutex.WaitOne(TimeSpan.Zero, true))
        {
          MessageBox.Show("Achtung mehr als eine Instanz der Software kann nicht geöffnet werden!");
          Application.Current.Shutdown();
          return;
        }
      }
      catch (Exception exception)
      {
        Close();
        logger.LogError(exception, "Failed to create {MainWindowName}", nameof(MainWindow));
      }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IServiceProvider ServiceProvider { get; } = default!;

    private Client Client { get; } = default!;

    private Database Db { get; } = default!;

    protected void OnPropertyChanged([CallerMemberName] string name = null!)
    {
      PropertyChanged?.Invoke(this, new(name));
    }

    private static BitmapImage LoadPhoto(string path)
    {
      BitmapImage bmi = new();
      bmi.BeginInit();
      bmi.CacheOption = BitmapCacheOption.OnLoad;
      bmi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
      bmi.UriSource = new(path);
      bmi.EndInit();
      return bmi;
    }

    private static BitmapImage LoadPhoto(Stream stream)
    {
      BitmapImage bitmap = new();
      bitmap.BeginInit();
      bitmap.StreamSource = stream;
      bitmap.CacheOption = BitmapCacheOption.OnLoad;
      bitmap.EndInit();
      bitmap.Freeze();
      return bitmap;
    }

    private async void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      if (e.ClickCount == 2 && sender is VehicleBorder border)
      {
        await Task.Delay(150);
        TrainControl.TrainControl.CreatTrainControlWindow(ServiceProvider, border.Vehicle);
      }
    }

    private void MiImportNewDatabase(object sender, RoutedEventArgs e)
    {
      databaseImportView.ShowDialogOrActivate();
    }

 
    private void MeasureLoko_Click(object sender, RoutedEventArgs e)
    {
      new Einmessen(ServiceProvider).Show();
    }

    private void Mw_Closing(object sender, CancelEventArgs e)
    {
      Application.Current.Shutdown();
    }

    private void Mw_Loaded(object sender, RoutedEventArgs e)
    {
#if RELEASE
                if (MessageBoxResult.No == MessageBox.Show("Achtung! Es handelt sich bei der Software um eine Alpha version! Es können und werden Bugs auftreten, wenn Sie auf JA drücken, stimmen Sie zu, dass der Entwickler für keinerlei Schäden, die durch die Verwendung der Software entstehen könnten, haftbar ist!", "Haftungsausschluss", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    Application.Current.Shutdown();
                    return;
                }
#endif
      RemoveUnneededImages();
    }

    private void Mw_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      // if (IsActive && !TbSearch.IsFocused)
      // {
      //   TbSearch.Focus();
      // }
    }

    private void OpenVehicleManagement_Click(object sender, RoutedEventArgs e)
    {
      if (Application.Current.Windows.OfType<VehicleManagement>().FirstOrDefault() is VehicleManagement vehicleManagement)
      {
        vehicleManagement.WindowState = WindowState.Normal;
        vehicleManagement.Activate();
      }
      else
      {
        new VehicleManagement(ServiceProvider).Show();
      }
    }

    private void RemoveUnneededImages()
    {
      Task.Run(() =>
               {
                 try
                 {
                   List<string>? images = Db.Vehicles.Select(e => e.ImageName).ToList();
                   string directory = Configuration.ApplicationData.VehicleImages.FullName;
                   Directory.CreateDirectory(directory);
                   foreach (string? item in Directory.GetFiles($"{directory}\\"))
                   {
                     if (!images.Any(e => e == Path.GetFileName(item)))
                     {
                       File.Delete(item);
                     }
                   }
                 }
                 catch
                 {
                 }
               });
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
      if (Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault() is SettingsWindow settings)
      {
        settings.WindowState = WindowState.Normal;
        settings.Activate();
      }
      else
      {
        new SettingsWindow().Show();
      }
    }

    private void MiDeleteDatabase(object sender, RoutedEventArgs e)
    {
      if (MessageBoxResult.Yes == MessageBox.Show("Sicher dass die Datenbank gelöscht werden soll?", "Datenbank löschen", MessageBoxButton.YesNo, MessageBoxImage.Warning))
      {
        Db.DeleteAll();
      }
    }
  }
}