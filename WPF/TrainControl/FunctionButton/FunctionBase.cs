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
    public FunctionBase(IServiceProvider serviceProvider, FunctionModel functionModel)
    {
      ServiceProvider = serviceProvider;
      FunctionModel = functionModel;
      FunctionButton.ApplyStyle(this, FunctionModel);

      VehicleFunction = new(ServiceProvider, functionModel);
    }

    private IServiceProvider ServiceProvider { get; }

    private FunctionModel FunctionModel { get; }

    internal VehicleFunctionPresenter VehicleFunction { get; }
  }
}