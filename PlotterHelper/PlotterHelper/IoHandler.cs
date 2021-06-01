using Newtonsoft.Json;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Svg;
using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PlotterHelper {

    class IoHandler {

        private const string SETTINGS_FILE_NAME = "setting.json";

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

        public static void SaveToPdf(BitmapSource bitmap, string path, double dpiX, double dpiY) {
            // creating a pdf file
            PdfDocument pdf = new PdfDocument();
            // creating a page
            PdfPage page = pdf.AddPage();
            // setting the page size
            page.Width = XUnit.FromInch(bitmap.PixelWidth / dpiX);
            page.Height = XUnit.FromInch(bitmap.PixelHeight / dpiY);
            // calculating image size
            double imageWidth = bitmap.PixelWidth / bitmap.DpiX; // [inches]
            double imageHeight = bitmap.PixelHeight / bitmap.DpiY; // [inches]
            // creating graphics
            XGraphics graphics = XGraphics.FromPdfPage(page);
            // creating image
            XImage image = XImage.FromBitmapSource(bitmap);
            // writing image
            graphics.DrawImage(image, 
                new XRect(0, 0, page.Width, page.Height),
                new XRect(0, 0, imageWidth, imageHeight),
                XGraphicsUnit.Inch);
            // saving the file
            pdf.Save(path);
        }

        public static void SaveSettings(Settings settings) {
            // getting the data folder
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // saving data
            File.WriteAllText(Path.Join(path, SETTINGS_FILE_NAME), JsonConvert.SerializeObject(settings));
        }

        public static Settings LoadSettings() {
            // getting the data folder
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // does the file exist?
            if (File.Exists(Path.Join(path, SETTINGS_FILE_NAME))) {
                // getting data
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Join(path, SETTINGS_FILE_NAME)));
            }
            else {
                // returning the default settings object
                return new Settings();
            }
        }
    }
}
