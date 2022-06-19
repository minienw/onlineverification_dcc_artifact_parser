using SixLabors.ImageSharp;

namespace DccParser
{

    public class PdfParserArgs
    {
        public PdfParserArgs(int pageIndex, Rectangle rectangle, float scale)
        {
            PageIndex = pageIndex;
            Rectangle = rectangle;
            Scale = scale;
        }

        public Rectangle Rectangle { get; }
        public int PageIndex { get; }
        public float Scale { get; }

        public Rectangle GetScaledRectangle()
        {
            static int Transform(double arg, double scale) => (int)Math.Round(arg * scale, MidpointRounding.AwayFromZero);

            return new(
                Transform(Rectangle.Left, Scale),
                Transform(Rectangle.Top, Scale),
                Transform(Rectangle.Width, Scale),
                Transform(Rectangle.Height, Scale)
                );
        }
    }
}