using Microsoft.ML.Data;

namespace CustomVision
{
    public class CustomVisionPredictions
    {
        [ColumnName("model_outputs0")]
        public float[] CustomVisionType { get; set; }
    }
}
