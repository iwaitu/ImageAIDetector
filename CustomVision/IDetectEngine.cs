using System.Drawing;

namespace CustomVision
{
    public interface IDetectEngine
    {
        IdentityRect ProcessDetectorResult( Stream stream);
        byte[] ProcessDetector(string name, Stream stream);
    }
}