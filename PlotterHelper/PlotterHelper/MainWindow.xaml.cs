using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PlotterHelper {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        // TODO: check that the image has SIZE and DPI
        // TODO: show notification when export is done
        // TODO: sanitize all inputs
        // CONSIDER: show preview
        // TODO: cleanup data after saving

        // constants
        private const double MIN_CUT_WIDTH = 1; // inches
        private const double MIN_CUT_HEIGHT = 1; // inches


        private BitmapImage bitmapImage = null;

        public MainWindow() {
            InitializeComponent();
            // for the PDF generator
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private void LoadImageButtonClick(object sender, RoutedEventArgs e) {
            // loading image
            LoadImage();
            // setting default cut border
            SetUiToDefaults();
        }

        private void SavePdfButtonClick(object sender, RoutedEventArgs e) {
            // cutting, drawing marks, writing text
            BitmapImage procesedImage = Logic.ProcessImage(
                bitmapImage, 
                (int)(cutSliderX.Value / preview.ActualWidth * bitmapImage.Width), 
                (int)(cutSliderY.Value / preview.ActualHeight * bitmapImage.Height), 
                (int)(double.Parse(cutWidth.Text) * bitmapImage.DpiX), 
                (int)(double.Parse(cutHeight.Text) * bitmapImage.DpiY),
                int.Parse(sliceCount.Text));
            // saving the PDF file
            SavePdf(procesedImage);
        }

        private void CutSliderXValueChange(object sender, RoutedPropertyChangedEventArgs<double> e) {
            // not yet initialized check
            if (cutSliderX == null || cutSliderY == null) { return; }
            // setting new margin (by the slider values)
            cutBorder.Margin = new Thickness(cutSliderX.Value, cutSliderY.Value, 0, 0);
        }

        private void CutSliderYValueChange(object sender, RoutedPropertyChangedEventArgs<double> e) {
            // not yet initialized check
            if (cutSliderX == null || cutSliderY == null) { return; }
            // setting new margin (by the slider values)
            cutBorder.Margin = new Thickness(cutSliderX.Value, cutSliderY.Value, 0, 0);
        }

        private void ResizeCutButtonClick(object sender, RoutedEventArgs e) {
            // getting width and height
            double width = double.Parse(cutWidth.Text);
            double height = double.Parse(cutHeight.Text);
            // resizing the cut
            ResizeCut(width, height);
        }

        /// <summary>
        /// Resizes the cut control.
        /// </summary>
        /// <param name="width">The requested width in inches</param>
        /// <param name="height">The requested height in inches</param>
        private void ResizeCut(double width, double height) {
            // range constraints
            if (width < MIN_CUT_WIDTH) { width = MIN_CUT_WIDTH; }
            if (height < MIN_CUT_HEIGHT) { width = MIN_CUT_HEIGHT; }
            if (width > bitmapImage.WidthInches()) { width = bitmapImage.WidthInches(); }
            if (height > bitmapImage.HeightInches()) { height = bitmapImage.HeightInches(); }
            // calculating the cut size in pixels
            cutBorder.Width = width / bitmapImage.WidthInches() * preview.ActualWidth;
            cutBorder.Height = height / bitmapImage.HeightInches() * preview.ActualHeight;
            cutBorder.UpdateLayout();
            // setting slider maximums
            SetSliderMaximums();
            SetSliderPositions();
        }

        /// <summary>
        /// Loads the selected image. (jpg, jpeg, png and sgv images are supported)
        /// </summary>
        private void LoadImage() {
            // creating a file open dialog, setting filter, opening
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.svg) | *.jpg; *.jpeg; *.png; *.svg";
            bool? result = dialog.ShowDialog();
            // no file is selected, returning
            if (result != true) { return; }
            // getting the filename
            string path = dialog.FileName;
            // loading the file
            bitmapImage = IoHandler.LoadImage(path);
            preview.Source = bitmapImage;
            preview.UpdateLayout();
        }

        /// <summary>
        /// Sets the UI to defaults. (cut border, sliders, cut sizes)
        /// </summary>
        private void SetUiToDefaults() {
            // null check
            if (bitmapImage == null) { return; }
            // setting cut size (by the image dimensions)
            cutWidth.Text = bitmapImage.WidthInches().ToString("0.##");
            cutHeight.Text = bitmapImage.HeightInches().ToString("0.##");
            // setting the margin of the cut to the top-left of the image
            cutBorder.Margin = new Thickness(0, 0, 0, 0);
            // setting the size of the cut to the size of the image
            cutBorder.Width = preview.ActualWidth;
            cutBorder.Height = preview.ActualHeight;
            cutBorder.UpdateLayout();
            // setting the sliders maximum...
            SetSliderMaximums();
            // and actual value
            SetSliderPositions();
        }

        /// <summary>
        /// Sets the maximum values of the sliders according to the size difference of the image and the cur border.
        /// </summary>
        private void SetSliderMaximums() {
            // setting the sliders maximum...
            cutSliderX.Maximum = preview.ActualWidth - cutBorder.ActualWidth;
            cutSliderY.Maximum = preview.ActualHeight - cutBorder.ActualHeight;
            cutSliderX.UpdateLayout();
            cutSliderY.UpdateLayout();
        }

        /// <summary>
        /// Sets the values of the sliders according to the cut border margin.
        /// </summary>
        private void SetSliderPositions() {
            cutSliderX.Value = cutBorder.Margin.Left;
            cutSliderY.Value = cutBorder.Margin.Top;
            cutSliderX.UpdateLayout();
            cutSliderY.UpdateLayout();
        }

        /// <summary>
        /// Saves the selected region to a PDF file.
        /// </summary>
        private void SavePdf(BitmapImage bitmapImage) {
            // creating a save file dialog, setting filter, opening
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PDF files (*.pdf) | *.pdf";
            bool? result = dialog.ShowDialog();
            // no file is selected, returning
            if (result != true) { return; }
            // getting the filename
            string path = dialog.FileName;
            // saving the file
            IoHandler.SaveToPdf(bitmapImage, path);
        }
    }
}
