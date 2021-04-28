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

        private BitmapImage bitmap = null;

        public MainWindow() {
            InitializeComponent();
            // for the PDF generator
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private void LoadImage(object sender, RoutedEventArgs e) {
            // creating a file open dialog, setting filter, opening
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.svg) | *.jpg; *.jpeg; *.png; *.svg";
            bool? result = dialog.ShowDialog();
            // no file is selected, returning
            if (result != true) { return; }
            // getting the filename
            string path = dialog.FileName;
            // loading the file
            bitmap = ImageHandler.LoadImage(path);
            preview.Source = bitmap;
        }

        private void SavePdf(object sender, RoutedEventArgs e) {
            // creating a save file dialog, setting filter, opening
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PDF files (*.pdf) | *.pdf";
            bool? result = dialog.ShowDialog();
            // no file is selected, returning
            if (result != true) { return; }
            // getting the filename
            string path = dialog.FileName;
            // saving the file
            ImageHandler.SaveToPdf(bitmap, path);
        }
    }
}
