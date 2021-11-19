using CustomVision;
using EnlargeImage;
using System;
using System.Collections.Generic;
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
        public void TestArray()
        {
            var list = new List<string>();
            list.Add("test1");
            list.Add("test2");
            var ret = string.Join(",", list);
        }

        [Fact]
        public void RecognizeCaptureImage()
        {
            var file = @"./testSamples/666.jpg";
            using(var stream = new FileStream(file,FileMode.Open))
            using (Bitmap bmpImage = new Bitmap(stream))
            {
                var rects = _licensePlatDetectEngine.ProcessDetectorResult(stream);
                if (rects != null)
                {
                    foreach(var rect in rects)
                    {
                        var newRect  = new Rectangle(rect.rectangle.X,rect.rectangle.Y, rect.rectangle.Width + 4, rect.rectangle.Height);
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
}