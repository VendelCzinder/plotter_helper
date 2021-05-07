using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotterHelper {
    public static class OverlayLogic {

        // constants
        private const double LINE_LENGTH = 0.25; // inch
        private const double LINE_WIDTH = 0.02; // inch
        private const double TEXT_TOP_MARGIN = 0.1; // inch
        private const double TEXT_RIGHT_MARGIN = 0.02; // inch
        private const double TEXT_SIZE = 0.1; // inch
        private const int COLOR_ALPHA = 200; // in pixel intensity [0-255]

        public static void RenderOverlay(Bitmap input, int count, double dpiX, double dpiY) {
            // calculating sizes
            double lineLength = LINE_LENGTH * dpiX;
            double lineWidth = LINE_WIDTH * dpiY;
            double textSize = TEXT_SIZE * dpiX;
            double textRightMargin = TEXT_RIGHT_MARGIN * dpiX;
            double step = input.Height / (double)count;
            // creating the font
            Font font = new Font(FontFamily.GenericSansSerif, (float)textSize);
            // creating a Graphics object
            using Graphics graphics = Graphics.FromImage(input);
            // loop for the drawing
            for (int i = 0; i < count; i++) {
                // calculating the top position for the line
                float lineTop = (float)((step * i) - (lineWidth / 2));
                // creating the number
                string number = (count - i).ToString();
                // drawing the lines (if not the first cut)
                if (i != 0) {
                    DrawLines(input, graphics, input.Width, (float)lineLength, lineTop, lineWidth);
                }
                // drawing the text
                DrawText(input, graphics, number, font, lineTop, lineWidth, textRightMargin);
            }
            // saving the drawing
            graphics.Flush();
        }

        private static Color GetAverageColor(Bitmap bmp, int left, int top, int width, int height) {
            // variables
            long r = 0, g = 0, b = 0;
            int count = width * height;
            // loops
            for (int x = left; x < left + width; x++) {
                for (int y = top; y < top + height; y++) {
                    // index check
                    if (x < 0 || x >= bmp.Width || y < 0 || y >= bmp.Height) {
                        continue;
                    }
                    // summing
                    Color pixel = bmp.GetPixel(x, y);
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                }
            }
            // calculating average, creating color, returning
            return Color.FromArgb((int)(r / count), (int)(g / count), (int)(b / count));
        }

        private static void DrawLines(Bitmap bmp, Graphics graphics, int totalWidth, float lineLength, float lineTop,
            double lineWidth) {
            // calculating positions for left line
            float x1 = 0;
            float y = lineTop;
            float x2 = lineLength;
            // getting the color and offset color for left line
            Color averageColor = GetAverageColor(bmp, (int)x1,
                (int)(lineTop - (lineWidth / 2)),
                (int)lineLength, (int)(lineTop + (lineWidth / 2)));
            Color offsetColor = GetOffsetColor(averageColor, COLOR_ALPHA);
            // drawing the left line
            graphics.DrawLine(new Pen(offsetColor, (float)lineWidth), x1, y, x2, y);
            // calculating positions for righ line
            x1 = totalWidth - lineLength;
            x2 = totalWidth;
            // getting the color and offset color for righ line
            averageColor = GetAverageColor(bmp, (int)x1,
                (int)(lineTop - (lineWidth / 2)),
                (int)lineLength, (int)(lineTop + (lineWidth / 2)));
            offsetColor = GetOffsetColor(averageColor, COLOR_ALPHA);
            // drawing the righ line
            graphics.DrawLine(new Pen(offsetColor, (float)lineWidth), x1, y, x2, y);
        }

        private static void DrawText(Bitmap bmp, Graphics graphics, string text, Font font, float lineTop,
            double lineWidth, double textRightMargin) {
            // calculating the top position for the text
            float textTop = (float)(lineTop + lineWidth + TEXT_TOP_MARGIN);
            // calculating text size
            SizeF textSize = graphics.MeasureString(text, font);
            // left position
            float textLeft = (float)(bmp.Width - textSize.Width - textRightMargin);
            // getting the color and offset color for left line
            Color averageColor = GetAverageColor(bmp, (int)textLeft, (int)textTop,
                (int)textSize.Width, (int)textSize.Height);
            Color offsetColor = GetOffsetColor(averageColor, COLOR_ALPHA);
            // drawing the text
            graphics.DrawString(text, font, new SolidBrush(offsetColor), new PointF(textLeft, textTop));
        }

        private static Color GetOffsetColor(Color color, int alpha) {
            // getting HLS
            (double h, double l, double s) = RgbToHls(color.R, color.G, color.B);
            // changing the luminance
            if (l > 0.5) {
                l = 0;
            }
            else {
                l = 1;
            }
            // getting RGB
            (int r, int g, int b) = HlsToRgb(h, l, s);
            // returning
            return Color.FromArgb(alpha, r, g, b);
        }

        private static (double h, double l, double s) RgbToHls(int r, int g, int b) {
            // creating variables
            double h, l, s;
            // Convert RGB to a 0.0 to 1.0 range.
            double double_r = r / 255.0;
            double double_g = g / 255.0;
            double double_b = b / 255.0;
            // Get the maximum and minimum RGB components.
            double max = double_r;
            if (max < double_g) max = double_g;
            if (max < double_b) max = double_b;

            double min = double_r;
            if (min > double_g) min = double_g;
            if (min > double_b) min = double_b;

            double diff = max - min;
            l = (max + min) / 2;
            if (Math.Abs(diff) < 0.00001) {
                s = 0;
                h = 0;  // H is really undefined.
            }
            else {
                if (l <= 0.5) s = diff / (max + min);
                else s = diff / (2 - max - min);

                double r_dist = (max - double_r) / diff;
                double g_dist = (max - double_g) / diff;
                double b_dist = (max - double_b) / diff;

                if (double_r == max) h = b_dist - g_dist;
                else if (double_g == max) h = 2 + r_dist - b_dist;
                else h = 4 + g_dist - r_dist;

                h = h * 60;
                if (h < 0) h += 360;
            }
            // returning
            return (h, l, s);
        }

        private static (int r, int g, int b) HlsToRgb(double h, double l, double s) {
            // creating variables
            int r, g, b;
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double double_r, double_g, double_b;
            if (s == 0) {
                double_r = l;
                double_g = l;
                double_b = l;
            }
            else {
                double_r = QqhToRgb(p1, p2, h + 120);
                double_g = QqhToRgb(p1, p2, h);
                double_b = QqhToRgb(p1, p2, h - 120);
            }
            // Convert RGB to the 0 to 255 range.
            r = (int)(double_r * 255.0);
            g = (int)(double_g * 255.0);
            b = (int)(double_b * 255.0);
            //returning
            return (r, g, b);
        }

        private static double QqhToRgb(double q1, double q2, double hue) {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }
    }
}
