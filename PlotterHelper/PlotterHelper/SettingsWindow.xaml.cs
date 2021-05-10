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
using System.Windows.Shapes;

namespace PlotterHelper
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public Settings Settings { get; private set; }

        public SettingsWindow(Settings settings)
        {
            InitializeComponent();
            Settings = settings;
        }
        private void WindowLoaded(object sender, RoutedEventArgs e) {
            // setting fields
            SettingsToGui();
        }

        private void ResetDefaultsClick(object sender, RoutedEventArgs e) {
            // creating a new instance
            Settings = new Settings();
            // saving it
            IoHandler.SaveSettings(Settings);
            // setting fields
            SettingsToGui();
        }

        private void SaveClick(object sender, RoutedEventArgs e) {
            // loading data from the textfields
            GuiToSettings();
            // saving settings
            IoHandler.SaveSettings(Settings);
            // closing the window
            Close();
        }

        private void GuiToSettings() {
            try {
                Settings.PrinterWidth = double.Parse(printerWidth.Text);
                Settings.LineLength = double.Parse(lineLength.Text);
                Settings.LineWidth = double.Parse(lineWidth.Text);
                Settings.TextTopMargin = double.Parse(textTopMargin.Text);
                Settings.TextRightMargin = double.Parse(textRightMargin.Text);
                Settings.TextSize = double.Parse(textSize.Text);
                Settings.ColorAlpha = int.Parse(colorAlpha.Text);
            }
            catch {
                MessageBox.Show("One or more field values are invalid, please correct them or reset the settings!",
                    "Error - wrong data", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void SettingsToGui() {
            printerWidth.Text = Settings.PrinterWidth.ToString("0.##");
            lineLength.Text = Settings.LineLength.ToString("0.##");
            lineWidth.Text = Settings.LineWidth.ToString("0.##");
            textTopMargin.Text = Settings.TextTopMargin.ToString("0.##");
            textRightMargin.Text = Settings.TextRightMargin.ToString("0.##");
            textSize.Text = Settings.TextSize.ToString("0.##");
            colorAlpha.Text = Settings.ColorAlpha.ToString();
        }
    }
}
