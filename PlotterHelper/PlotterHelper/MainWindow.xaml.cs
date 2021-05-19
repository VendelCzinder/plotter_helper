using Microsoft.Win32;
using System;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PlotterHelper {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        
        // constants
        private const double MIN_CUT_WIDTH = 1; // inches
        private const double MIN_CUT_HEIGHT = 1; // inches

        private Settings settings = null;
        private BitmapImage bitmapImage = null;
        private Rect cutArea = new Rect(0, 0, 0, 0); // parameters in inches!

        public MainWindow() {
            InitializeComponent();
            // for the PDF generator
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // loading settings
            settings = IoHandler.LoadSettings();
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateUiOnResize();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            MinHeight = Height;
        }

        private void LoadImageButtonClick(object sender, RoutedEventArgs e) {
            // loading image
            LoadImage();
            // setting default cut border
            SetUiToDefaults();
            // showing image information
            SetImageInfo();
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
                if (height > bitmapImage.HeightInches() + 0.01) {
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
            // storing the size
            cutArea.Size = new Size(width, height);
            // resizing the cut
            ResizeCutControl();
            // setting slider maximums
            SetSliderMaximums();
            // enabling button
            saveImageButton.IsEnabled = true;
        }

        private void SavePdfButtonClick(object sender, RoutedEventArgs e) {
            // showing spinner
            spinner.Visibility = Visibility.Visible;
            spinner.UpdateLayout();
            // cutting, drawing marks, writing text
            BitmapImage procesedImage = Logic.ProcessImage(
                bitmapImage,
                (int)Math.Round(cutArea.Left * bitmapImage.DpiX), 
                (int)Math.Round(cutArea.Top * bitmapImage.DpiY),
                (int)Math.Round(double.Parse(cutWidthInput.Text) * bitmapImage.DpiX), 
                (int)Math.Round(double.Parse(cutHeightInput.Text) * bitmapImage.DpiY),
                int.Parse(stepCountInput.Text),
                settings);
            // null check
            if (procesedImage == null) {
                // error
                MessageBox.Show("The image cannot be printed as the steps are bigger than the printer! " +
                    "Consider lowering the image width or the step height, or changing the settings!",
                    "Error - image too big",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                // hiding spinner
                spinner.Visibility = Visibility.Hidden;
                // returning
                return;
            }
            // saving the PDF file
            SavePdf(procesedImage);
            // hiding spinner
            spinner.Visibility = Visibility.Hidden;
        }

        private void OpenSettingsClick(object sender, RoutedEventArgs e) {
            // opening a settings Window
            SettingsWindow settingsWindow = new SettingsWindow(settings);
            settingsWindow.ShowDialog();
            // getting the settings file
            settings = settingsWindow.Settings;
        }

        private void CutSliderXValueChange(object sender, RoutedPropertyChangedEventArgs<double> e) {
            // not yet initialized check
            if (cutSliderX == null || cutSliderY == null) { return; }
            // updating the cutArea location
            cutArea.Location = new Point(cutSliderX.Value, cutSliderY.Value);
            // updating the cut border's position
            RepositionCutControl();
        }

        private void CutSliderYValueChange(object sender, RoutedPropertyChangedEventArgs<double> e) {
            // not yet initialized check
            if (cutSliderX == null || cutSliderY == null) { return; }
            // updating the cutArea location
            cutArea.Location = new Point(cutSliderX.Value, cutSliderY.Value);
            // updating the cut border's position
            RepositionCutControl();
        }

        private void RepositionCutControl() {
            // calculating sizes
            double left = cutArea.Left / bitmapImage.WidthInches() * preview.ActualWidth;
            double top = cutArea.Top / bitmapImage.HeightInches() * preview.ActualHeight;
            // setting the margin
            cutBorder.Margin = new Thickness(left, top, 0, 0);
        }

        /// <summary>
        /// Resizes the cut control.
        /// </summary>
        /// <param name="width">The requested width [inches]</param>
        /// <param name="height">The requested height [inches]</param>
        private void ResizeCutControl() {
            // range constraints
            if (cutArea.Width < MIN_CUT_WIDTH) { cutArea.Width = MIN_CUT_WIDTH; }
            if (cutArea.Height < MIN_CUT_HEIGHT) { cutArea.Height = MIN_CUT_HEIGHT; }
            if (cutArea.Width > bitmapImage.WidthInches()) { cutArea.Width = bitmapImage.WidthInches(); }
            if (cutArea.Height > bitmapImage.HeightInches()) { cutArea.Height = bitmapImage.HeightInches(); }
            // calculating the cut size in pixels
            cutBorder.Width = cutArea.Width / bitmapImage.WidthInches() * preview.ActualWidth;
            cutBorder.Height = cutArea.Height / bitmapImage.HeightInches() * preview.ActualHeight;
            cutBorder.UpdateLayout();
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
            // setting inputs
            cutWidthInput.Text = bitmapImage.WidthInches().ToString("0.##");
            cutHeightInput.Text = bitmapImage.HeightInches().ToString("0.##");
            stepHeightInput.Text = string.Empty;
            // resetting sliders
            ResetSliders();
            // setting the cut area
            cutArea.Location = new Point(0, 0);
            cutArea.Size = new Size(bitmapImage.WidthInches(), bitmapImage.HeightInches());
            // setting the sliders maximum...
            SetSliderMaximums();
            // updating the cut border
            RepositionCutControl();
            ResizeCutControl();
        }

        private void UpdateUiOnResize() {
            // null check
            if (bitmapImage == null) { return; }
            // resizing the cut border
            ResizeCutControl();
            // repositioning the cut border
            RepositionCutControl();
        }

        private void SetImageInfo() {
            imageInfo.Text = $"Pixel resolution (width x height)\n" +
                $" {(int)bitmapImage.PixelWidth} x {(int)bitmapImage.PixelHeight}\n" +
                $"Pixel density (horizontal x vertical) [DPI]\n" +
                $"{bitmapImage.DpiX.ToString("0.##")} x {bitmapImage.DpiY.ToString("0.##")}\n" +
                $"Image size (width x height) [inches]\n" +
                $"{bitmapImage.WidthInches().ToString("0.##")} x {bitmapImage.HeightInches().ToString("0.##")}";
        }

        /// <summary>
        /// Sets the maximum values of the sliders according to the size difference of the image and the cur border.
        /// Sliders are in inches!
        /// </summary>
        private void SetSliderMaximums() {
            // setting the sliders maximum...
            cutSliderX.Maximum = bitmapImage.WidthInches() - cutArea.Width;
            cutSliderY.Maximum = bitmapImage.HeightInches() - cutArea.Height;
            cutSliderX.UpdateLayout();
            cutSliderY.UpdateLayout();
        }

        private void ResetSliders() {
            // setting the sliders to zero
            cutSliderX.Value = 0;
            cutSliderY.Value = 0;
            cutSliderX.UpdateLayout();
            cutSliderY.UpdateLayout();
        }

        /// <summary>
        /// Saves the selected region to a PDF file.
        /// </summary>
        private void SavePdf(BitmapImage image) {
            // creating a save file dialog, setting filter, opening
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PDF files (*.pdf) | *.pdf";
            bool? result = dialog.ShowDialog();
            // no file is selected, returning
            if (result != true) { return; }
            // getting the filename
            string path = dialog.FileName;
            // saving the file
            IoHandler.SaveToPdf(image, path, bitmapImage.DpiX, bitmapImage.DpiY);
        }

    }
}
