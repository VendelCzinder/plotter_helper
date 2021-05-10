using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PlotterHelper {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        
        // TODO: show notification when export is done

        // constants
        private const double MIN_CUT_WIDTH = 1; // inches
        private const double MIN_CUT_HEIGHT = 1; // inches

        private BitmapImage bitmapImage = null;

        public MainWindow() {
            InitializeComponent();
            // for the PDF generator
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e) {
            SetUiToDefaults();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            MinHeight = Height;
        }

        private void LoadImageButtonClick(object sender, RoutedEventArgs e) {
            // loading image
            LoadImage();
            // setting default cut border
            SetUiToDefaults();
        }

        private void ResizeCutButtonClick(object sender, RoutedEventArgs e) {
            // image check
            if (bitmapImage == null) {
                // error
                MessageBox.Show("Please load an image first!", "Error - missing image",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // variables
            double width, height, stepHeight; // [inches]
            int count;
            // trying to get the width
            if (!double.TryParse(cutWidthInput.Text, out width)) {
                // error
                MessageBox.Show("The cut width must be specified!", "Error - missing data",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // range check
            if (width <= 0 || width > bitmapImage.WidthInches() + 0.01) {
                // error
                MessageBox.Show("The cut width must be between zero and the width of the image!",
                    "Error - wrong input",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // trying to get number of steps
            if (!int.TryParse(stepCountInput.Text, out count)) {
                // error
                MessageBox.Show("The number of steps must be specified!", "Error - missing data",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // range check
            if (count <= 0) {
                // error
                MessageBox.Show("The step count must be larger than zero!",
                    "Error - wrong input",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // trying to get the step height
            if (double.TryParse(stepHeightInput.Text, out stepHeight) && stepHeight != 0) {
                height = stepHeight * count;
                // range check
                if (height > bitmapImage.HeightInches() + 0.01) {
                    // error
                    MessageBox.Show("The step height times the cut count must not exceed the the image height!",
                        "Error - wrong input",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // updating the cut height textbox
                cutHeightInput.Text = height.ToString("0.##");
            }
            else {
                // trying to get the height
                if (!double.TryParse(cutHeightInput.Text, out height)) {
                    // error
                    MessageBox.Show("The cut height OR the step heigh must be specified!",
                        "Error - missing data",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // range check
                if (width <= 0 || width > bitmapImage.WidthInches() + 0.01) {
                    // error
                    MessageBox.Show("The cut height must be between zero and the height of the image!",
                        "Error - wrong input",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                stepHeight = height / count;
                // updating the step height textbox
                stepHeightInput.Text = stepHeight.ToString("0.##");
            }
            // resizing the cut
            ResizeCut(width, height);
        }

        private void SavePdfButtonClick(object sender, RoutedEventArgs e) {
            // showing spinner
            spinner.Visibility = Visibility.Visible;
            spinner.UpdateLayout();
            // cutting, drawing marks, writing text
            BitmapImage procesedImage = Logic.ProcessImage(
                bitmapImage, 
                (int)(cutSliderX.Value / preview.ActualWidth * bitmapImage.PixelWidth), 
                (int)(cutSliderY.Value / preview.ActualHeight * bitmapImage.PixelHeight), 
                (int)(double.Parse(cutWidthInput.Text) * bitmapImage.DpiX), 
                (int)(double.Parse(cutHeightInput.Text) * bitmapImage.DpiY),
                int.Parse(stepCountInput.Text));
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
            // enabling button
            saveImageButton.IsEnabled = true;
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
            // showing spinner
            spinner.Visibility = Visibility.Visible;
            spinner.UpdateLayout();
            // loading the file
            bitmapImage = IoHandler.LoadImage(path);
            // hiding spinner
            spinner.Visibility = Visibility.Hidden;
            // size check
            if (bitmapImage.PixelWidth == 0 || bitmapImage.PixelHeight == 0) {
                // error
                MessageBox.Show("The image is missing width or height data! Please set it before loading it!",
                    "Error - missing image metadata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                bitmapImage = null;
                return;
            }
            // DPI check
            if (bitmapImage.DpiX == 0 || bitmapImage.DpiY == 0) {
                // error
                MessageBox.Show("The image is missing vertical or horizontal DPI data! " +
                    "Please set it before loading it!",
                    "Error - missing image metadata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                bitmapImage = null;
                return;
            }
            preview.Source = bitmapImage;
            preview.UpdateLayout();
            // enabling button
            updateCutButton.IsEnabled = true;
        }

        /// <summary>
        /// Sets the UI to defaults. (cut border, sliders, cut sizes)
        /// </summary>
        private void SetUiToDefaults() {
            // null check
            if (bitmapImage == null) { return; }
            // setting cut size (by the image dimensions)
            cutWidthInput.Text = bitmapImage.WidthInches().ToString("0.##");
            cutHeightInput.Text = bitmapImage.HeightInches().ToString("0.##");
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
