using Microsoft.ML.Transforms.Image;
using System.Drawing;

namespace CustomVision
{
    public struct ImageSettings
    {
        public const int imageHeight = 416;
        public const int imageWidth = 416;
    }

    public class CustomVisionInput
    {
        [ImageType(ImageSettings.imageHeight, ImageSettings.imageWidth)]
        public Bitmap? Image { get; set; }
    }
}
