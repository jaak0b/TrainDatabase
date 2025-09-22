using AutoMapper;
using Persistence.Entities;
using Persistence.Model;

namespace Persistence.Mapping
{
  public class VehicleCalibrationDataProfile : Profile
  {
    public VehicleCalibrationDataProfile()
    {
      CreateMap<VehicleCalibrationDataEntity, VehicleCalibrationData>()
       .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle));

      CreateMap<VehicleCalibrationData, VehicleCalibrationDataEntity>()
       .ForMember(dest => dest.Vehicle, opt => opt.MapFrom(src => src.Vehicle));
    }
  }
}