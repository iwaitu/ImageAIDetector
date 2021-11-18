using CustomVision;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace TestLibrary
{
    public class DetectEngineTest
    {
        private ILicensePlatDetectEngine _licensePlatDetectEngine;
        private XunitLogger<LicensePlatDetectEngine> _logger;

        public DetectEngineTest(ITestOutputHelper output)
        {
            _logger = new XunitLogger<LicensePlatDetectEngine>(output);
            _licensePlatDetectEngine = new LicensePlatDetectEngine(_logger);
        }

        [Fact]
        public void DetectCar()
        {
            var file = @"C:\Users\iwaitu\Pictures\666.jpg";
            using (var stream = new FileStream(file, FileMode.Open))
            {
                var rect = _licensePlatDetectEngine.ProcessDetectorResult(stream);
                Assert.NotNull(rect);
            }
            
        }
    }
}
