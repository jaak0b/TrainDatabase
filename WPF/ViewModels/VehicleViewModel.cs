using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Core;
using Core.Presenters;
using Helper;
using Persistence.Model;
using Persistence.Ports;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleViewModel VehicleViewModelFactory(int vehicleId);

  public class VehicleViewModel
  {
    private readonly IClientAdapter clientAdapter;

    public VehicleViewModel(VehiclePresenterFactory presenterFactory, int vehicleId, IClientAdapter clientAdapter)
    {
      this.clientAdapter = clientAdapter;
      VehiclePresenter = presenterFactory(vehicleId);
      Vehicle = VehiclePresenter.Vehicle.ToReactiveProperty()!;

      SpeedDisplayText = VehiclePresenter.Speed
                                         .CombineLatest(VehiclePresenter.Direction, clientAdapter.IsConnected, (speed, direction, isConnected) => GetSpeedDisplayTest(speed, direction, isConnected))
                                         .ToReadOnlyReactiveProperty();

      VehicleDisplayText = Vehicle.Select(vehicle => $"#{vehicle.Address} - {vehicle.Name}").ToReadOnlyReactiveProperty();

      string path = Path.Combine(Configuration.ApplicationData.VehicleImages.FullName, Vehicle.Value.ImageName);
      if (File.Exists(path))
      {
        VehicleImage.Value = LoadPhoto(path);
      }
      else
      {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("NotFound.png"));
        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        VehicleImage.Value = LoadPhoto(stream!);
      }
    }


    public ReactiveProperty<BitmapImage> VehicleImage { get; } = new();

    public IVehiclePresenter VehiclePresenter { get; }

    public ReactiveProperty<Vehicle> Vehicle { get; }

    public ReadOnlyReactiveProperty<string?> SpeedDisplayText { get; }

    public ReadOnlyReactiveProperty<string?> VehicleDisplayText { get; }

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

    private static string GetSpeedDisplayTest(int speed, bool direction, bool isConnected)
    {
      string speedAsString = isConnected ? speed.ToString() : "-";
      string speedText = $"{speedAsString} SS";
      if (direction)
        return "< " + speedText + "  ";
      return "  " + speedText + " >";
    }
  }
}