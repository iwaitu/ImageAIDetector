using System.Drawing;

namespace CustomVision
{
    public interface IDetectEngine
    {
        IList<IdentityRect> ProcessDetectorResult(Stream stream);
        Bitmap ProcessDetector( Stream stream);
    }

    public interface IQuarryDetectEngine : IDetectEngine
    {
        
    }

    public interface ILicensePlatDetectEngine : IDetectEngine
    {

    }

    public enum DetectEngineType
    {
        Quarry,
        LicensePlat
    }
}