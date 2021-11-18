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
            var file = @"C:\Users\iwaitu\Pictures\666.jpg";
            using(var stream = new FileStream(file,FileMode.Open))
            using (Bitmap bmpImage = new Bitmap(stream))
            {
                var rects = _licensePlatDetectEngine.ProcessDetectorResult(stream);
                if (rects != null)
                {
                    foreach(var rect in rects)
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
}