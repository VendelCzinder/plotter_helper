using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace PlotterHelper {
    public static class Extensions {

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
