using Emgu.CV;
using Emgu.CV.Structure;
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
            
            //_tesseractEngine.SetVariable("tessedit_char_whitelist", "ABCDEFHKLMNPRSTVXY1234567890");
        }

        public void Dispose()
        {
            if(_tesseractEngine != null)
            {
                _tesseractEngine.Dispose();
            }
        }

        public string EnlargeProcess(string filepath)
        {
            try
            {
                Image<Bgr, Byte> captureImage = new Image<Bgr, byte>(filepath);
                Image<Bgr, byte> resizedImage = captureImage.Resize((int)(captureImage.Width * 1.6), (int)(captureImage.Height * 1.6), Emgu.CV.CvEnum.Inter.LinearExact);
                FileInfo fi = new FileInfo(filepath);


                Image<Gray, byte> imgTarget = resizedImage.Convert<Gray, byte>()
                    .SmoothGaussian(3).ThresholdBinaryInv(new Gray(128), new Gray(255));

                var name = fi.Name.Substring(0,fi.Name.Length - 4) + "_large.jpg";
                var filename = Path.Combine(fi.Directory.FullName, name);
                imgTarget.Save(filename);

                return filename;
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
            
        }

        public string ProcessBitmap(string filename)
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
    }
}