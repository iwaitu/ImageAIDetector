using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomVision
{
    public class LicensePlatDetectEngine : DetectEngine , ILicensePlatDetectEngine
    {
        public LicensePlatDetectEngine(ILogger<LicensePlatDetectEngine> logger)
            : base(logger, DetectEngineType.LicensePlat)
        {

        }

    }
}
