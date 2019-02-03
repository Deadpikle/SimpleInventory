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
    /// Interaction logic for ScanItems.xaml
    /// </summary>
    public partial class ScanItems : UserControl
    {
        public ScanItems()
        {
            InitializeComponent();
            Loaded += ScanItems_Loaded;
        }

        private void ScanItems_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(BarcodeTextBox);
            Loaded -= ScanItems_Loaded;
        }

        private void BarcodeScanTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (DataContext as ScanItemsViewModel)?.MarkItemPurchased.Execute(null);
            }
        }

        private void CancelPurchase_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to cancel this purchase?", "Cancel Purchase", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                (DataContext as ScanItemsViewModel)?.CancelPurchase.Execute(null);
            }
        }
    }
}
