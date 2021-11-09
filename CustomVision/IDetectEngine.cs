using System.Drawing;

namespace CustomVision
{
    public interface IDetectEngine
    {
        Rectangle? ProcessDetectorResult(string name, FileStream stream);
        byte[] ProcessDetector(string name, FileStream stream);
    }
}