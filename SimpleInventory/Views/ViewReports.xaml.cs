using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace SimpleInventory.Views
{
    /// <summary>
    /// Interaction logic for ViewReports.xaml
    /// </summary>
    public partial class ViewReports : UserControl
    {
        public ViewReports()
        {
            InitializeComponent();

            WeeklyReportDatePicker.Loaded += WeeklyReportDatePicker_Loaded;
            var culture = new CultureInfo("en-US");
            culture.DateTimeFormat = new DateTimeFormatInfo() { ShortDatePattern = "dd/MM/yyyy", ShortTimePattern = "HH:mm:ss"};
            DetailedStockStartDatePicker.SelectedDateFormat = DatePickerFormat.Short;
            DetailedStockStartDatePicker.SelectedTimeFormat = MahApps.Metro.Controls.TimePickerFormat.Short;
            DetailedStockEndDatePicker.SelectedDateFormat = DatePickerFormat.Short;
            DetailedStockEndDatePicker.SelectedTimeFormat = MahApps.Metro.Controls.TimePickerFormat.Short;
            DetailedStockStartDatePicker.Culture = culture;
            DetailedStockEndDatePicker.Culture = culture;
        }

        private void WeeklyReportDatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            WeeklyReportDatePicker.Loaded -= WeeklyReportDatePicker_Loaded;
            // add blackout dates for all days but first day of week: https://stackoverflow.com/a/15341837/3938401
            // unfortunately blackout dates aren't bindable :(
            int years = 10;
            WeeklyReportDatePicker.IsTodayHighlighted = false;
            var currentSunday = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);
            // X years ahead...
            for (int weeks = 0; weeks < (52 * years); weeks++)
            {
                WeeklyReportDatePicker.BlackoutDates.Add(new CalendarDateRange(currentSunday.AddDays(1), currentSunday.AddDays(6)));
                currentSunday = currentSunday.AddDays(7);
            }
            currentSunday = DateTime.Now.StartOfWeek(DayOfWeek.Sunday).AddDays(-7);
            // X years behind...
            for (int weeks = 0; weeks < (52 * years); weeks++)
            {
                WeeklyReportDatePicker.BlackoutDates.Add(new CalendarDateRange(currentSunday.AddDays(1), currentSunday.AddDays(6)));
                currentSunday = currentSunday.AddDays(-7);
            }
        }
    }
}
