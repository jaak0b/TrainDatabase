using System.Windows.Controls;
using Persistence.Models;

namespace Shell.WPF;

public class VehicleBorder : Border
{
  public VehicleEntity Vehicle { get; set; } = default!;
}