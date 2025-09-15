using Persistence;
using System;
using Persistence.Models;

namespace Shell.WPF.TrainControl.FunctionButton
{
  internal class PushButton : FunctionBase
  {
    public PushButton(IServiceProvider serviceProvider, FunctionModel functionModel) : base(
                                                                                            serviceProvider,
                                                                                            functionModel)
    {
      PreviewMouseDown += (sender, e) => VehicleFunction.SetState(true);
      PreviewMouseUp += (sender, e) => VehicleFunction.SetState(false);
    }
  }
}