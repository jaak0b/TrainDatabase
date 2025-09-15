using Persistence;
using Core.Controller;
using System;
using System.Windows.Controls;
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

      Function = new(ServiceProvider, functionModel);
    }

    private IServiceProvider ServiceProvider { get; }

    private FunctionModel FunctionModel { get; }

    internal FunctionController Function { get; }
  }
}