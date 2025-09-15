using Persistence;
using System;
using System.Windows.Controls;
using Persistence.Entities;

namespace Shell.WPF
{
  public class VehicleMenuItem : MenuItem
  {
    public VehicleMenuItem(VehicleEntity vehicle, string content, Action<VehicleEntity> onClick)
    {
      Vehicle = vehicle;
      Header = content;
      Click += (a, b) => onClick(Vehicle);
    }

    public VehicleEntity Vehicle { get; set; } = default!;
  }
}