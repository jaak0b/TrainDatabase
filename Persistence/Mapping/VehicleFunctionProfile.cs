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
       .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle));

      CreateMap<VehicleFunction, VehicleFunctionEntity>()
       .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle));
    }
  }
}