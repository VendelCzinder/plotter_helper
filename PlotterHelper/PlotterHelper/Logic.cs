using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace PlotterHelper {

    // TODO: settings



    static class Logic {
        
        public static BitmapImage ProcessImage(BitmapImage input, int left, int top, int width, int height, 
            int cutCount) {
            // cutting the image
            Bitmap bmp = CutLogic.CutToSize(input.ToBitmap(), left, top, width, height);
            // adding cutmarks
            OverlayLogic.RenderOverlay(bmp, cutCount, input.DpiX, input.DpiY);
            // optimizing layout
            BitmapImage output = CutLogic.PlaceImage(bmp, cutCount, input.DpiX, input.DpiY);
            // converting and returning the image
            return output;
        }
    }
}
