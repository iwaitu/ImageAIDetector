using CustomVision;
using Microsoft.AspNetCore.Mvc;

namespace ImageAIDetector.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DetectorController : Controller
    {
        private readonly ILogger<DetectorController> _logger;

        private readonly IDetectEngine _detectEngine;

        public DetectorController(ILogger<DetectorController> logger, IDetectEngine detectEngine)
        {
            _logger = logger;
            _detectEngine = detectEngine;
        }

        [HttpPost]
        [Route("analysisdemo")]
        public async Task<IActionResult> AnalysisDemo(IFormFile file)
        {


            if (file.Length > 0)
            {
                var filePath = Path.GetTempFileName();

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                    var data = _detectEngine.ProcessDetector(file.Name, stream);
                    if (data != null)
                    {
                        return File(data, "image/jpeg");
                    }

                }
            }

            return Ok();
        }

        [HttpPost]
        [Route("analysis")]
        public async Task<IActionResult> Analysis(IFormFile file)
        {
            if (file.Length > 0)
            {
                var filePath = Path.GetTempFileName();

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                    var data = _detectEngine.ProcessDetectorResult( stream);
                    if (data != null)
                    {
                        return Ok(data);
                    }

                }
            }
            return Ok();
        }

    }
}
