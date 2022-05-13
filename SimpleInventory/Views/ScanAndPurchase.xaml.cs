using SimpleInventory.Interfaces;
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
    /// Interaction logic for ScanItems.xaml
    /// </summary>
    public partial class ScanAndPurchase : UserControl, IConfirmDelete<ItemSoldInfo>, IFinishedPurchase
    {
        public ScanAndPurchase()
        {
            InitializeComponent();
            Loaded += ScanItems_Loaded;
            DataContextChanged += ViewItemSoldInfo_DataContextChanged;
        }

        private void ViewItemSoldInfo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ScanAndPurchaseViewModel spvm)
            {
                spvm.DeleteItemSoldInfoConfirmer = this;
                spvm.FinishedPurchasedListener = this;
            }
        }

        public void ConfirmDelete(ItemSoldInfo item)
        {
            var result = MessageBox.Show("Are you sure you want to remove this item from being purchased?", 
                "Delete Purchase Info", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes && DataContext is ScanAndPurchaseViewModel)
            {
                (DataContext as ScanAndPurchaseViewModel)?.DeleteItemSoldInfo(item);
            }
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
                (DataContext as ScanAndPurchaseViewModel)?.MarkItemPurchased.Execute(null);
            }
        }

        private void CancelPurchase_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ScanAndPurchaseViewModel sapvm && sapvm.PurchasedItems.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to cancel this purchase?", "Cancel Purchase", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    sapvm.CancelPurchase.Execute(null);
                }
            }
        }

        public void FinishedPurchase(Purchase purchase)
        {
            Keyboard.Focus(BarcodeTextBox);
        }
    }
}
