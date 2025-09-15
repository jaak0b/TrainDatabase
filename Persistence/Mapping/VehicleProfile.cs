using System.Linq;
using AutoMapper;
using Persistence.Entities;
using Persistence.Model;

namespace Persistence.Mapping
{
  public class VehicleProfile : Profile
  {
    public VehicleProfile()
    {
      CreateMap<VehicleFunctionEntity, VehicleFunction>().ReverseMap();

      CreateMap<VehicleEntity, Vehicle>()
       .ForMember(vehicle => vehicle.Functions, expression => expression.MapFrom(src => src.Functions))
       .ForMember(vehicle => vehicle.TractionForward, expression => expression.MapFrom(src => src.TractionForward.ToArray()))
       .ForMember(vehicle => vehicle.TractionBackward, expression => expression.MapFrom(src => src.TractionBackward.ToArray()))
       .ForMember(vehicle => vehicle.TractionVehicleIds, expression => expression.MapFrom(src => src.TractionVehicleIds.ToList()));

      CreateMap<Vehicle, VehicleEntity>()
       .ForMember(vehicleEntity => vehicleEntity.Functions, expression => expression.MapFrom(src => src.Functions))
       .ForMember(vehicleEntity => vehicleEntity.TractionForward, expression => expression.MapFrom(src => src.TractionForward.ToArray()))
       .ForMember(vehicleEntity => vehicleEntity.TractionBackward, expression => expression.MapFrom(src => src.TractionBackward.ToArray()))
       .ForMember(vehicleEntity => vehicleEntity.TractionVehicleIds, expression => expression.MapFrom(src => src.TractionVehicleIds.ToList()));
    }
  }
}