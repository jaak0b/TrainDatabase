using AutoMapper;
using Persistence.Ports;

namespace Persistence.Repositories
{
  public class VehicleRepository(Database.Database database, IMapper mapper) : IVehicleRepository
  {

  }
}