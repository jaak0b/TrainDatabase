using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Persistence.Mapping;

namespace Persistence.UnitTest
{
  public class AutoMapperConfigurationTests
  {
    [Test]
    public void AllProfiles_ShouldBeValid()
    {
      MapperConfiguration config = new(cfg =>
                                       {
                                         cfg.AddProfile<BaseObjectProfile>();
                                         cfg.AddProfile<VehicleProfile>();
                                         cfg.AddProfile<VehicleFunctionProfile>();
                                         cfg.AddProfile<VehicleCalibrationDataProfile>();
                                       });
      Assert.DoesNotThrow(() => config.AssertConfigurationIsValid());
    }
  }
}