using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Extension;
using Core.TDO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Database;
using Persistence.Enums;
using Persistence.Models;
using Z21;
using Z21.Events;
using Z21.Model;

namespace Core.Presenters
{
  public class VehiclePresenter
  {
    private LokInfoData? liveData = default!;
    private int speed;

    public VehiclePresenter(IServiceProvider serviceProvider, VehicleEntity vehicleEntity)
    {
      ServiceProvider = serviceProvider;
      Db = ServiceProvider.GetService<Database>()!;
      Z21Client = ServiceProvider.GetService<Client>()!;
      SetVehicleModel(vehicleEntity);

      Z21Client.OnGetLocoInfo += Z21Client_OnGetLocoInfo;

      Db.ChangeTracker.StateChanged += (a, b) => SetVehicleModel(VehicleEntity!);

      Z21Client.GetLocoInfo(new(VehicleEntity.Address));

      Z21Client.ClientReachabilityChanged += (a, b) => Z21Client.GetLocoInfo(new(VehicleEntity.Address));
    }

    /// <summary>
    /// Occurs when a property changes.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// Stores the direction of travel for the current vehicle. This property should be used for one way binding (Source to target).
    /// </summary>
    /// <remarks>
    /// To switch direction call <see cref="SwitchDirection"/>.
    /// </remarks>
    public bool Direction => GetDrivingDirection(VehicleEntity, LiveData?.DrivingDirection ?? true);

    /// <summary>
    /// Holds the date and time when the user last interacted with the controls. 
    /// </summary>
    public DateTime LastUserInteraction { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Stores and sets the speed for the current vehicle. 
    /// </summary>
    public int Speed
    {
      get => speed;
      set
      {
        if (value < 0 || value > Client.maxDccStep)
        {
          return;
        }

        speed = value == 1 ? speed > 1 ? 0 : 2 : value;

        if ((LiveData?.Speed ?? int.MinValue) != speed)
        {
          _ = SetLocoDrive(speed);
        }

        OnStateChanged();
      }
    }

    private Database Db { get; } = default!;

    /// <summary>
    /// Holds the date and time when the last speed change occured.
    /// </summary>
    private DateTime LastSpeedChange { get; set; } = DateTime.MinValue;

    private LokInfoData? LiveData
    {
      get => liveData;
      set
      {
        liveData = value;
        if ((DateTime.Now - LastUserInteraction).TotalSeconds > 2)
        {
          Speed = value?.Speed ?? Speed;
        }

        OnStateChanged();
      }
    }

    private List<MultiTractionItem> MultiTractionItems { get; } = new();

    private IServiceProvider ServiceProvider { get; }

    private VehicleEntity SlowestVehicleInTractionList { get; set; } = default!;

    private VehicleEntity VehicleEntity { get; set; } = default!;

    private Client Z21Client { get; } = default!;

    /// <summary>
    /// Sets the direction of travel.
    /// </summary>
    public async void SetDirection(bool direction)
    {
      await SetLocoDrive(drivingDirection: direction);
    }

    /// <summary>
    /// Switches the direction of travel.
    /// </summary>
    public async void SwitchDirection()
    {
      await SetLocoDrive(drivingDirection: !LiveData?.DrivingDirection);
    }

    private static LokInfoData GetLocoInfoData(int speedstep, bool direction, bool inUse, VehicleEntity Vehicle)
    {
      return new()
             {
               Adresse = new(Vehicle.Address),
               DrivingDirection = direction,
               InUse = inUse,
               Speed = (byte)speedstep
             };
    }

    private static double GetSlowestVehicleSpeed(int speedstep, bool direction, MultiTractionItem traction)
    {
      if (!IsVehicleMeasured(traction))
      {
        return double.NaN;
      }

      if (!traction.Vehicle.InvertTraction)
      {
        return direction
                 ? traction.TractionForward.GetYValue(speedstep)
                 : traction.TractionBackward.GetYValue(speedstep);
      }
      else
      {
        return direction
                 ? traction.TractionBackward.GetYValue(speedstep)
                 : traction.TractionForward.GetYValue(speedstep);
      }
    }

    private static bool IsVehicleMeasured(MultiTractionItem item)
    {
      return (item.TractionForward?.Any() ?? false) && (item.TractionBackward?.Any() ?? false);
    }

    private async Task DeterminSlowestVehicleInList()
    {
      await Task.Run(
                     () =>
                     {
                       List<MultiTractionItem>? list = MultiTractionItems
                                                      .Where(
                                                             e => e.Vehicle.Type == VehicleType.Lokomotive &&
                                                                  e.TractionForward.Any() && e.TractionBackward.Any())
                                                      .ToList();

                       if (list.Any())
                       {
                         SlowestVehicleInTractionList =
                           list.Aggregate(
                                          (cur, next) =>
                                            (cur.TractionForward?.GetYValue(Client.maxDccStep) ??
                                             int.MaxValue) <
                                            (next.TractionForward?.GetYValue(Client.maxDccStep) ??
                                             int.MaxValue)
                                              ? cur
                                              : next).Vehicle ?? VehicleEntity;
                       }
                       else
                       {
                         SlowestVehicleInTractionList = VehicleEntity;
                       }
                     });
    }

    private bool GetDrivingDirection(VehicleEntity vehicle, bool direction)
    {
      return vehicle.Id != VehicleEntity.Id ? vehicle.InvertTraction ? !direction : direction : direction;
    }

    /// <summary>
    /// Raises the <see cref="StateChanged"/> event. 
    /// </summary>
    private void OnStateChanged()
    {
      StateChanged?.Invoke(this, null!);
    }

    private async Task SetLocoDrive(int? speedstep = null, bool? drivingDirection = null, bool inUse = true)
    {
      await Task.Run(
                     () =>
                     {
                       if (speedstep is not null && speedstep != 0 && speedstep != Client.maxDccStep &&
                           DateTime.Now - LastSpeedChange < new TimeSpan(0, 0, 0, 0, 500))
                       {
                         return;
                       }
                       else
                       {
                         LastSpeedChange = DateTime.Now;
                       }

                       bool direction = drivingDirection ??= LiveData?.DrivingDirection ?? true;
                       int speed = speedstep ?? Speed;
                       List<LokInfoData> data = new();

                       MultiTractionItem slowestVehicle =
                         MultiTractionItems.FirstOrDefault(
                                                           e => e.Vehicle.Equals(
                                                                                 SlowestVehicleInTractionList));
                       double yValue = GetSlowestVehicleSpeed(speed, direction, slowestVehicle);

                       foreach (MultiTractionItem item in MultiTractionItems.Where(
                                                                                   e => !e.Vehicle.Equals(
                                                                                                          SlowestVehicleInTractionList)))
                       {
                         if (IsVehicleMeasured(item))
                         {
                           int dccSpeed;
                           if (!item.Vehicle.InvertTraction)
                           {
                             dccSpeed = direction
                                          ? item.TractionForward.GetXValue(yValue)
                                          : item.TractionBackward.GetXValue(yValue);
                           }
                           else
                           {
                             dccSpeed = direction
                                          ? item.TractionBackward.GetXValue(yValue)
                                          : item.TractionForward.GetXValue(yValue);
                           }

                           data.Add(
                                    GetLocoInfoData(
                                                    dccSpeed, GetDrivingDirection(item.Vehicle, direction),
                                                    inUse, item.Vehicle));
                         }
                         else
                         {
                           data.Add(
                                    GetLocoInfoData(
                                                    speed, GetDrivingDirection(item.Vehicle, direction),
                                                    inUse, item.Vehicle));
                         }
                       }

                       data.Add(
                                GetLocoInfoData(
                                                speed, GetDrivingDirection(slowestVehicle.Vehicle, direction),
                                                inUse, slowestVehicle.Vehicle));
                       LiveData.DrivingDirection = direction;
                       LiveData.Speed = speed;
                       OnStateChanged();
                       Z21Client.SetLocoDrive(data);
                     });
    }

    /// <summary>
    /// Sets the vehiclemodel property of this object.
    /// </summary>
    /// <param name="vehicleEntity"></param>
    /// <exception cref="ApplicationException"></exception>
    private async void SetVehicleModel(VehicleEntity vehicleEntity)
    {
      VehicleEntity = Db.Vehicles.Include(e => e.Functions).FirstOrDefault(e => e.Id == vehicleEntity.Id) ??
                     throw new ApplicationException($"Could not find vehicle '{vehicleEntity}'.");
      UpdateMultiTractionList();
      await DeterminSlowestVehicleInList();
      OnStateChanged();
    }

    private void UpdateMultiTractionList()
    {
      MultiTractionItems.Clear();
      MultiTractionItems.AddRange(
                                  VehicleEntity.TractionVehicleIds
                                              .Select(
                                                      e => new MultiTractionItem(
                                                                                 Db.Vehicles.FirstOrDefault(
                                                                                                            f =>
                                                                                                              f.Id == e)
                                                                                 !)).Where(e => e.Vehicle is not null));
      MultiTractionItems.Add(new(VehicleEntity));
    }

    private void Z21Client_OnGetLocoInfo(object? sender, GetLocoInfoEventArgs e)
    {
      if (e.Data.Adresse.Value == VehicleEntity.Address)
      {
        //if (VehicleEntity.InvertTraction)
        //    e.Data.DrivingDirection = !e.Data.DrivingDirection;
        LiveData = e.Data;
        OnStateChanged();
      }
    }
  }
}