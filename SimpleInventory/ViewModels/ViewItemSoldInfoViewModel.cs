using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;

namespace SimpleInventory.ViewModels
{
    class ViewItemSoldInfoViewModel : BaseViewModel
    {
        private int _inventoryItemID;
        private InventoryItem _item;

        public ViewItemSoldInfoViewModel(IChangeViewModel viewModelChanger, int inventoryItemID) : base(viewModelChanger)
        {
            _inventoryItemID = inventoryItemID;
            _item = InventoryItem.LoadItemByID(inventoryItemID);

        }

        public ICommand ReturnToReports
        {
            get { return new RelayCommand(PopToReports); }
        }

        private void PopToReports()
        {
            PopViewModel();
        }
    }
}
