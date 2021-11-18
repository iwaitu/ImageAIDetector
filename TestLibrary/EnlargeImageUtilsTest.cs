using EnlargeImage;
using System;
using Xunit;

namespace TestLibrary
{
    public class EnlargeImageUtilsTest
    {
        [Fact]
        public void EnlargeImage()
        {
            var file = @"C:\Users\iwaitu\Pictures\333_large.jpg";
            using(var imgUtils = new ImageUtils( TesseractLanguage.Chinese))
            {
                var resizeFile = imgUtils.RecognizeProcess(file);

                Assert.False(string.IsNullOrEmpty(resizeFile));

            }
            
        }
    }
}