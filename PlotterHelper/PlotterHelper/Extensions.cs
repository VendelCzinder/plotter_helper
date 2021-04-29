using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace PlotterHelper {
    public static class Extensions {

        /// <summary>
        /// Returns the width of the image in inches
        /// </summary>
        /// <param name="bitmap">A bitmap image, both PixelWidth and DpiX must be set!</param>
        /// <returns>The width of the image in inches</returns>
        public static double WidthInches(this BitmapImage bitmap) { 
            return bitmap.PixelWidth / bitmap.DpiX;
        }

        /// <summary>
        /// Returns the height of the image in inches
        /// </summary>
        /// <param name="bitmap">A bitmap image, both PixelHeight and DpiY must be set!</param>
        /// <returns>The height of the image in inches</returns>
        public static double HeightInches(this BitmapImage bitmap) {
            return bitmap.PixelHeight / bitmap.DpiY;
        }

        /// <summary>
        /// Takes a bitmap and converts it to an image that can be handled by WPF ImageBrush
        /// </summary>
        /// <param name="bitmap">A bitmap image</param>
        /// <returns>The image as a BitmapImage for WPF</returns>
        public static BitmapImage ToBitmapImage(this Bitmap bitmap) {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        // spare method
        /// <summary>
        /// Converts a Bitmap to a BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        //public static BitmapImage ToBitmapImage(this Bitmap bitmap) {
        //    using (MemoryStream memory = new MemoryStream()) {
        //        bitmap.Save(memory, ImageFormat.Png);
        //        memory.Position = 0;
        //        BitmapImage bitmapImage = new BitmapImage();
        //        bitmapImage.BeginInit();
        //        bitmapImage.StreamSource = memory;
        //        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmapImage.EndInit();
        //        bitmapImage.Freeze();
        //        return bitmapImage;
        //    }
        //}
    }
}
