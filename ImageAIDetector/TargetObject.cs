

namespace ImageAIDetector
{
    public class TargetObject
    {
        public GisPoint LeftTop { get; set; }
        public GisPoint LeftBottom { get; set; }
        public GisPoint RightTop { get; set; }
        public GisPoint RightBottom { get; set; }

        public string? mapServiceUrl { get; set; }
    }

    public class GisPoint
    {
        public float x { get; set; }
        public float y { get; set; }
    }
}
