using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Svg;
using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PlotterHelper {

    class ImageHandler {

        private static readonly string[] rasterTypes = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] vectorTypes = { ".svg" };

        public static BitmapImage LoadImage(string path) {
            // getting the extension
            string extension = Path.GetExtension(path);
            // raster image types (loading directly)
            if (rasterTypes.Contains(extension.ToLower())) {
                return new BitmapImage(new Uri(path));
            }
            else if (vectorTypes.Contains(extension.ToLower())) {
                // loading the document
                SvgDocument svg = SvgDocument.Open<SvgDocument>(path);
                // returning rendered bitmap image
                return svg.Draw().ToBitmapImage();
            }
            // file not supported
            return null;
        }

        public static void SaveToPdf(BitmapImage bitmap, string path) {
            // creating a pdf file
            PdfDocument pdf = new PdfDocument();
            // creating a page
            PdfPage page = pdf.AddPage();
            // setting the page size
            page.Width = XUnit.FromInch(bitmap.Width / bitmap.DpiX);
            page.Height = XUnit.FromInch(bitmap.Height / bitmap.DpiY);
            // creating graphics
            XGraphics graphics = XGraphics.FromPdfPage(page);
            // creating image
            XImage image = XImage.FromBitmapSource(bitmap);
            // writing image
            graphics.DrawImage(image, 0, 0);
            // saving the file
            pdf.Save(path);
        }
    }
}
