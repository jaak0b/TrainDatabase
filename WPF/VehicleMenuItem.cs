using Persistence;
using System;
using System.Windows.Controls;
using Persistence.Models;

namespace Wpf_Application
{
  public class VehicleMenuItem : MenuItem
  {
    public VehicleMenuItem(VehicleModel vehicle, string content, Action<VehicleModel> onClick)
    {
      Vehicle = vehicle;
      Header = content;
      Click += (a, b) => onClick(Vehicle);
    }

    public VehicleModel Vehicle { get; set; } = default!;
  }
}