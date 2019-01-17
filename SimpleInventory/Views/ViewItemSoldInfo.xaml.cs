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
    /// Interaction logic for ViewItemSoldInfo.xaml
    /// </summary>
    public partial class ViewItemSoldInfo : UserControl, IConfirmDelete<ItemSoldInfo>
    {
        public ViewItemSoldInfo()
        {
            InitializeComponent();
            DataContextChanged += ViewItemSoldInfo_DataContextChanged;
        }

        private void ViewItemSoldInfo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ViewItemSoldInfoViewModel)
            {
                (DataContext as ViewItemSoldInfoViewModel).DeleteItemSoldInfoConfirmer = this;
            }
        }

        public void ConfirmDelete(ItemSoldInfo item)
        {
            var result = MessageBox.Show("Are you sure you want to delete this info on an item being sold? You CANNOT undo this action, and YOU are responsible for making " +
                "sure that this action is correct!", "Delete Info", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes && DataContext is ViewItemSoldInfoViewModel)
            {
                (DataContext as ViewItemSoldInfoViewModel)?.DeleteItemSoldInfo(item);
            }
        }
    }
}
