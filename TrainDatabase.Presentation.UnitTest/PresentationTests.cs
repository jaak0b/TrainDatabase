using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Presentation.Dialogs;
using TrainDatabase.Presentation.Infrastructure;
using TrainDatabase.Presentation.Navigation;
using TrainDatabase.Presentation.UnitTest.Fakes;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class NavigationServiceTests
{
    private sealed class StubViewModel : ViewModelBase;

    [Test]
    public void NavigateTo_SetsCurrent_AndEnablesBack()
    {
        NavigationService navigation = new();
        StubViewModel first = new();
        StubViewModel second = new();

        navigation.NavigateTo(first);
        Assert.That(navigation.Current, Is.SameAs(first));
        Assert.That(navigation.CanGoBack, Is.False);

        navigation.NavigateTo(second);
        Assert.Multiple(() =>
        {
            Assert.That(navigation.Current, Is.SameAs(second));
            Assert.That(navigation.CanGoBack, Is.True);
        });

        navigation.Back();
        Assert.That(navigation.Current, Is.SameAs(first));
    }
}

[TestFixture]
public class DialogServiceTests
{
    [Test]
    public async Task ConfirmAsync_ShowsDialog_ThenResolvesOnAccept()
    {
        DialogService service = new();
        Task<bool> confirm = service.ConfirmAsync("Title", "Sure?");

        Assert.That(service.Current, Is.Not.Null);
        service.Current!.AcceptCommand.Execute(null);

        Assert.Multiple(async () =>
        {
            Assert.That(await confirm, Is.True);
            Assert.That(service.Current, Is.Null);
        });
    }

    [Test]
    public async Task ConfirmAsync_ResolvesFalse_OnCancel()
    {
        DialogService service = new();
        Task<bool> confirm = service.ConfirmAsync("Title", "Sure?");
        service.Current!.CancelCommand.Execute(null);
        Assert.That(await confirm, Is.False);
    }
}

[TestFixture]
public class SettingsViewModelTests
{
    [Test]
    public void Save_PersistsValuesToStore()
    {
        FakeSettingsStore store = new();
        SettingsViewModel vm = new(store)
        {
            ClientIp = "10.0.0.5",
            ArduinoComPort = "COM3",
            ArduinoBaudRate = 115200,
        };

        vm.SaveCommand.Execute(null);

        Assert.Multiple(() =>
        {
            Assert.That(store.Get(SettingsViewModel.ClientIpKey), Is.EqualTo("10.0.0.5"));
            Assert.That(store.Get(SettingsViewModel.ArduinoComPortKey), Is.EqualTo("COM3"));
            Assert.That(store.Get(SettingsViewModel.ArduinoBaudRateKey), Is.EqualTo("115200"));
        });
    }
}

[TestFixture]
public class VehicleManualControlViewModelTests
{
    private static VehicleManualControlViewModel Create(
        FakeVehiclePresenter presenter,
        FakeVehicleControlService control,
        FakeClientPresenter client)
    {
        VehiclePresenterFactory factory = _ => presenter;
        return new VehicleManualControlViewModel(1, factory, control, client, new ImmediateUiDispatcher());
    }

    [Test]
    public void SettingSpeed_SendsDriveCommand()
    {
        FakeVehiclePresenter presenter = new(new Vehicle { Address = 9 });
        FakeVehicleControlService control = new();
        VehicleManualControlViewModel vm = Create(presenter, control, new FakeClientPresenter());

        vm.Speed = 42;

        Assert.That(control.Calls, Has.Count.EqualTo(1));
        Assert.That(control.Calls[0].Speed, Is.EqualTo(42));
    }

    [Test]
    public void LiveUpdateFromPresenter_UpdatesSpeed_WithoutSendingDrive()
    {
        FakeVehiclePresenter presenter = new(new Vehicle { Address = 9 });
        FakeVehicleControlService control = new();
        VehicleManualControlViewModel vm = Create(presenter, control, new FakeClientPresenter());

        presenter.SpeedValue.SetValue(77);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Speed, Is.EqualTo(77));
            Assert.That(control.Calls, Is.Empty);
        });
    }

    [Test]
    public void DisconnectedState_FlowsFromClientPresenter()
    {
        FakeClientPresenter client = new();
        VehicleManualControlViewModel vm = Create(new FakeVehiclePresenter(new Vehicle { Address = 9 }), new FakeVehicleControlService(), client);

        client.IsDisconnectedValue.SetValue(true);

        Assert.That(vm.IsDisconnected, Is.True);
    }
}
