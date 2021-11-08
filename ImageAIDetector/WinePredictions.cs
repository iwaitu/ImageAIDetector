using Microsoft.ML.Data;

namespace ImageAIDetector
{
    public class WinePredictions
    {
        [ColumnName("model_outputs0")]
        public float[]? PredictedLabels { get; set; }
    }
}
