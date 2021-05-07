using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PlotterHelper {
    public static class CutLogic {

        // TODO: test these!

        // constants
        private const double PRINTER_WIDTH = 52; // inch

        /// <summary>
        /// Cuts the specified portion from the provided Bitmap.
        /// </summary>
        /// <param name="input">The Bitmap to cut from</param>
        /// <param name="left">Left margin/space [px]</param>
        /// <param name="top">Top margin/space [px]</param>
        /// <param name="width">The width of the cut [px]</param>
        /// <param name="height">The height of the cut [px]</param>
        /// <returns>The cut portion</returns>
        public static Bitmap CutToSize(Bitmap input, int left, int top, int width, int height) {
            // cropping, returning
            return input.Clone(new Rectangle(left, top, width, height), input.PixelFormat);
        }

        /// <summary>
        /// Returns the total print length, if the image is put into the printer lengthwise.
        /// (the top of the image is the first to come out of the printer)
        /// </summary>
        /// <param name="image">The image to calculate</param>
        /// <param name="count">The count of image parts</param>
        /// <returns>The total length [inches]</returns>
        private static double GetLengthwiseLength(BitmapImage image, int count) {
            // width check
            if (image.WidthInches() > PRINTER_WIDTH) { return double.MaxValue; }
            // calculating the number of rows
            int rowCount = (int)(PRINTER_WIDTH / image.WidthInches());
            // calculating the step height [inches]
            double stepHeight = image.HeightInches() / count;
            // calculating and returning the maximum column height: maximum column count * element height [inches]
            return Math.Ceiling(count / (double)rowCount) * stepHeight;
        }

        /// <summary>
        /// Returns the total print length, if the image is put into the printer crosswise.
        /// (the left side of the image is the first to come out of the printer)
        /// </summary>
        /// <param name="image">The image to calculate</param>
        /// <param name="count">The count of image parts</param>
        /// <returns>The total length [inches]</returns>
        private static double GetCrosswiseLength(BitmapImage image, int count) {
            // calculating the step height [inches]
            double stepHeight = image.HeightInches() / count;
            // step height check
            if (stepHeight > PRINTER_WIDTH) { return double.MaxValue; }
            // calculating the number of columns
            int columnCount = (int)(PRINTER_WIDTH / stepHeight);
            // calculating the number of rows
            return Math.Ceiling(count / (double)columnCount) * image.WidthInches();

        }

        /// <summary>
        /// Returns the total print length, if the image is put into the printer in a mixed orientation: one part is
        /// put in crosswise, another part is put in lengthwise.
        /// (the left side of the image is the first to come out of the printer, then in an another part, the top of 
        /// the image)
        /// </summary>
        /// <param name="image">The image to calculate</param>
        /// <param name="count">The count of image parts</param>
        /// <returns>The total length [inches]</returns>
        private static double GetMixedwiseLength(BitmapImage image, int count) {
            // width check
            if (image.WidthInches() > PRINTER_WIDTH) { return double.MaxValue; }
            // calculating the step height [inches]
            double stepHeight = image.HeightInches() / count;
            // calculating the number of crosswise columns
            int crosswiseColumnCount = (int)(PRINTER_WIDTH / stepHeight);
            // calculating the number of crosswise rows
            int crosswiseRowCount = count % crosswiseColumnCount;
            // calculating the lengthwise row count
            int lengthwiseRowCount = count - crosswiseColumnCount * crosswiseRowCount;
            // calculating the total length
            return crosswiseRowCount * image.WidthInches() + lengthwiseRowCount * stepHeight;
        }
    }
}
