using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class ViewReportsViewModel : BaseViewModel
    {
        private DateTime _selectedReportDate;
        private DaySales _currentDaySalesReport;

        public ViewReportsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            SelectedReportDate = DateTime.Now;
        }

        public DateTime SelectedReportDate
        {
            get { return _selectedReportDate; }
            set { _selectedReportDate = value; NotifyPropertyChanged(); RunDayReport(); }
        }

        public DaySales CurrentDaySalesReport
        {
            get { return _currentDaySalesReport; }
            set { _currentDaySalesReport = value; NotifyPropertyChanged(); }
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
            CurrentDaySalesReport = DaySales.GenerateDataForSingleDay(SelectedReportDate);
        }
    }
}
