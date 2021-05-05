using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace PlotterHelper {

    // TODO: settings



    static class Logic {

        // constants
        private const double LINE_LENGTH = 1; // inch
        private const double LINE_WIDTH = 0.05; // inch
        private const double TEXT_TOP_MARGIN = 0.5; // inch
        private const double TEXT_RIGHT_MARGIN = 0.1; // inch
        private const double TEXT_SIZE = 0.2; // inch

        public static BitmapImage ProcessImage(BitmapImage input, int left, int top, int width, int height, int cutCount) {
            // cutting the image
            Bitmap bmp = CutBitmapImage(input, left, top, width, height);
            // adding cutmarks
            RenderOverlay(bmp, cutCount, input.DpiX, input.DpiY);
            // converting and returning the image
            return bmp.ToBitmapImage();
        }

        /// <summary>
        /// Cuts the specified portion from the provided BitmapImage.
        /// </summary>
        /// <param name="left">Left margin/space [px]</param>
        /// <param name="top">Top margin/space [px]</param>
        /// <param name="width">The width of the cut [px]</param>
        /// <param name="height">The height of the cut [px]</param>
        /// <returns>The cut portion</returns>
        private static Bitmap CutBitmapImage(BitmapImage input, int left, int top, int width, int height) {
            // converting to bitmap
            Bitmap bmp = input.ToBitmap();
            // cropping, returning
            return bmp.Clone(new Rectangle(left, top, width, height), bmp.PixelFormat);
        }

        private static void RenderOverlay(Bitmap input, int count, double dpiX, double dpiY) {
            // calculating sizes
            double lineLength = LINE_LENGTH * dpiX;
            double lineWidth = LINE_WIDTH * dpiY;
            double textSize = TEXT_SIZE * dpiX;
            double textRightMargin = TEXT_RIGHT_MARGIN * dpiX;
            double step = input.Height / (double)count;
            // creating a Graphics object
            using Graphics graphics = Graphics.FromImage(input);
            // loop for the drawing
            for (int i = 1; i < count; i++) {
                // calculating the top position for the line
                int lineTop = (int)((step * i) - (lineWidth / 2));
                // drawing the line
                graphics.DrawLine(new Pen(Brushes.Red, (float)lineWidth), 
                    input.Width - (int)lineLength, lineTop, input.Width, lineTop);
                // calculating the top position for the text
                int textTop = (int)(lineTop + lineWidth + TEXT_TOP_MARGIN);
                // calculating text width
                var textWidth = graphics.MeasureString((count - i + 1).ToString(), 
                    new Font(FontFamily.GenericSansSerif, (float)textSize)).Width;
                // drawing the text
                graphics.DrawString((count - i).ToString(), new Font(FontFamily.GenericSansSerif, (float)textSize),
                    Brushes.Red, new PointF((float)(input.Width - textWidth - textRightMargin), textTop));
            }           
            // saving the drawing
            graphics.Flush();
        }
    }
}
