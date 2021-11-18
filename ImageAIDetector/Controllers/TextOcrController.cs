using CustomVision;
using EnlargeImage;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace ImageAIDetector.Controllers
{
    /// <summary>
    /// 车牌识别
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TextOcrController : ControllerBase
    {
        private readonly ILicensePlatDetectEngine _licensePlatDetectEngine;

        public TextOcrController(ILicensePlatDetectEngine licensePlatDetectEngine)
        {
            _licensePlatDetectEngine = licensePlatDetectEngine;
        }

        [HttpPost("readtext")]
        public async Task<string> ReadText(IFormFile file)
        {
            if (file.Length > 0)
            {
                var filePath = "target.bmp";
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                    using(Bitmap bmpImage = new Bitmap(stream))
                    {
                        var rect = _licensePlatDetectEngine.ProcessDetectorResult(stream);
                        if (rect != null)
                        {
                            using (var imgPlat = bmpImage.Clone(rect.rectangle, bmpImage.PixelFormat))
                            {
                                using (var imgUtils = new ImageUtils(TesseractLanguage.Chinese))
                                {
                                    var result = imgUtils.RecognizeProcess(imgPlat);
                                    return await Task.FromResult(result);
                                }
                            }

                        }
                    }
                }
                
            }
            return await Task.FromResult("");
        }
    }
}
