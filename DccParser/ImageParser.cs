using ZXing.Common;

namespace DccParser
{
    public class ImageParser
    {
        public bool TryParse(Stream stream, long size, out string result)
        {
            result = null;
            if (size > 512000) //FromEncodedData has size limit
                return false;

            using var image = SkiaSharp.SKImage.FromEncodedData(stream);
            if (image == null) //Not actually an image
                return false;

            using var bitmap = SkiaSharp.SKBitmap.FromImage(image);
            var reader = new ZXing.SkiaSharp.BarcodeReader();
            reader.AutoRotate = true;
            reader.Options = new DecodingOptions()
            {
                TryHarder = true,
                TryInverted = true,
            };

            var decodeResult = reader.Decode(bitmap);
            if (string.IsNullOrWhiteSpace(decodeResult?.Text))
                return false;

            result = decodeResult.Text;
            return true;
        }
    }
}