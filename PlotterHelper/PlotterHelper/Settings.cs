using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotterHelper
{
    public class Settings {

        // for the cut logic
        public double PrinterWidth { get; set; } = 52; // [inches]

        // for the overlay logic
        public double LineLength { get; set; } = 0.25; // [inches]
        public double LineWidth { get; set; } = 0.02; // [inches]
        public double TextTopMargin { get; set; } = 0.1; // [inches]
        public double TextRightMargin { get; set; } = 0.02; // [inches]
        public double TextSize { get; set; } = 0.1; // [inches]
        public int ColorAlpha { get; set; } = 200; // in pixel intensity [0-255]
    }
}
