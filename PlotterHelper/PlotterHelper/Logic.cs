using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace PlotterHelper {

    static class Logic {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="left">[px]</param>
        /// <param name="top">[px]</param>
        /// <param name="width">[px]</param>
        /// <param name="height">[px]</param>
        /// <param name="cutCount"></param>
        /// <returns></returns>
        public static BitmapImage ProcessImage(BitmapImage input, int left, int top, int width, int height, 
            double dpiX, double dpiY, int cutCount, Settings settings) {
            // cutting the image
            Bitmap bmp = CutLogic.CutToSize(input.ToBitmap(), left, top, width, height, settings);
            // adding cutmarks
            OverlayLogic.RenderOverlay(bmp, cutCount, dpiX, dpiY, settings);
            // optimizing layout
            BitmapImage output = CutLogic.PlaceImage(bmp, cutCount, input.DpiX, input.DpiY, settings);
            // converting and returning the image
            return output;
        }
    }
}
