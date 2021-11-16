using Patagames.Ocr;
using System.Drawing;

namespace TesseractOcr
{
    public static class OcrDetector
    {
        public static string ReadText(Stream stream)
        {
            var target = (Bitmap)Image.FromStream(stream);
            using (var objOcr = OcrApi.Create())
            {
                objOcr.Init(Patagames.Ocr.Enums.Languages.ChineseSimplified);

                string plainText = objOcr.GetTextFromImage(target);

                return plainText;
            }
        }

    }
}