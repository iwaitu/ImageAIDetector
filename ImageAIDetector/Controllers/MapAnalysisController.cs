using ImageAIDetector.Utils;
using Microsoft.AspNetCore.Mvc;

namespace ImageAIDetector.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MapAnalysisController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<MapAnalysisController> _logger;

        public MapAnalysisController(IHttpClientFactory httpClientFactory, ILogger<MapAnalysisController> logger)
        {
            _clientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AnalysisRegion(TargetObject target)
        {
            if(target == null || target.mapServiceUrl == null)
            {
                return Ok("参数不能为空!");
            }
            var mapHelper = new MapServiceHelper(target, _clientFactory, _logger);
            try
            {
                var result = await mapHelper.CheclAvailable();
                if (result == false)
                {
                    return Ok("地图服务地址不可用!");
                }

                mapHelper.CalcularTaskJobs();
                    
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message + " 传入参数不可用");
            }
            
            return Ok();
        }
    }
}
