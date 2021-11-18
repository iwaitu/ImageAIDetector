using System.Drawing;

namespace CustomVision
{
    public interface IDetectEngine
    {
        IdentityRect ProcessDetectorResult( Stream stream);
        byte[] ProcessDetector(string name, Stream stream);
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