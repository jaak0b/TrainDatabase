using Mapster;
using TrainDatabase.Core.Domain;
using TrainDatabase.Infrastructure.Entities;

namespace TrainDatabase.Infrastructure.Mapping;

/// <summary>
/// Mapster configuration for entity ↔ domain mapping (replaces the former AutoMapper
/// profiles). Property names match by convention, so only the cyclic
/// <see cref="Vehicle"/> ↔ <see cref="VehicleFunction"/> relationship needs special
/// handling via reference preservation.
/// </summary>
public static class MappingConfig
{
    public static TypeAdapterConfig Create()
    {
        TypeAdapterConfig config = new();

        // Vehicle.Functions[].Vehicle (and Calibrations[].Vehicle) form cycles; preserving
        // references makes the back-link point at the already-mapped instance instead of
        // recursing forever.
        config.Default.PreserveReference(true);

        config.NewConfig<VehicleEntity, Vehicle>()
            .Map(dest => dest.TractionVehicleIds, src => src.TractionMembers.Select(member => member.MemberVehicleId).ToList());
        config.NewConfig<Vehicle, VehicleEntity>()
            .Ignore(dest => dest.TractionMembers);
        config.NewConfig<VehicleFunctionEntity, VehicleFunction>().TwoWays();
        config.NewConfig<VehicleCalibrationDataEntity, VehicleCalibrationData>().TwoWays();

        config.Compile();
        return config;
    }
}
