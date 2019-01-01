using Microsoft.Win32;
using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public GenerateBarcodesViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            NumberOfPages = 1;
            PaperSizes = new List<string> { "A4", "Letter" };
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

        private void UpdateBarcodeOutputAmount()
        {
            var generator = new BarcodePDFGenerator();
            generator.PageSize = GetPaperSize();
            generator.BarcodeType = BarcodeLib.TYPE.CODE128;
            generator.IsDryRun = true;
            NumberOfBarcodesOutput = generator.GenerateBarcodes("");
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
            saveFileDialog.FileName = "Inventory-Barcodes-" + DateTime.Now.ToString("yyyy-M-d-H-mm-ss");
            if (saveFileDialog.ShowDialog() == true)
            {
                var generator = new BarcodePDFGenerator();
                generator.PageSize = GetPaperSize();
                generator.BarcodeType = BarcodeLib.TYPE.CODE128;
                generator.GenerateBarcodes(saveFileDialog.FileName);
            }
        }
    }
}
