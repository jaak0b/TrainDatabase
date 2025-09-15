using Persistence;
using System.Windows.Controls;
using Persistence.Models;

public class VehicleBorder : Border
{
  public VehicleModel Vehicle { get; set; } = default!;
}