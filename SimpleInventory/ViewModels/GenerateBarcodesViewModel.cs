using Microsoft.Win32;
using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class GenerateBarcodesViewModel : BaseViewModel
    {
        private int _numberOfPages;
        private int _paperSizeSelectedIndex;
        private int _barcodeTypeSelectedIndex;

        private int _numberOfBarcodesOutput;

        private List<string> _paperSizes;
        private List<string> _barcodeTypes;

        public GenerateBarcodesViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            NumberOfPages = 1;
            PaperSizes = new List<string> { "A4", "Letter" };
            BarcodeTypes = new List<string> { "Code128", "Code39" };
        }

        public int NumberOfPages
        {
            get { return _numberOfPages; }
            set { _numberOfPages = value; NotifyPropertyChanged(); UpdateBarcodeOutputAmount(); }
        }

        public int PaperSizeSelectedIndex
        {
            get { return _paperSizeSelectedIndex; }
            set
            {
                _paperSizeSelectedIndex = value;
                NotifyPropertyChanged();
                UpdateBarcodeOutputAmount();
            }
        }

        public int BarcodeTypeSelectedIndex
        {
            get { return _barcodeTypeSelectedIndex; }
            set { _barcodeTypeSelectedIndex = value; NotifyPropertyChanged(); UpdateBarcodeOutputAmount(); }
        }

        public int NumberOfBarcodesOutput
        {
            get { return _numberOfBarcodesOutput; }
            set { _numberOfBarcodesOutput = value; NotifyPropertyChanged(); }
        }

        public List<string> PaperSizes
        {
            get { return _paperSizes; }
            set { _paperSizes = value; NotifyPropertyChanged(); }
        }

        public List<string> BarcodeTypes
        {
            get { return _barcodeTypes; }
            set { _barcodeTypes = value; NotifyPropertyChanged(); }
        }

        private PdfSharp.PageSize GetPaperSize()
        {
            if (PaperSizeSelectedIndex == 0)
            {
                return PdfSharp.PageSize.A4;
            }
            else if (PaperSizeSelectedIndex == 1)
            {
                return PdfSharp.PageSize.Letter;
            }
            return PdfSharp.PageSize.A4;
        }

        private BarcodeLib.TYPE GetBarcodeType()
        {
            if (BarcodeTypeSelectedIndex == 0)
            {
                return BarcodeLib.TYPE.CODE128;
            }
            else if (BarcodeTypeSelectedIndex == 1)
            {
                return BarcodeLib.TYPE.CODE39;
            }
            return BarcodeLib.TYPE.CODE128;
        }

        private void UpdateBarcodeOutputAmount()
        {
            if (NumberOfPages >= 0)
            {
                var generator = new BarcodePDFGenerator();
                generator.PageSize = GetPaperSize();
                generator.BarcodeType = GetBarcodeType();
                generator.NumberOfPages = 1;
                generator.IsDryRun = true;
                NumberOfBarcodesOutput = NumberOfPages * generator.GenerateBarcodes("");
            }
        }

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }

        public ICommand GenerateBarcodes
        {
            get { return new RelayCommand(GeneratePDFOfBarcodes); }
        }

        private void GeneratePDFOfBarcodes()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF file (*.pdf)|*.pdf";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.FileName = "Inventory-Barcodes-" + DateTime.Now.ToString("yyyy-MM-dd-H-mm-ss");
            if (saveFileDialog.ShowDialog() == true)
            {
                var generator = new BarcodePDFGenerator();
                generator.PageSize = GetPaperSize();
                generator.BarcodeType = GetBarcodeType();
                generator.NumberOfPages = NumberOfPages;
                try
                {
                    generator.GenerateBarcodes(saveFileDialog.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error generating PDF! Please make sure to close the PDF with the same name" +
                        " if it is open in Adobe or other software before generating a PDF report.", "Error!", MessageBoxButton.OK);
                }
            }
        }
    }
}
