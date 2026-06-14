using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrainDatabase.Core.Ports;

namespace TrainDatabase.Presentation.ViewModels;

/// <summary>Application settings (command-station IP, Arduino port) persisted via the settings store.</summary>
public partial class SettingsViewModel : ViewModelBase
{
    public const string ClientIpKey = "ClientIP";
    public const string ArduinoComPortKey = "ArduinoComPort";
    public const string ArduinoBaudRateKey = "ArduinoBaudrate";

    private readonly ISettingsStore settings;

    [ObservableProperty] private string clientIp = "";
    [ObservableProperty] private string arduinoComPort = "";
    [ObservableProperty] private int arduinoBaudRate;

    public SettingsViewModel(ISettingsStore settings)
    {
        this.settings = settings;
        ClientIp = settings.Get(ClientIpKey) ?? "192.168.0.111";
        ArduinoComPort = settings.Get(ArduinoComPortKey) ?? "";
        ArduinoBaudRate = int.TryParse(settings.Get(ArduinoBaudRateKey), out int rate) ? rate : 9600;
    }

    [RelayCommand]
    private void Save()
    {
        settings.Set(ClientIpKey, ClientIp);
        settings.Set(ArduinoComPortKey, ArduinoComPort);
        settings.Set(ArduinoBaudRateKey, ArduinoBaudRate.ToString());
    }
}
