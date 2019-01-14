using Microsoft.Win32;
using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class ViewReportsViewModel : BaseViewModel
    {
        private DateTime _selectedDailyReportDate;
        private DateTime _selectedWeeklyReportDate;
        private DateTime _selectedInventoryStockDate;
        private DaySales _currentDaySalesReport;
        private WeekSales _currentWeeklySalesReport;
        private int _selectedTabIndex;

        private List<InventoryItem> _inventoryStockReport;

        public ViewReportsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            SelectedDailyReportDate = DateTime.Now;
            SelectedWeeklyReportDate = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            SelectedInventoryStockDate = DateTime.Now;
        }

        public DateTime SelectedDailyReportDate
        {
            get { return _selectedDailyReportDate; }
            set { _selectedDailyReportDate = value; NotifyPropertyChanged(); RunDayReport(); }
        }

        public DateTime SelectedWeeklyReportDate
        {
            get { return _selectedWeeklyReportDate; }
            set { _selectedWeeklyReportDate = value; NotifyPropertyChanged(); RunWeeklyReport(); }
        }

        public DateTime SelectedInventoryStockDate
        {
            get { return _selectedInventoryStockDate; }
            set { _selectedInventoryStockDate = value; NotifyPropertyChanged(); RunStockReport(); }
        }

        public DaySales CurrentDaySalesReport
        {
            get { return _currentDaySalesReport; }
            set { _currentDaySalesReport = value; NotifyPropertyChanged(); }
        }

        public WeekSales CurrentWeeklySalesReport
        {
            get { return _currentWeeklySalesReport; }
            set { _currentWeeklySalesReport = value; NotifyPropertyChanged(); }
        }

        public List<InventoryItem> InventoryStockReport
        {
            get { return _inventoryStockReport; }
            set { _inventoryStockReport = value; NotifyPropertyChanged(); }
        }

        // for some reason, binding this property fixes an issue where if you leave the screen
        // and come back, the tab changes to the first tab on you...huh. *shrug*
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { _selectedTabIndex = value; NotifyPropertyChanged(); }
        }

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }

        private void RunDayReport()
        {
            CurrentDaySalesReport = DaySales.GenerateDataForSingleDay(SelectedDailyReportDate);
        }

        public ICommand SaveDayReportToPDF
        {
            get { return new RelayCommand(CreateAndSaveDayReport); }
        }

        private void CreateAndSaveDayReport()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF file (*.pdf)|*.pdf";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.FileName = "Daily-Inventory-Report-" + SelectedDailyReportDate.ToString("yyyy-MM-dd");
            if (saveFileDialog.ShowDialog() == true)
            {
                var generator = new ReportPDFGenerator();
                //generator.PageSize = GetPaperSize();
                //generator.BarcodeType = GetBarcodeType();
                //generator.NumberOfPages = NumberOfPages;
                generator.GeneratePDF(CurrentDaySalesReport, saveFileDialog.FileName);
            }
        }

        private void RunWeeklyReport()
        {
            CurrentWeeklySalesReport = WeekSales.GenerateDataForWeek(SelectedWeeklyReportDate);
        }

        public ICommand SaveWeeklyReportToPDF
        {
            get { return new RelayCommand(CreateAndSaveWeeklyReport); }
        }

        private void CreateAndSaveWeeklyReport()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF file (*.pdf)|*.pdf";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.FileName = "Weekly-Inventory-Report-" + SelectedWeeklyReportDate.ToString("yyyy-MM-dd");
            if (saveFileDialog.ShowDialog() == true)
            {
                var generator = new ReportPDFGenerator();
                //generator.PageSize = GetPaperSize();
                //generator.BarcodeType = GetBarcodeType();
                //generator.NumberOfPages = NumberOfPages;
                generator.GeneratePDF(CurrentWeeklySalesReport, saveFileDialog.FileName);
            }
        }

        private void RunStockReport()
        {
            InventoryStockReport = InventoryItem.GetStockByEndOfDate(SelectedInventoryStockDate);
        }

        public ICommand ViewPurchaseDetails
        {
            get { return new RelayCommand<int>(inventoryItemID => LoadViewPurchaseDetailsScreen(inventoryItemID)); }
        }

        private void LoadViewPurchaseDetailsScreen(int inventoryItemID)
        {
            PushViewModel(new ViewItemSoldInfoViewModel(ViewModelChanger, SelectedDailyReportDate, inventoryItemID));
        }

        public ICommand ViewPurchaseDetailsForWeek
        {
            get { return new RelayCommand<int>(inventoryItemID => LoadViewPurchaseDetailsScreenForWeek(inventoryItemID)); }
        }

        private void LoadViewPurchaseDetailsScreenForWeek(int inventoryItemID)
        {
            PushViewModel(new ViewItemSoldInfoViewModel(ViewModelChanger, SelectedWeeklyReportDate, SelectedWeeklyReportDate.AddDays(6), inventoryItemID));
        }
    }
}
