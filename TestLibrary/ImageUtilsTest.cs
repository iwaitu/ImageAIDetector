using CustomVision;
using EnlargeImage;
using System;
using System.Drawing;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace TestLibrary
{
    public class ImageUtilsTest
    {
        private ILicensePlatDetectEngine _licensePlatDetectEngine;
        private XunitLogger<LicensePlatDetectEngine> _logger;

        public ImageUtilsTest(ITestOutputHelper output)
        {
            _logger = new XunitLogger<LicensePlatDetectEngine>(output);
            _licensePlatDetectEngine = new LicensePlatDetectEngine(_logger);
        }

        [Fact]
        public void RecognizePlatImage()
        {
            var file = @"C:\Users\iwaitu\Pictures\333_large.jpg";
            using(var imgUtils = new ImageUtils( TesseractLanguage.Chinese))
            {
                var resizeFile = imgUtils.RecognizeProcess(file);

                Assert.False(string.IsNullOrEmpty(resizeFile));

            } 
        }

        [Fact]
        public void RecognizeCaptureImage()
        {
            var file = @"C:\Users\iwaitu\Pictures\666.jpg";
            using(var stream = new FileStream(file,FileMode.Open))
            using (Bitmap bmpImage = new Bitmap(stream))
            {
                var rect = _licensePlatDetectEngine.ProcessDetectorResult(stream);
                if (rect != null)
                {
                    using (var imgPlat = bmpImage.Clone(rect.rectangle, bmpImage.PixelFormat))
                    {
                        using (var imgUtils = new ImageUtils(TesseractLanguage.Chinese))
                        {
                            var result = imgUtils.RecognizeProcess(imgPlat);
                            Assert.False(string.IsNullOrEmpty(result));
                        }
                    }

                }
            }
        }
    }
}