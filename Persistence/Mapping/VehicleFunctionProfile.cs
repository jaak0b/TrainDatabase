using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Persistence.Entities;
using Persistence.Model;

namespace Persistence.Mapping
{
  public class VehicleFunctionProfile : Profile
  {
    public VehicleFunctionProfile()
    {
      CreateMap<VehicleFunctionEntity, VehicleFunction>()
       .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
       .EqualityComparison((src, dest) => src.Id == dest.Id);

      CreateMap<VehicleFunction, VehicleFunctionEntity>()
       .EqualityComparison((src, dest) => src.Id == dest.Id)
       .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle))
       .ForMember(vehicleEntity => vehicleEntity.Id, expression => expression.Ignore());
    }
  }
}