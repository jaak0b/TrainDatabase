using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Core.Factories;
using Core.Presenters;
using Helper;
using Persistence.Model;
using Persistence.Ports;

namespace Shell.WPF.ViewModels
{
  public delegate VehicleViewModel VehicleViewModelFactory(int vehicleId);

  public class VehicleViewModel : INotifyPropertyChanged
  {
    public VehicleViewModel(VehiclePresenterFactory presenterFactory, int vehicleId, IVehicleRepository vehicleRepository)
    {
      VehiclePresenter = presenterFactory(vehicleId);
      Vehicle = vehicleRepository.GetVehicleByIdRequired(vehicleId);


      string path = Path.Combine(Configuration.ApplicationData.VehicleImages.FullName, Vehicle?.ImageName ?? "");
      if (File.Exists(path))
      {
        VehicleImage = LoadPhoto(path);
      }
      else
      {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("NotFound.png"));
        using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        VehicleImage = LoadPhoto(stream!);
      }
    }

    public BitmapImage VehicleImage { get; }

    public VehiclePresenter VehiclePresenter { get; }

    public Vehicle Vehicle { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
  }
}