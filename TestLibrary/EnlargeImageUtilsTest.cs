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
            using(var imgUtils = new ImageUtils( TesseractLanguage.English))
            {
                var resizeFile = imgUtils.EnlargeProcess(file);
                Assert.False(string.IsNullOrEmpty(resizeFile));
                var text = imgUtils.ProcessBitmap(resizeFile);
                Assert.False(string.IsNullOrEmpty(text));
            }
            
        }
    }
}