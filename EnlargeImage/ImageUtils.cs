using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;

using Tesseract;

namespace EnlargeImage
{
    public enum TesseractLanguage
    {
        Chinese,
        English
    }
    public class ImageUtils : IDisposable
    {
        private TesseractEngine _tesseractEngine;

        public ImageUtils(TesseractLanguage lang)
        {
            switch (lang)
            {
                case TesseractLanguage.Chinese:
                    _tesseractEngine = new TesseractEngine(@"./tessdata", "chi_sim", EngineMode.Default);
                    break;
                case TesseractLanguage.English:
                    _tesseractEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
                    break;
                default:
                    _tesseractEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
                    break;
            }
            
            _tesseractEngine.SetVariable("tessedit_char_whitelist", "ABCDEFHKLMNPRSTVXY1234567890");
        }

        public void Dispose()
        {
            if(_tesseractEngine != null)
            {
                _tesseractEngine.Dispose();
            }
        }

        /// <summary>
        /// 车牌影像识别车牌号
        /// </summary>
        /// <param name="filepath">裁切好的车牌图像</param>
        /// <returns></returns>
        public string RecognizeProcess(string filepath)
        {
            try
            {
                Image<Bgr, Byte> captureImage = new Image<Bgr, byte>(filepath);
                Image<Bgr, byte> resizedImage = captureImage.Resize((int)(captureImage.Width * 1.6), (int)(captureImage.Height * 1.6), Emgu.CV.CvEnum.Inter.LinearExact);
                FileInfo fi = new FileInfo(filepath);


                Image<Gray, byte> imgTarget = resizedImage.Convert<Gray, byte>().ThresholdBinaryInv(new Gray(128), new Gray(255));
                var city = FindCity(imgTarget);
                ImageConverter converter = new ImageConverter();
                var dataTarget = (byte[])converter.ConvertTo(imgTarget.AsBitmap(), typeof(byte[]));

                var text = ProcessOcr(dataTarget);
                return city + text.Trim();
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// 车牌影像识别车牌号
        /// </summary>
        /// <param name="imgPlat">裁切好的车牌图像</param>
        /// <returns></returns>
        public string RecognizeProcess(Bitmap imgPlat)
        {
            try
            {
                Image<Bgr, byte> captureImage = imgPlat.ToImage<Bgr, byte>();
                Image<Bgr, byte> resizedImage = captureImage.Resize((int)(captureImage.Width * 1.6), (int)(captureImage.Height * 1.6), Emgu.CV.CvEnum.Inter.LinearExact);

                Image<Gray, byte> imgTarget = resizedImage.Convert<Gray, byte>().ThresholdBinaryInv(new Gray(128), new Gray(255));
                imgTarget.Save("grayTarget.bmp");
                var city = FindCity(imgTarget);
                ImageConverter converter = new ImageConverter();
                var dataTarget = (byte[])converter.ConvertTo(imgTarget.AsBitmap(), typeof(byte[]));

                var text = ProcessOcr(dataTarget);
                return city.Trim() + text.Trim();
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public string FindCity(Image<Gray, byte> imgScene)
        {
            Image<Gray, byte> imgTemp = new Image<Gray, byte>(@"D:\dev\ivilson\ImageAIDetector\TestLibrary\template\桂.bmp");
            var vp = ProcessImage(imgTemp, imgScene);
            if(vp != null)
            {
                return "桂";
            }
            return string.Empty;
        }
        public string ProcessOcr(byte[] data)
        {
            if (_tesseractEngine != null)
            {
                using (var pix = Pix.LoadFromMemory(data))
                {
                    var paper = _tesseractEngine.Process(pix);
                    if (paper != null)
                    {
                        return paper.GetText();
                    }
                }

            }
            return "";
        }
        public string ProcessOcr(string filename)
        {
            if(_tesseractEngine !=null)
            {
                using (var pix = Pix.LoadFromFile(filename))
                {
                    var paper = _tesseractEngine.Process(pix);
                    if(paper != null)
                    {
                        return paper.GetText();
                    }
                }

            }
            return "";
        }

        private byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private static VectorOfPoint ProcessImage(Image<Gray, byte> template, Image<Gray, byte> sceneImage)
        {
            try
            {
                // initialization
                VectorOfPoint finalPoints = null;
                Mat homography = null;
                VectorOfKeyPoint templateKeyPoints = new VectorOfKeyPoint();
                VectorOfKeyPoint sceneKeyPoints = new VectorOfKeyPoint();
                Mat tempalteDescriptor = new Mat();
                Mat sceneDescriptor = new Mat();

                Mat mask;
                int k = 2;
                double uniquenessthreshold = 0.80;
                VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();

                // feature detectino and description
                Brisk featureDetector = new Brisk();
                featureDetector.DetectAndCompute(template, null, templateKeyPoints, tempalteDescriptor, false);
                featureDetector.DetectAndCompute(sceneImage, null, sceneKeyPoints, sceneDescriptor, false);

                // Matching
                BFMatcher matcher = new BFMatcher(DistanceType.Hamming);
                matcher.Add(tempalteDescriptor);
                matcher.KnnMatch(sceneDescriptor, matches, k);

                mask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));

                Features2DToolbox.VoteForUniqueness(matches, uniquenessthreshold, mask);

                int count = Features2DToolbox.VoteForSizeAndOrientation(templateKeyPoints, sceneKeyPoints, matches, mask, 1.5, 20);

                if (count >= 4)
                {
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(templateKeyPoints,
                        sceneKeyPoints, matches, mask, 5);
                }

                if (homography != null)
                {
                    Rectangle rect = new Rectangle(Point.Empty, template.Size);
                    PointF[] pts = new PointF[]
                    {
                        new PointF(rect.Left,rect.Bottom),
                        new PointF(rect.Right,rect.Bottom),
                        new PointF(rect.Right,rect.Top),
                        new PointF(rect.Left,rect.Top)
                    };

                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    finalPoints = new VectorOfPoint(points);
                }

                return finalPoints;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        private static VectorOfPoint ProcessImageFLANN(Image<Gray, byte> template, Image<Gray, byte> sceneImage)
        {
            try
            {
                // initializations done
                VectorOfPoint finalPoints = null;
                Mat homography = null;
                VectorOfKeyPoint templateKeyPoints = new VectorOfKeyPoint();
                VectorOfKeyPoint sceneKeyPoints = new VectorOfKeyPoint();
                Mat tempalteDescriptor = new Mat();
                Mat sceneDescriptor = new Mat();

                Mat mask;
                int k = 2;
                double uniquenessthreshold = 0.80;
                VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();

                // feature detectino and description
                KAZE featureDetector = new KAZE();
                featureDetector.DetectAndCompute(template, null, templateKeyPoints, tempalteDescriptor, false);
                featureDetector.DetectAndCompute(sceneImage, null, sceneKeyPoints, sceneDescriptor, false);


                // Matching

                //KdTreeIndexParams ip = new KdTreeIndexParams();
                //var ip = new AutotunedIndexParams();
                var ip = new LinearIndexParams();
                SearchParams sp = new SearchParams();
                FlannBasedMatcher matcher = new FlannBasedMatcher(ip, sp);


                matcher.Add(tempalteDescriptor);
                matcher.KnnMatch(sceneDescriptor, matches, k);

                mask = new Mat(matches.Size, 1, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));

                Features2DToolbox.VoteForUniqueness(matches, uniquenessthreshold, mask);

                int count = Features2DToolbox.VoteForSizeAndOrientation(templateKeyPoints, sceneKeyPoints, matches, mask, 1.5, 20);

                if (count >= 4)
                {
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(templateKeyPoints,
                        sceneKeyPoints, matches, mask, 5);
                }

                if (homography != null)
                {
                    Rectangle rect = new Rectangle(Point.Empty, template.Size);
                    PointF[] pts = new PointF[]
                    {
                        new PointF(rect.Left,rect.Bottom),
                        new PointF(rect.Right,rect.Bottom),
                        new PointF(rect.Right,rect.Top),
                        new PointF(rect.Left,rect.Top)
                    };

                    pts = CvInvoke.PerspectiveTransform(pts, homography);
                    Point[] points = Array.ConvertAll<PointF, Point>(pts, Point.Round);
                    finalPoints = new VectorOfPoint(points);
                }

                return finalPoints;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}