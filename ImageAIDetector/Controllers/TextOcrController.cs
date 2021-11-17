﻿using EnlargeImage;
using Microsoft.AspNetCore.Mvc;

namespace ImageAIDetector.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextOcrController : ControllerBase
    {
        [HttpPost("readtext")]
        public async Task<string> ReadText(IFormFile file)
        {
            if (file.Length > 0)
            {
                var filePath = "target.bmp";

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                    
                }
                using(var imgUtils = new ImageUtils(TesseractLanguage.Chinese))
                {
                    var resizedImage =  imgUtils.ProcessBitmap(filePath);
                    return imgUtils.ProcessBitmap(resizedImage);
                }
            }
            return await Task.FromResult("");
        }
    }
}