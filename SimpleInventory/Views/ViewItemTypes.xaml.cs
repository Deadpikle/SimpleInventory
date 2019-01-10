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
    /// Interaction logic for ViewItemTypes.xaml
    /// </summary>
    public partial class ViewItemTypes : UserControl
    {
        public ViewItemTypes()
        {
            InitializeComponent();
        }

        private void DeleteItemType_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete this item category? Any " +
                "items that belong to this category will be moved to the default category.", "Delete Item Category", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                (DataContext as ViewItemTypesViewModel)?.DeleteItem(ItemsGrid.SelectedValue as ItemType);
            }
        }
    }
}
