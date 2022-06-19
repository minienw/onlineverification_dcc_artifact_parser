using PDFiumCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.SkiaSharp;

namespace DccParser;

public class PdfParser
{
    private readonly FpdfDocumentT _Doc;

    /// <summary>
    /// Return null on fail or empty result
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public unsafe PdfParser(byte[] buffer)
    {
        fpdfview.FPDF_InitLibrary();
        fixed (byte* ptr = buffer)
            _Doc = fpdfview.FPDF_LoadMemDocument((IntPtr)ptr, (int)buffer.Length, "");
    }

    private static MemoryStream GetMemoryStream(Stream buffer)
    {
        //Avoid copy YET AGAIN
        if (buffer is MemoryStream ms)
            return ms;

        ms = new MemoryStream();
        buffer.Position = 0;
        buffer.CopyTo(ms);
        return ms;
    }


    private static readonly PdfParserArgs[] KnownConfigurations = 
    {
        new (0, new Rectangle(30, 510, 240, 235), 3),
        new (1, new Rectangle(30, 510, 240, 235), 3),
    };

    public bool TryParse(out string result)
    {
        foreach(var i in KnownConfigurations)
        {
            if (TryParse(i, out result))
                return true;
        }
        result = null;
        return false;
    }

    public bool TryParse(PdfParserArgs args, out string result)
    {
        var p = fpdfview.FPDF_LoadPage(_Doc, args.PageIndex);
        var r = args.GetScaledRectangle();
        var image = Render(p, r, args.Scale);
        using var ms = new MemoryStream();
        image.Save(ms, new BmpEncoder()); //Windows image
        var reader = new BarcodeReader();
        var decodeResult = reader.Decode(ms.ToArray(), r.Width, r.Height, RGBLuminanceSource.BitmapFormat.RGB24);

        if (!string.IsNullOrWhiteSpace(decodeResult?.Text))
        {
            result = decodeResult.Text;
            return true;
        }

        result = null;
        return false;

    }

    private unsafe Image<Bgra32> Render(FpdfPageT page, Rectangle rectangle, float scale)
    {
        var bitmap = fpdfview.FPDFBitmapCreateEx(
            rectangle.Size.Width,
            rectangle.Size.Height,
            (int)FPDFBitmapFormat.BGRA,
            IntPtr.Zero,
            0);

        fpdfview.FPDFBitmapFillRect(
            bitmap,
            0,
            0,
            rectangle.Size.Width,
            rectangle.Size.Height,
            Color.White.ToPixel<Argb32>().Argb);

        using var clipping = new FS_RECTF_
        {
            Left = 0,
            Right = rectangle.Size.Width,
            Bottom = 0,
            Top = rectangle.Size.Height
        };

        using var matrix = new FS_MATRIX_
        {
            A = scale,
            B = 0,
            C = 0,
            D = scale,
            E = -rectangle.X,
            F = -rectangle.Y
        };

        fpdfview.FPDF_RenderPageBitmapWithMatrix(bitmap, page, matrix, clipping, (int)RenderFlags.DisablePathAntialiasing);
        var buffer = fpdfview.FPDFBitmapGetBuffer(bitmap);
        return Image.WrapMemory<Bgra32>(buffer.ToPointer(), rectangle.Size.Width, rectangle.Size.Height);
    }
}