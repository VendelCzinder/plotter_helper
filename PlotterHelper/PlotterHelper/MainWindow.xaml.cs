using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlotterHelper {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void SelectImage(object sender, RoutedEventArgs e) {
            // creating a file open dialog, setting filter, opening
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files(*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.svg) | " +
                         "*.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.svg";
            bool? result = dialog.ShowDialog();
            // no file is selected, returning
            if (result != true) { return; }
            // getting the filename
            var path = dialog.FileName;
            // loading the file
            var bitmap = ImageHandler.LoadImage(path);
            preview.Source = bitmap;
        }



        private void LoadImage(object sender, RoutedEventArgs e) {

        }

        private void SavePdf(object sender, RoutedEventArgs e) {

        }
    }
}
