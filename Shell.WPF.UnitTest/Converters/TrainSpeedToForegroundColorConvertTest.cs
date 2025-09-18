using System.Globalization;
using System.Windows.Media;
using Shell.WPF.Converters;

namespace Shell.WPF.UnitTest.Converters
{
  public class TrainSpeedToForegroundColorConvertTest
  {
    private TrainSpeedToForegroundColorConvert converter;

    [SetUp]
    public void Setup()
    {
      converter = new();
    }

    private static IEnumerable<TestCaseData> ConvertReturnsExpectedBrushForGivenSpeedTestCaseSource
    {
      get
      {
        yield return new(null, Brushes.Gray);
        yield return new(0, Brushes.Gray);
        yield return new(1, Brushes.Black);
        yield return new(100, Brushes.Black);
      }
    }

    [Test]
    [TestCaseSource(nameof(ConvertReturnsExpectedBrushForGivenSpeedTestCaseSource))]
    public void Convert_ReturnsExpectedBrush_ForGivenSpeed(int? speed, Brush brush)
    {
      object? result = converter.Convert(speed, typeof(Brush), null, CultureInfo.InvariantCulture);
      Assert.That(result, Is.EqualTo(brush));
    }

    [Test]
    public void Convert_TargetTypeIsWrongType_ThrowsArgumentException()
    {
      Assert.Throws<ArgumentException>(() => converter.Convert(0, typeof(int), null, CultureInfo.InvariantCulture));
    }

    [Test]
    public void ConvertBack_ThrowsInvalidOperationException()
    {
      Assert.Throws<InvalidOperationException>(() => converter.ConvertBack(Brushes.Black, typeof(int), null, CultureInfo.InvariantCulture));
    }
  }
}