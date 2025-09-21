using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Persistence.Entities;
using Persistence.Model;

namespace Persistence.Mapping
{
  public class BaseObjectProfile : Profile
  {
    public BaseObjectProfile()
    {
      CreateMap<BaseObjectEntity, BaseObject>()
       .EqualityComparison((src, dest) => src.Id == dest.Id);

      CreateMap<BaseObject, BaseObjectEntity>()
       .EqualityComparison((src, dest) => src.Id == dest.Id)
       .ForMember(vehicleEntity => vehicleEntity.Id, expression => expression.Ignore());
    }
  }
}