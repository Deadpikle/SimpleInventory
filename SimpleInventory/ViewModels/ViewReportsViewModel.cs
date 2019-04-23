using Microsoft.Win32;
using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
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
    class ViewReportsViewModel : BaseViewModel, IDeletedItemSoldInfo
    {
        private DateTime _selectedDailyReportDate;
        private DateTime _selectedWeeklyReportDate;
        private DateTime _selectedInventoryStockDate;

        private DateTime _selectedStockReportFirstDate;
        private DateTime _selectedStockReportSecondDate;
        private List<DetailedStockReportInfo> _detailedStockReport;

        private DaySales _currentDaySalesReport;
        private WeekSales _currentWeeklySalesReport;
        
        private int _selectedTabIndex;

        private List<InventoryItem> _inventoryStockReport;
        private bool _isViewingDailyReportInfo;
        private int _lastDailyReportInfoInventoryID;

        private List<User> _users;
        private List<string> _userChoiceList;
        private int _dailyReportUserChoiceIndex;
        private int _weeklyReportUserChoiceIndex;

        private bool _canGenerateDailyPDFReports;
        private bool _canGenerateWeeklyPDFReports;

        public ViewReportsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            SelectedDailyReportDate = DateTime.Now;
            SelectedWeeklyReportDate = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            SelectedInventoryStockDate = DateTime.Now;
            SelectedStockReportFirstDate = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            SelectedStockReportSecondDate = DateTime.Now;
            _isViewingDailyReportInfo = false;
            _users = User.LoadUsers();
            _users.Sort((left, right) => left.Name.ToLower().CompareTo(right.Name.ToLower()));
            _userChoiceList = new List<string>();
            _userChoiceList.Add("All Users");
            _userChoiceList.AddRange(_users.Select(x => x.Name));
            _dailyReportUserChoiceIndex = 0;
            _weeklyReportUserChoiceIndex = 0;
            _canGenerateDailyPDFReports = true;
            _canGenerateWeeklyPDFReports = true;
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

        public DateTime SelectedStockReportFirstDate
        {
            get { return _selectedStockReportFirstDate; }
            set { _selectedStockReportFirstDate = value; NotifyPropertyChanged(); RunDetailedStockReport(); }
        }

        public DateTime SelectedStockReportSecondDate
        {
            get { return _selectedStockReportSecondDate; }
            set { _selectedStockReportSecondDate = value; NotifyPropertyChanged(); RunDetailedStockReport(); }
        }

        public List<DetailedStockReportInfo> DetailedStockReport
        {
            get { return _detailedStockReport; }
            set { _detailedStockReport = value; NotifyPropertyChanged(); }
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

        public List<string> UserChoiceList
        {
            get { return _userChoiceList; }
        }

        public int DailyReportUserChoiceIndex
        {
            get { return _dailyReportUserChoiceIndex; }
            set
            {
                _dailyReportUserChoiceIndex = value;
                CanGenerateDailyPDFReports = value == 0;
                NotifyPropertyChanged();
                RunDayReport();
            }
        }

        public int WeeklyReportUserChoiceIndex
        {
            get { return _weeklyReportUserChoiceIndex; }
            set
            {
                _weeklyReportUserChoiceIndex = value;
                CanGenerateWeeklyPDFReports = value == 0;
                NotifyPropertyChanged();
                RunWeeklyReport();
            }
        }

        public bool CanGenerateDailyPDFReports
        {
            get { return _canGenerateDailyPDFReports; }
            set { _canGenerateDailyPDFReports = value; NotifyPropertyChanged(); }
        }

        public bool CanGenerateWeeklyPDFReports
        {
            get { return _canGenerateWeeklyPDFReports; }
            set { _canGenerateWeeklyPDFReports = value; NotifyPropertyChanged(); }
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
            int userID = DailyReportUserChoiceIndex == 0 ? -1 : _users[DailyReportUserChoiceIndex - 1].ID;
            CurrentDaySalesReport = DaySales.GenerateDataForSingleDay(SelectedDailyReportDate, userID);
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
            var lastDayReportLocation = Properties.Settings.Default.LastDayReportSaveFolder;
            if (!string.IsNullOrWhiteSpace(lastDayReportLocation) && Directory.Exists(Path.GetDirectoryName(lastDayReportLocation)))
            {
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.InitialDirectory = lastDayReportLocation;
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var generator = new ReportPDFGenerator();
                    //generator.PageSize = GetPaperSize();
                    //generator.BarcodeType = GetBarcodeType();
                    //generator.NumberOfPages = NumberOfPages;
                    generator.GeneratePDF(CurrentDaySalesReport, saveFileDialog.FileName);
                    Properties.Settings.Default.LastDayReportSaveFolder = Path.GetDirectoryName(saveFileDialog.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error generating PDF! Please make sure to close the PDF with the same name" +
                        " if it is open in Adobe or other software before generating a PDF report.", "Error!", MessageBoxButton.OK);
                }
            }
        }

        private void RunWeeklyReport()
        {
            int userID = WeeklyReportUserChoiceIndex == 0 ? -1 : _users[WeeklyReportUserChoiceIndex - 1].ID;
            CurrentWeeklySalesReport = WeekSales.GenerateDataForWeek(SelectedWeeklyReportDate, userID);
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
            var lastWeekReportLocation = Properties.Settings.Default.LastWeekReportSaveFolder;
            if (!string.IsNullOrWhiteSpace(lastWeekReportLocation) && Directory.Exists(Path.GetDirectoryName(lastWeekReportLocation)))
            {
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.InitialDirectory = lastWeekReportLocation;
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var generator = new ReportPDFGenerator();
                    //generator.PageSize = GetPaperSize();
                    //generator.BarcodeType = GetBarcodeType();
                    //generator.NumberOfPages = NumberOfPages;
                    generator.GeneratePDF(CurrentWeeklySalesReport, saveFileDialog.FileName);
                    Properties.Settings.Default.LastWeekReportSaveFolder = Path.GetDirectoryName(saveFileDialog.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error generating PDF! Please make sure to close the PDF with the same name" +
                        " if it is open in Adobe or other software before generating a PDF report.", "Error!", MessageBoxButton.OK);
                }
            }
        }

        private void RunStockReport()
        {
            InventoryStockReport = InventoryItem.GetStockByDateTime(SelectedInventoryStockDate);
        }

        public ICommand ViewPurchaseDetails
        {
            get { return new RelayCommand<ReportItemSold>(reportForItem => LoadViewPurchaseDetailsScreen(reportForItem)); }
        }

        private void LoadViewPurchaseDetailsScreen(ReportItemSold reportForItem)
        {
            _isViewingDailyReportInfo = true;
            _lastDailyReportInfoInventoryID = reportForItem.InventoryItemID;
            var userToFilterBy = DailyReportUserChoiceIndex == 0 ? null : _users[DailyReportUserChoiceIndex - 1];
            PushViewModel(new ViewItemSoldInfoViewModel(ViewModelChanger, SelectedDailyReportDate, 
                reportForItem, userToFilterBy) { CurrentUser = CurrentUser, DeletedItemSoldInfoListener = this });
        }

        public ICommand ViewPurchaseDetailsForWeek
        {
            get { return new RelayCommand<ReportItemSold>(reportForItem => LoadViewPurchaseDetailsScreenForWeek(reportForItem)); }
        }

        private void LoadViewPurchaseDetailsScreenForWeek(ReportItemSold reportForItem)
        {
            _isViewingDailyReportInfo = false;
            _lastDailyReportInfoInventoryID = reportForItem.InventoryItemID;
            var userToFilterBy = WeeklyReportUserChoiceIndex == 0 ? null : _users[WeeklyReportUserChoiceIndex - 1];
            PushViewModel(new ViewItemSoldInfoViewModel(ViewModelChanger, SelectedWeeklyReportDate, 
                SelectedWeeklyReportDate.AddDays(6), reportForItem, userToFilterBy) { CurrentUser = CurrentUser, DeletedItemSoldInfoListener= this });
        }

        /// <summary>
        /// returns null if no updated report (e.g. you deleted the last item of that type that was sold)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ReportItemSold ItemSoldInfoWasDeleted(ItemSoldInfo model)
        {
            // need to rerun all reports!!
            RunDayReport();
            RunWeeklyReport();
            RunStockReport();
            ReportItemSold report = null;
            var reportList = _isViewingDailyReportInfo ? CurrentDaySalesReport.ItemsSold : CurrentWeeklySalesReport.AllItemsSold;
            foreach (ReportItemSold itemReport in reportList)
            {
                if (itemReport.InventoryItemID == _lastDailyReportInfoInventoryID)
                {
                    report = itemReport;
                    break;
                }
            }
            return report;
        }

        private void RunDetailedStockReport()
        {
            DetailedStockReport = InventoryItem.GetStockOnDates(SelectedStockReportFirstDate, SelectedStockReportSecondDate);
        }

        public ICommand ExportSoldItemStockInfoToExcel
        {
            get { return new RelayCommand(ExportSoldItemInfoToExcel); }
        }

        private void ExportSoldItemInfoToExcel()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel File (*.xlsx)|*.xlsx";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.FileName = "Stock-Info-Sold-Items-Report-" + SelectedStockReportFirstDate.ToString("yyyy-MM-dd") 
                    + "-" + SelectedStockReportSecondDate.ToString("yyyy-MM-dd");
            var lastExcelLocation = Properties.Settings.Default.LastExcelReportSaveLocation;
            if (!string.IsNullOrWhiteSpace(lastExcelLocation) && Directory.Exists(Path.GetDirectoryName(lastExcelLocation)))
            {
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.InitialDirectory = lastExcelLocation;
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var soldItemIDs = ItemSoldInfo.LoadItemIDsSoldBetweenDateAndItemUntilDate(SelectedStockReportFirstDate, SelectedStockReportSecondDate);
                    // we only want items in the excel sheet that have been sold in between the two dates
                    var soldItemIDHashSet = new HashSet<int>(soldItemIDs);
                    var itemsToExport = DetailedStockReport.Where(x => soldItemIDHashSet.Contains(x.Item.ID));
                    // export data to excel
                    var excelGenerator = new StockInfoExcelGenerator();
                    excelGenerator.ExportStockInfo(itemsToExport.ToList(), SelectedStockReportFirstDate, SelectedStockReportSecondDate, saveFileDialog.FileName);
                    Properties.Settings.Default.LastExcelReportSaveLocation = Path.GetDirectoryName(saveFileDialog.FileName);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error generating file! Please make sure to close the file with the same name" +
                        " if it is open in Excel or other software before generating a file report.", "Error!", MessageBoxButton.OK);
                }
            }
        }
    }
}
