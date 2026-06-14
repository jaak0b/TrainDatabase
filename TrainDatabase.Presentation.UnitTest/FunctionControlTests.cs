using TrainDatabase.Core.Domain;
using TrainDatabase.Core.Presenters;
using TrainDatabase.Presentation.Infrastructure;
using TrainDatabase.Presentation.UnitTest.Fakes;
using TrainDatabase.Presentation.ViewModels;

namespace TrainDatabase.Presentation.UnitTest;

[TestFixture]
public class FunctionControlTests
{
    [Test]
    public async Task SwitchFunction_Activate_TogglesStateAndSendsCommand()
    {
        FakeVehicleControlService control = new();
        Vehicle vehicle = new() { Address = 3 };
        VehicleFunction function = new() { Address = 1, Name = "Light", ButtonType = ButtonType.Switch, IsActive = true };
        VehicleFunctionViewModel vm = new(vehicle, function, control);

        await vm.ActivateCommand.ExecuteAsync(null);
        Assert.Multiple(() =>
        {
            Assert.That(vm.IsActive, Is.True);
            Assert.That(control.FunctionCalls, Is.EqualTo(new[] { (1, true) }));
        });

        await vm.ActivateCommand.ExecuteAsync(null);
        Assert.Multiple(() =>
        {
            Assert.That(vm.IsActive, Is.False);
            Assert.That(control.FunctionCalls, Has.Count.EqualTo(2));
            Assert.That(control.FunctionCalls[1], Is.EqualTo((1, false)));
        });
    }

    [Test]
    public async Task PushButton_Activate_PulsesOnThenOff()
    {
        FakeVehicleControlService control = new();
        Vehicle vehicle = new() { Address = 3 };
        VehicleFunction function = new() { Address = 2, ButtonType = ButtonType.PushButton };
        VehicleFunctionViewModel vm = new(vehicle, function, control);

        await vm.ActivateCommand.ExecuteAsync(null);

        Assert.That(control.FunctionCalls, Is.EqualTo(new[] { (2, true), (2, false) }));
    }

    [Test]
    public void ManualControl_BuildsFunctionButtons_FromActiveVehicleFunctions()
    {
        FakeVehiclePresenter presenter = new(new Vehicle
        {
            Address = 7,
            Functions =
            {
                new VehicleFunction { Address = 0, Name = "Light", ButtonType = ButtonType.Switch, IsActive = true },
                new VehicleFunction { Address = 1, Name = "Horn", ButtonType = ButtonType.PushButton, IsActive = true },
                new VehicleFunction { Address = 2, Name = "Hidden", IsActive = false },
            },
        });
        VehiclePresenterFactory factory = _ => presenter;
        VehicleManualControlViewModel vm = new(7, factory, new FakeVehicleControlService(), new FakeClientPresenter(), new ImmediateUiDispatcher());

        Assert.That(vm.Functions, Has.Count.EqualTo(2));
        Assert.That(vm.Functions.Select(f => f.Label), Is.EqualTo(new[] { "Light", "Horn" }));
    }

    [Test]
    public void ManualControl_LiveFunctionState_UpdatesSwitchButton()
    {
        FakeVehiclePresenter presenter = new(new Vehicle
        {
            Address = 7,
            Functions = { new VehicleFunction { Address = 0, Name = "Light", ButtonType = ButtonType.Switch, IsActive = true } },
        });
        VehiclePresenterFactory factory = _ => presenter;
        VehicleManualControlViewModel vm = new(7, factory, new FakeVehicleControlService(), new FakeClientPresenter(), new ImmediateUiDispatcher());

        presenter.FunctionStatesSubject.OnNext(new Dictionary<ushort, bool> { [0] = true });

        Assert.That(vm.Functions[0].IsActive, Is.True);
    }
}
