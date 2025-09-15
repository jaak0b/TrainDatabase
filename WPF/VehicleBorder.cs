using System.Windows.Controls;
using Persistence.Entities;

namespace Shell.WPF;

public class VehicleBorder : Border
{
  public VehicleEntity Vehicle { get; set; } = default!;
}