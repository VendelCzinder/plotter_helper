using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PlotterHelper {
    public static class CutLogic {

        public enum CutPolicy {
            Lengthwise,
            Crosswise,
            Mixed
        }

        // constants
        private const double PRINTER_WIDTH = 12; // [inches]

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
            // creating output
            Bitmap output = new Bitmap(width, height);
            // creating graphics, copying the image part
            using Graphics graphics = Graphics.FromImage(output);
            graphics.DrawImage(
                input,
                new Rectangle(0, 0, width, height),
                new Rectangle(left, top, width, height),
                GraphicsUnit.Pixel);
            // writing changes
            graphics.Flush();
            // returning
            return output;
        }

        // TODO: handle if print is too big, cannot print

        public static BitmapImage PlaceImage(Bitmap input, int count, double dpiX, double dpiY) {
            // getting the length and count values from the different cut policies
            (int rowCount, int columnCount, double totalLength) lengthwise = GetLengthwiseLength(input, count, 
                dpiX, dpiY);
            (int rowCount, int columnCount, double totalLength) crosswise = GetCrosswiseLength(input, count, 
                dpiX, dpiY);
            (int crosswiseRowCount, int crosswiseColumnCount, int lengthwiseRowCount, int lengthwiseColumnCount,
                double totalLength) mixed = GetMixedwiseLength(input, count, dpiX, dpiY);
            // calcualting the minimum value
            if (lengthwise.totalLength <= crosswise.totalLength && lengthwise.totalLength <= mixed.totalLength) {
                // lengthwise
                return CutBitmapLengthwise(input, count, lengthwise.rowCount, lengthwise.columnCount)
                    .ToBitmapImage();
            }
            else if (crosswise.totalLength <= mixed.totalLength) {
                // crosswise
                return CutBitmapCrosswise(input, count, crosswise.rowCount, crosswise.columnCount)
                    .ToBitmapImage();
            }
            else {
                // mixed
                return CutBitmapMixed(input, count, mixed.crosswiseRowCount, mixed.crosswiseColumnCount,
                    mixed.lengthwiseRowCount, mixed.lengthwiseColumnCount, mixed.totalLength).ToBitmapImage();
            }
        }

        private static Bitmap CutBitmapLengthwise(Bitmap image, int totalCount, int rowCount, int columnCount) {
            // calculating the step height [px]
            double stepHeight = image.Height / (double)totalCount;
            // calculating the dimensions of the new image
            int newWidth = image.Width * columnCount;
            int newHeight = (int)Math.Round(rowCount * stepHeight);
            // creating a new bitmap and graphics
            Bitmap output = new Bitmap(newWidth, newHeight);
            output.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using Graphics graphics = Graphics.FromImage(output);
            // clearing the bitmap to white
            graphics.Clear(Color.White);
            // copying parts to the output image
            for (int i = 0; i < columnCount; i++) {
                // source cut top (left is always zero)
                int sourceCutTop = (int)Math.Round(rowCount * stepHeight * i);
                // source cut bottom (and bottom of image overflow check)
                int sourceCutBottom = Math.Min((int)Math.Round(rowCount * stepHeight * (i + 1)), image.Height);
                // source rectangle
                Rectangle sourceRect = new Rectangle(0, sourceCutTop, image.Width, sourceCutBottom - sourceCutTop);
                // destination cut left (top is always zero)
                int leftPosition = image.Width * i;
                // destination rectangle
                Rectangle destRect = new Rectangle(leftPosition, 0, sourceRect.Width, sourceRect.Height);
                // doing the copy
                graphics.DrawImage(image, destRect, sourceRect, GraphicsUnit.Pixel);
            }
            // writing the new image
            graphics.Flush();
            // returning output
            return output;
        }

        private static Bitmap CutBitmapCrosswise(Bitmap image, int totalCount, int rowCount, int columnCount) {
            // rotating the image
            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            // calculating the step width [px]
            double stepWidth = image.Width / (double)totalCount;
            // calculating the dimensions of the new image
            int newWidth = (int)(stepWidth * columnCount);
            int newHeight = image.Height * rowCount;
            // creating a new bitmap and graphics
            Bitmap output = new Bitmap(newWidth, newHeight);
            output.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using Graphics graphics = Graphics.FromImage(output);
            // clearing the bitmap to white
            graphics.Clear(Color.White);
            // copying parts to the output image
            for (int i = 0; i < columnCount; i++) {
                // source cut left (top is always zero)
                int sourceCutLeft = (int)Math.Round(columnCount * stepWidth * i);
                // source cut right (and right of image overflow check)
                int sourceCutRight = Math.Min((int)Math.Round(columnCount * stepWidth * (i + 1)), image.Width);
                // source rectangle
                Rectangle sourceRect = new Rectangle(sourceCutLeft, 0, sourceCutRight - sourceCutLeft, image.Height);
                // destination cut top (left is always zero)
                int topPosition = image.Height * i;
                // destination rectangle
                Rectangle destRect = new Rectangle(0, topPosition, sourceRect.Width, sourceRect.Height);
                // doing the copy
                graphics.DrawImage(image, destRect, sourceRect, GraphicsUnit.Pixel);
            }
            // writing the new image
            graphics.Flush();
            // returning output
            return output;
        }

        private static Bitmap CutBitmapMixed(Bitmap image, int totalCount, int crosswiseRowCount, 
            int crosswiseColumnCount, int lengthwiseRowCount, int lengthwiseColumnCount, double totalLength) {
            // calculating the step size (width/height depending on orientation) [px]
            double stepSize = image.Height / (double)totalCount;
            // calculating the dimensions of the new image
            int newWidth = Math.Max((int)(stepSize * crosswiseColumnCount), image.Width);
            int newHeight = (int)(image.Width * crosswiseRowCount + stepSize * lengthwiseRowCount);
            // creating a new bitmap and graphics
            Bitmap output = new Bitmap(newWidth, newHeight);
            output.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            using Graphics graphics = Graphics.FromImage(output);
            // clearing the bitmap to white
            graphics.Clear(Color.White);
            // rotating the input image
            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            // copying crosswise parts to the output image
            for (int i = 0; i < crosswiseRowCount; i++) {
                // source cut left (top is always zero)
                int sourceCutLeft = (int)Math.Round(crosswiseColumnCount * stepSize * i);
                // source cut right (and right of image overflow check)
                int sourceCutRight = (int)Math.Round(crosswiseColumnCount * stepSize * (i + 1));
                // source rectangle
                Rectangle sourceRect = new Rectangle(sourceCutLeft, 0, sourceCutRight - sourceCutLeft, image.Height);
                // destination cut top (left is always zero)
                int topPosition = image.Height * i;
                // destination rectangle
                Rectangle destRect = new Rectangle(0, topPosition, sourceRect.Width, sourceRect.Height);
                // doing the copy
                graphics.DrawImage(image, destRect, sourceRect, GraphicsUnit.Pixel);
            }
            // rotating the input image back to its original orientation
            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            // starting positions for the lengthwise part
            int sourceStartTop = (int)(stepSize * crosswiseColumnCount * crosswiseRowCount);
            int destinationStartTop = (int)(crosswiseRowCount * image.Width);
            // copying lengthwise parts to the output image
            for (int i = 0; i < lengthwiseColumnCount; i++) {
                // source cut top (left is always zero)
                int sourceCutTop = (int)Math.Round(sourceStartTop + lengthwiseRowCount * stepSize * i);
                // source cut bottom (and bottom of image overflow check)
                int sourceCutBottom = Math.Min((int)Math
                    .Round(sourceStartTop + lengthwiseRowCount * stepSize * (i + 1)), image.Height);
                // source rectangle
                Rectangle sourceRect = new Rectangle(0, sourceCutTop, image.Width, sourceCutBottom - sourceCutTop);
                // destination cut left (top is always zero)
                int leftPosition = image.Width * i;
                // destination rectangle
                Rectangle destRect = new Rectangle(leftPosition, destinationStartTop, sourceRect.Width, 
                    sourceRect.Height);
                // doing the copy
                graphics.DrawImage(image, destRect, sourceRect, GraphicsUnit.Pixel);
            }
            // writing the new image
            graphics.Flush();
            // returning output
            return output;
        }

        /// <summary>
        /// Returns the row and column counts and the total print length, 
        /// if the image is put into the printer lengthwise.
        /// (the top of the image is the first to come out of the printer)
        /// </summary>
        /// <param name="image">The image to calculate</param>
        /// <param name="count">The count of image parts</param>
        /// <returns>The row and column count and the total print length [inches]</returns>
        private static (int rowCount, int columnCount, double totalLength) GetLengthwiseLength(Bitmap image,
            int count, double dpiX, double dpiY) {
            // sizes [inches]
            double widthInches = image.Width / dpiX;
            double heightInches = image.Height / dpiY;
            // width check
            if (widthInches > PRINTER_WIDTH) { return (0, 0, double.MaxValue); }
            // calculating the number of columns
            int columnCount = (int)(PRINTER_WIDTH / widthInches);
            // calculating the step height [inches]
            double stepHeight = heightInches / count;
            // calculating the row count
            int rowCount = (int)Math.Ceiling(count / (double)columnCount);
            // calculating and returning the maximum column height: maximum column count * element height [inches]
            return (rowCount, columnCount, rowCount * stepHeight);
        }

        /// <summary>
        /// Returns the total print length, if the image is put into the printer crosswise.
        /// (the left side of the image is the first to come out of the printer)
        /// </summary>
        /// <param name="image">The image to calculate</param>
        /// <param name="count">The count of image parts</param>
        /// <returns>The total length [inches]</returns>
        private static (int rowCount, int columnCount, double totalLength) GetCrosswiseLength(Bitmap image, 
            int count, double dpiX, double dpiY) {
            // sizes [inches]
            double widthInches = image.Width / dpiX;
            double heightInches = image.Height / dpiY;
            // calculating the step height [inches]
            double stepHeight = heightInches / count;
            // step height check
            if (stepHeight > PRINTER_WIDTH) { return (0, 0, double.MaxValue); }
            // calculating the number of columns
            int columnCount = (int)(PRINTER_WIDTH / stepHeight);
            // calculating the number of rows
            int rowcount = (int)Math.Ceiling(count / (double)columnCount);
            // calculating the number of rows
            return (rowcount, columnCount, rowcount * widthInches);
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
        private static (int crosswiseRowCount, int crosswiseColumnCount, int lengthwiseRowCount, 
            int lengthwiseColumnCount, double totalLength) GetMixedwiseLength(Bitmap image, int count, 
            double dpiX, double dpiY) {
            // sizes [inches]
            double widthInches = image.Width / dpiX;
            double heightInches = image.Height / dpiY;
            // width check
            if (widthInches > PRINTER_WIDTH) { return (0, 0, 0, 0, double.MaxValue); }
            // calculating the step height [inches]
            double stepHeight = heightInches / count;
            // step height check
            if (stepHeight > PRINTER_WIDTH) { return (0, 0, 0, 0, double.MaxValue); }
            // calculating the number of crosswise columns
            int crosswiseColumnCount = Math.Min((int)(PRINTER_WIDTH / stepHeight), count);
            // calculating the number of crosswise rows
            int crosswiseRowCount = Math.Max(count / crosswiseColumnCount, 1);
            // calculating the number of lengthwise pieces
            int lengthwiseCount = count - (crosswiseColumnCount * crosswiseRowCount);
            // calculating the number of lengthwise columns
            int lengthwiseColumnCount = (int)(PRINTER_WIDTH / widthInches);
            // calculating the (maximum) lengthwise row count
            int lengthwiseRowCount = (int)Math.Ceiling(lengthwiseCount / (double)lengthwiseColumnCount);
            // calculating the total length
            return (crosswiseRowCount, crosswiseColumnCount, lengthwiseRowCount, lengthwiseColumnCount,
                crosswiseRowCount * widthInches + lengthwiseRowCount * stepHeight);
        }
    }
}
