using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Printing;
using System.Windows.Forms;
namespace YesPos
{
    class PrintOptions
    {
        public int marginLeft = 10;
        public int marginRight = 10;
        public int marginTop = 10;
        public int marginBottom = 10;
        public string printer = System.Drawing.Printing.PrinterSettings.InstalledPrinters[0];
        public int paperWidth = 210;
        public int paperHeight = 297;
    }
}
