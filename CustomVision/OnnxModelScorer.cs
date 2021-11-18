using CustomVision.Parser;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Drawing;

namespace CustomVision
{
    class OnnxModelScorer
    {
        private readonly MLContext mlContext;

        private IList<CustomVisionBoundingBox> _boundingBoxes = new List<CustomVisionBoundingBox>();

        private ITransformer _transformer;

        public OnnxModelScorer(string modelLocation, MLContext mlContext)
        {
            this.mlContext = mlContext;
            LoadModel(modelLocation);
        }

        public struct ImageNetSettings
        {
            public const int imageHeight = 416;
            public const int imageWidth = 416;
        }


        private void LoadModel(string modelLocation)
        {

            var lstData = new List<CustomVisionInput>();
            var data = mlContext.Data.LoadFromEnumerable(lstData);

            // Define scoring pipeline
            var pipeline = mlContext.Transforms.ResizeImages(outputColumnName: "data", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: nameof(CustomVisionInput.Image))
                            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "data"))
                            .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnName: "model_outputs0", inputColumnName: "data"));

            // Fit scoring pipeline
            _transformer = pipeline.Fit(data);
            
        }

        private IEnumerable<float[]> PredictDataUsingModel(IDataView testData)
        {
            
            IDataView scoredData = _transformer.Transform(testData);

            IEnumerable<float[]> probabilities = scoredData.GetColumn<float[]>("model_outputs0");

            return probabilities;
        }

        public IEnumerable<float[]> Score(Bitmap target)
        {
            var targetData = new List<CustomVisionInput>(); 
            targetData.Add(new CustomVisionInput { Image = target });
            IDataView data = mlContext.Data.LoadFromEnumerable(targetData);
            return PredictDataUsingModel(data);
        }

        public CustomVisionOutputParser GetParser(string filepath)
        {
            var parser = new CustomVisionOutputParser();
            parser.LoadLabels(filepath);
            return parser;
        }
    }
}
