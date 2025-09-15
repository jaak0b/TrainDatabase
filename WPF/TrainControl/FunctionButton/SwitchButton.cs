using Persistence;
using Core.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using Core.Presenters;
using Persistence.Enums;
using Persistence.Models;

namespace Shell.WPF.TrainControl.FunctionButton
{
  internal class SwitchButton : ToggleButton
  {
    public SwitchButton(IServiceProvider serviceProvider, FunctionModel functionModel)
    {
      if (functionModel.ButtonType is not ButtonType.Switch)
      {
        throw new ApplicationException($"Button is type {functionModel.ButtonType} but should be {ButtonType.Switch}");
      }

      ServiceProvider = serviceProvider;
      FunctionModel = functionModel;
      FunctionButton.ApplyStyle(this, FunctionModel);

      VehicleFunction = new(ServiceProvider, functionModel);
      VehicleFunction.StateChanged += (a, state) => Dispatcher.Invoke(() => IsChecked = state);
      Click += (a, b) => VehicleFunction.SetState(IsChecked ?? false);
    }

    private IServiceProvider ServiceProvider { get; }

    private FunctionModel FunctionModel { get; }

    private VehicleFunctionPresenter VehicleFunction { get; }
  }
}