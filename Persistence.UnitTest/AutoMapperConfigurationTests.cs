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
      MapperConfiguration config = new(cfg => { cfg.AddProfile<VehicleProfile>(); }, new NullLoggerFactory());
      Assert.DoesNotThrow(() => config.AssertConfigurationIsValid());
    }
  }
}