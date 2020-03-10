using Masuit.Tools;
using System.Drawing;
using System.Drawing.Text;
using System.IO;

namespace Masuit.MyBlogs.Core.Common
{
    public class ImageWatermarker
    {
        public bool SkipWatermarkForSmallImages { get; set; }

        public int SmallImagePixelsThreshold { get; set; }

        private readonly Stream _stream;

        public ImageWatermarker(Stream originStream)
        {
            _stream = originStream;
        }

        public MemoryStream AddWatermark(string watermarkText, Color color, WatermarkPosition watermarkPosition = WatermarkPosition.BottomRight, int textPadding = 10, int fontSize = 20, Font font = null, bool textAntiAlias = true)
        {
            using var img = Image.FromStream(_stream);
            if (SkipWatermarkForSmallImages && img.Height * img.Width < SmallImagePixelsThreshold)
            {
                return _stream.SaveAsMemoryStream();
            }

            using var graphic = Graphics.FromImage(img);
            if (textAntiAlias)
            {
                graphic.TextRenderingHint = TextRenderingHint.AntiAlias;
            }

            using var brush = new SolidBrush(color);
            using var f = font ?? new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold, GraphicsUnit.Pixel);
            var textSize = graphic.MeasureString(watermarkText, f);
            int x, y;

            switch (watermarkPosition)
            {
                case WatermarkPosition.TopLeft:
                    x = textPadding;
                    y = textPadding;
                    break;
                case WatermarkPosition.TopRight:
                    x = img.Width - (int)textSize.Width - textPadding;
                    y = textPadding;
                    break;
                case WatermarkPosition.BottomLeft:
                    x = textPadding;
                    y = img.Height - (int)textSize.Height - textPadding;
                    break;
                case WatermarkPosition.BottomRight:
                    x = img.Width - (int)textSize.Width - textPadding;
                    y = img.Height - (int)textSize.Height - textPadding;
                    break;
                default:
                    x = textPadding;
                    y = textPadding;
                    break;
            }

            graphic.DrawString(watermarkText, f, brush, new Point(x, y));
            var ms = new MemoryStream();
            img.Save(ms, img.RawFormat);
            ms.Position = 0;
            return ms;
        }
    }
}