using Persistence;
using System;
using System.Threading.Tasks;
using Persistence.Models;

namespace Shell.WPF.TrainControl.FunctionButton
{
  internal class TimerButton : FunctionBase
  {
    public TimerButton(IServiceProvider serviceProvider, FunctionModel functionModel) : base(
                                                                                             serviceProvider,
                                                                                             functionModel)
    {
      PreviewMouseDown += async (sender, e) =>
                          {
                            VehicleFunction.SetState(true);
                            await Task.Delay(new TimeSpan(0, 0, functionModel.Time));
                            VehicleFunction.SetState(false);
                          };
      VehicleFunction.StateChanged += (a, state) => Dispatcher.Invoke(() => IsEnabled = !state);
    }
  }
}