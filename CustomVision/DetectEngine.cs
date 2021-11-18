using CustomVision.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CustomVision
{
    public class DetectEngine : IDetectEngine
    {
        private readonly ILogger<DetectEngine> _logger;

        private const int rowCount = 3, columnCount = 3;

        private const int featuresPerBox = 5;

        private static readonly (float x, float y)[] boxAnchors = { (0.573f, 0.677f), (1.87f, 2.06f), (3.34f, 5.47f), (7.88f, 3.53f), (9.77f, 9.17f) };

        private DetectEngineType _engineType;
        
        private PredictionEngine<CustomVisionInput, CustomVisionPredictions> _predictionEngine;
        private string[] _labels;
        private OnnxModelScorer _modelScorer;
        private CustomVisionOutputParser _outputParser;

        public DetectEngine(ILogger<DetectEngine> logger,DetectEngineType engineType = DetectEngineType.Quarry)
        {
            _logger = logger;
            _engineType = engineType;
            InitEngine();
        }

        private void InitEngine()
        {
            var mlContext = new MLContext();
            switch (_engineType)
            {
                case DetectEngineType.Quarry:
                    _modelScorer = new OnnxModelScorer("./Models/model.onnx", mlContext);
                    _outputParser = _modelScorer.GetParser("./Models/labels.txt");
                    break;
                case DetectEngineType.LicensePlat:
                    _modelScorer = new OnnxModelScorer("./Models/platmodel.onnx", mlContext);
                    _outputParser = _modelScorer.GetParser("./Models/platlabels.txt");
                    break;
                default:
                    break;
            }
            
        }

        public IList<IdentityRect> ProcessDetectorResult(Stream stream)
        {
            IList<IdentityRect> ret = new List<IdentityRect>();
            Bitmap testImage;
            testImage = (Bitmap)Image.FromStream(stream);

            IEnumerable<float[]> probabilities = _modelScorer.Score(testImage);

            if (probabilities == null || probabilities.Count() == 0)
            {
                return ret;
            }

            var boundingBoxes = probabilities.Select(probability => _outputParser.ParseOutputs(probability))
                .Select(boxes => _outputParser.FilterBoundingBoxes(boxes, 5, .5F)).ToList();

            
            var detectedObjects = boundingBoxes.FirstOrDefault();

            var originalImageHeight = testImage.Height;
            var originalImageWidth = testImage.Width;
            
            foreach (var box in detectedObjects)
            {
                // Get Bounding Box Dimensions
                var x = (int)Math.Max(box.Dimensions.X, 0);
                var y = (int)Math.Max(box.Dimensions.Y, 0);
                var width = (int)Math.Min(originalImageWidth - x, box.Dimensions.Width);
                var height = (int)Math.Min(originalImageHeight - y, box.Dimensions.Height);

                // Resize To Image
                x = (int)originalImageWidth * x / OnnxModelScorer.ImageNetSettings.imageWidth;
                y = (int)originalImageHeight * y / OnnxModelScorer.ImageNetSettings.imageHeight;
                width = (int)originalImageWidth * width / OnnxModelScorer.ImageNetSettings.imageWidth;
                height = (int)originalImageHeight * height / OnnxModelScorer.ImageNetSettings.imageHeight;

                // Bounding Box Text
                string text = $"{box.Label} ({(box.Confidence * 100).ToString("0")}%)";

                var ir = new IdentityRect { rectangle = new Rectangle(x, y, width, height), description = text };
                ret.Add(ir);
            }
            return ret;
            
        }

        public Bitmap ProcessDetector(Stream stream)
        {
            Bitmap testImage;

            testImage = (Bitmap)Image.FromStream(stream);

            IEnumerable<float[]> probabilities = _modelScorer.Score(testImage);

            var boundingBoxes = probabilities.Select(probability => _outputParser.ParseOutputs(probability))
                .Select(boxes => _outputParser.FilterBoundingBoxes(boxes, 5, .5F));

            var detectedObjects = boundingBoxes.FirstOrDefault();

            var originalImageHeight = testImage.Height;
            var originalImageWidth = testImage.Width;

            foreach (var box in detectedObjects)
            {
                // Get Bounding Box Dimensions
                var x = (int)Math.Max(box.Dimensions.X, 0);
                var y = (int)Math.Max(box.Dimensions.Y, 0);
                var width = (int)Math.Min(originalImageWidth - x, box.Dimensions.Width);
                var height = (int)Math.Min(originalImageHeight - y, box.Dimensions.Height);

                // Resize To Image
                x = (int)originalImageWidth * x / OnnxModelScorer.ImageNetSettings.imageWidth;
                y = (int)originalImageHeight * y / OnnxModelScorer.ImageNetSettings.imageHeight;
                width = (int)originalImageWidth * width / OnnxModelScorer.ImageNetSettings.imageWidth;
                height = (int)originalImageHeight * height / OnnxModelScorer.ImageNetSettings.imageHeight;

                // Bounding Box Text
                string text = $"{box.Label} ({(box.Confidence * 100).ToString("0")}%)";


                using (Graphics thumbnailGraphic = Graphics.FromImage(testImage))
                {
                    thumbnailGraphic.CompositingQuality = CompositingQuality.HighQuality;
                    thumbnailGraphic.SmoothingMode = SmoothingMode.HighQuality;
                    thumbnailGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    // Define Text Options
                    Font drawFont = new Font("Arial", 12, FontStyle.Bold);
                    SizeF size = thumbnailGraphic.MeasureString(text, drawFont);
                    SolidBrush fontBrush = new SolidBrush(Color.Black);
                    Point atPoint = new Point((int)x, (int)y - (int)size.Height - 1);

                    // Define BoundingBox options
                    Pen pen = new Pen(box.BoxColor, 3.2f);
                    SolidBrush colorBrush = new SolidBrush(box.BoxColor);

                    // Draw text on image 
                    thumbnailGraphic.FillRectangle(colorBrush, (int)x, (int)(y - size.Height - 1), (int)size.Width, (int)size.Height);
                    thumbnailGraphic.DrawString(text, drawFont, fontBrush, atPoint);

                    // Draw bounding box on image
                    thumbnailGraphic.DrawRectangle(pen, x, y, width, height);
                }
            }

            return testImage;
        }

    }
}
