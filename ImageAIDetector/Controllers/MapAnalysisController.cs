using CustomVision;
using ImageAIDetector.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ImageAIDetector.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MapAnalysisController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<MapAnalysisController> _logger;
        private readonly IDetectEngine _detectEngine;

        public MapAnalysisController(IHttpClientFactory httpClientFactory, ILogger<MapAnalysisController> logger, IDetectEngine detectEngine)
        {
            _clientFactory = httpClientFactory;
            _logger = logger;
            _detectEngine = detectEngine;
        }

        [HttpPost]
        public async Task<IActionResult> AnalysisRegion(TargetObject target)
        {
            if(target == null || target.mapServiceUrl == null)
            {
                return Ok("参数不能为空!");
            }
            var mapHelper = new MapServiceHelper(target, _clientFactory, _logger);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                if (await mapHelper.CheclAvailable() == false)
                {
                    return Ok("地图服务地址不可用!");
                }

                mapHelper.CalcularTaskJobs();
                var ret = await mapHelper.ProcessAllJobs(_detectEngine);
                var gjw = new NetTopologySuite.IO.GeoJsonWriter();
                var result = gjw.Write(ret);
                sw.Stop();
                return Ok(new { result = result,timespaned =  sw.ElapsedMilliseconds });
            }
            catch (Exception ex)
            {
                sw.Stop();
                return BadRequest(ex.Message + " 传入参数不可用");
            }
        }
    }
}
