using Microsoft.ML.Transforms.Image;
using System.Drawing;
using static CustomVision.OnnxModelScorer;

namespace CustomVision
{
    public class CustomVisionInput
    {
        [ImageType(ImageNetSettings.imageHeight, ImageNetSettings.imageWidth)]
        public Bitmap? Image { get; set; }
    }
}
