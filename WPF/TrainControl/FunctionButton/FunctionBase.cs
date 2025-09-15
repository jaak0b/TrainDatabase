using Persistence;
using Core.Controller;
using System;
using System.Windows.Controls;
using Core.Presenters;
using Persistence.Models;

namespace Shell.WPF.TrainControl.FunctionButton
{
  abstract internal class FunctionBase : Button
  {
    public FunctionBase(IServiceProvider serviceProvider, VehicleFunctionEntity vehicleFunctionEntity)
    {
      ServiceProvider = serviceProvider;
      FunctionModel = vehicleFunctionEntity;
      FunctionButton.ApplyStyle(this, FunctionModel);

      VehicleFunction = new(ServiceProvider, vehicleFunctionEntity);
    }

    private IServiceProvider ServiceProvider { get; }

    private VehicleFunctionEntity FunctionModel { get; }

    internal VehicleFunctionPresenter VehicleFunction { get; }
  }
}