using SimpleInventory.Models;
using SimpleInventory.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleInventory.Views
{
    /// <summary>
    /// Interaction logic for ViewCurrencies.xaml
    /// </summary>
    public partial class ViewCurrencies : UserControl
    {
        public ViewCurrencies()
        {
            InitializeComponent();
        }

        private void DeleteCurrency_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete this currency?", "Delete Currency", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                (DataContext as ViewCurrenciesViewModel)?.DeleteItem(ItemsGrid.SelectedValue as Currency);
            }
        }
    }
}
