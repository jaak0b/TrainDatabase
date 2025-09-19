
using System;
using System.Collections.Generic;
using Persistence.Extensions;
using Persistence.Model;

namespace Persistence.Ports
{
  public interface IVehicleRepository
  {
    
    IObservable<Vehicle> VehicleChangedStream { get; }
    
    /// <summary>
    /// Gets a <see cref="Vehicle"/> from the data source with the specified <paramref name="vehicleId"/>.
    /// </summary>
    /// <exception cref="IdNotFoundException">Thrown when no vehicle is found for the <paramref name="vehicleId"/>.</exception>
    Vehicle GetVehicleByIdRequired(int vehicleId);
    
    /// <summary>
    /// Gets a <see cref="Vehicle"/> from the data source with the specified <paramref name="vehicleId"/>.
    /// </summary>
    Vehicle? GetVehicleById(int vehicleId);

    /// <summary>
    /// Allows to search <see cref="Vehicle"/> by almost every parameter.
    /// </summary>
    IReadOnlyCollection<Vehicle> FullTextSearchVehicles(string? searchString);
    
    
  }
}