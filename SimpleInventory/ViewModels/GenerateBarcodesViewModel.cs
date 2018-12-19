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
    // TODO: A4 paper size vs 8.5x11 (defaults to A4)
    class GenerateBarcodesViewModel : BaseViewModel
    {
        public GenerateBarcodesViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {

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
                generator.GenerateBarcodes(saveFileDialog.FileName);
            }
        }
    }
}
