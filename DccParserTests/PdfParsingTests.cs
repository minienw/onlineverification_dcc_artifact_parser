using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using DccParser;
using NCrunch.Framework;
using PDFiumCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using ZXing;
using ZXing.SkiaSharp;
using Color = SixLabors.ImageSharp.Color;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using RectangleF = System.Drawing.RectangleF;

namespace CheckInQrWebTests;

[ExclusivelyUses("files")]
public class PdfParsingTests
{
    private static string GetProjectFileName(string name)
    {
        return Path.Combine(Path.GetDirectoryName(NCrunchEnvironment.GetOriginalProjectPath()), name);
    }

    [ExclusivelyUses("files")]
    [InlineData("Bob_bouwer_2021_minor.pdf")]
    [InlineData("Bobby Bouwer 1960 oudere vacc.pdf")]
    [InlineData("Test Cert SK 2022-01.pdf")]
    [InlineData("Vacc Cert SK 2022-01.pdf")]
    [Theory]
    public void Final(string name)
    {
        var original = File.ReadAllBytes(GetProjectFileName(name));
        var parser = new PdfParser(original);
        Assert.True(parser.TryParse(out var result));
        Assert.StartsWith("HC1", result);
    }

    [ExclusivelyUses("files")]
    [InlineData("Vacc Cert SK 2022-01.pdf", 1, 30, 510, 240, 235, 3)]
    [Theory]
    public void NotQuiteFinal(string name, int page, int x, int y, int w, int h, int scale)
    {
        var original = File.ReadAllBytes(GetProjectFileName(name));
        var parser = new PdfParser(original);
        Assert.True(parser.TryParse(new PdfParserArgs(page, new Rectangle(x, y, w, h), scale), out var result));
        Assert.StartsWith("HC1", result);
    }

    [ExclusivelyUses("files")]
    [InlineData("Test Cert SK 2022-01.pdf", RGBLuminanceSource.BitmapFormat.RGB24, 0, 30, 510, 240, 235, 5)]
    [InlineData("Vacc Cert SK 2022-01.pdf", RGBLuminanceSource.BitmapFormat.RGB24, 1, 30, 510, 240, 235, 3)]
    [Theory]
    public void ParseFromMemoryStream(string name, RGBLuminanceSource.BitmapFormat f, int page, int x, int y, int w,
        int h, int scale)
    {
        var original = File.ReadAllBytes(GetProjectFileName(name));

        fpdfview.FPDF_InitLibrary();
        var doc = ReadFileFromBuffer(original);
        var p = fpdfview.FPDF_LoadPage(doc, page);
        var r = new RectangleF(x * scale, y * scale, w * scale, h * scale);
        var bitmap = Render(p, scale, r);
        using var ms = new MemoryStream();
        ms.Seek(0, SeekOrigin.Begin);
        bitmap.Save(ms, new BmpEncoder());
        bitmap.Save(GetProjectFileName($"{name}.bmp"), new BmpEncoder());
        var reader = new BarcodeReader();
        var result = reader.Decode(ms.ToArray(), (int) r.Width, (int) r.Height, f);
        Debug.WriteLine(result?.Text);
        Assert.True(result?.Text.StartsWith("HC1") ?? false);
    }

    protected unsafe Image<Bgra32> Render(FpdfPageT page, float scale, RectangleF rect)
    {
        var bitmap = fpdfview.FPDFBitmapCreateEx(
            (int) rect.Size.Width,
            (int) rect.Size.Height,
            (int) FPDFBitmapFormat.BGRA,
            IntPtr.Zero,
            0);

        fpdfview.FPDFBitmapFillRect(
            bitmap,
            0,
            0,
            (int) rect.Size.Width,
            (int) rect.Size.Height,
            Color.White.ToPixel<Argb32>().Argb);

        using var clipping = new FS_RECTF_
        {
            Left = 0,
            Right = rect.Size.Width,
            Bottom = 0,
            Top = rect.Size.Height
        };

        using var matrix = new FS_MATRIX_
        {
            A = scale,
            B = 0,
            C = 0,
            D = scale,
            E = -rect.X,
            F = -rect.Y
        };

        fpdfview.FPDF_RenderPageBitmapWithMatrix(bitmap, page, matrix, clipping,
            (int) RenderFlags.DisablePathAntialiasing);
        var buffer = fpdfview.FPDFBitmapGetBuffer(bitmap);
        return Image.WrapMemory<Bgra32>(buffer.ToPointer(), (int) rect.Size.Width, (int) rect.Size.Height);
    }

    private static unsafe FpdfDocumentT ReadFileFromBuffer(byte[] original)
    {
        fixed (byte* ptr = original)
        {
            var p = (IntPtr) ptr;
            return fpdfview.FPDF_LoadMemDocument(p, original.Length, "");
        }
    }
}