using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Core.Factories;
using Core.Presenters;
using Helper;
using Persistence.Model;
using Persistence.Ports;
using Reactive.Bindings;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleViewModel VehicleViewModelFactory(int vehicleId);

  public class VehicleViewModel
  {
    public VehicleViewModel(VehiclePresenterFactory presenterFactory, int vehicleId, IVehicleRepository vehicleRepository)
    {
      VehiclePresenter = presenterFactory(vehicleId);
      Vehicle = vehicleRepository.GetVehicleByIdRequired(vehicleId);

      SpeedDisplayText = VehiclePresenter.Speed.CombineLatest(VehiclePresenter.Direction, GetSpeedDisplayTest).ToReadOnlyReactiveProperty();

      string path = Path.Combine(Configuration.ApplicationData.VehicleImages.FullName, Vehicle?.ImageName ?? "");
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

    private static string GetSpeedDisplayTest(int speed, bool direction)
    {
      string speedText = $"{speed} SS";
      if (direction)
        return "< " + speedText + "  ";
      return "  " + speedText + " >";
    }

    public ReactiveProperty<BitmapImage> VehicleImage { get; } = new();

    public VehiclePresenter VehiclePresenter { get; }

    public Vehicle Vehicle { get; }

    public ReadOnlyReactiveProperty<string?> SpeedDisplayText { get; }

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
  }
}