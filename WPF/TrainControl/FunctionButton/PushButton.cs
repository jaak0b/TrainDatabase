using Persistence;
using System;
using Persistence.Entities;

namespace Shell.WPF.TrainControl.FunctionButton
{
  internal class PushButton : FunctionBase
  {
    public PushButton(IServiceProvider serviceProvider, VehicleFunctionEntity vehicleFunctionEntity) : base(
                                                                                            serviceProvider,
                                                                                            vehicleFunctionEntity)
    {
      PreviewMouseDown += (sender, e) => VehicleFunction.SetState(true);
      PreviewMouseUp += (sender, e) => VehicleFunction.SetState(false);
    }
  }
}