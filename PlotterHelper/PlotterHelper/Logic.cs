using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PlotterHelper {

    static class Logic {

        /// <summary>
        /// Cuts the specified portion from the provided BitmapImage.
        /// </summary>
        /// <param name="left">Left margin/space [px]</param>
        /// <param name="top">Top margin/space [px]</param>
        /// <param name="width">The width of the cut [px]</param>
        /// <param name="height">The height of the cut [px]</param>
        /// <returns>The cut portion</returns>
        public static BitmapSource CutBitmapImage(BitmapImage input, int left, int top, int width, int height) {
            return new CroppedBitmap(input, new System.Windows.Int32Rect(left, top, width, height));
        }

        public static BitmapImage RenderCutMarks(BitmapImage image) {

            RenderTargetBitmap target = new RenderTargetBitmap(image.PixelWidth, image.PixelHeight, 
                image.DpiX, image.DpiY, image.Format);
            DrawingVisual visual = new DrawingVisual();

            using (var r = visual.RenderOpen()) {
                r.DrawImage(image, new Rect(0, 0, image.PixelWidth, image.PixelHeight));
                r.DrawLine(new Pen(Brushes.Red, 10.0), new Point(0, 0), new Point(image.PixelWidth, image.PixelHeight));
                r.DrawText(new FormattedText(
                    "Hello", CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"), 24.0, Brushes.Black), new Point(100, 10));
            }

            target.Render(visual);

            visual.
        }
    }
}
