using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TesseractOcr;

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
                var filePath = Path.GetTempFileName();

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                    return OcrDetector.ReadText(stream);

                }
                
            }
            return await Task.FromResult("");
        }
    }
}
