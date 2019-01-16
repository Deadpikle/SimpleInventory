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
    class AdjustQuantityViewModel : BaseViewModel
    {
        private InventoryItem _item;
        private int _quantity;

        public AdjustQuantityViewModel(IChangeViewModel viewModelChanger, InventoryItem item) : base(viewModelChanger)
        {
            _item = item;
            Quantity = item?.Quantity ?? 0;
        }

        public string ItemName
        {
            get { return _item?.Name; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; NotifyPropertyChanged(); }
        }

        public ICommand GoToManageItems
        {
            get { return new RelayCommand(ReturnToPreviousScreen); }
        }

        private void ReturnToPreviousScreen()
        {
            PopViewModel();
        }

        public ICommand SaveQuantityUpdates
        {
            get { return new RelayCommand(AdjustQuantity); }
        }

        private void AdjustQuantity()
        {
            var difference = Quantity - _item.Quantity;
            var userID = CurrentUser != null ? CurrentUser.ID : 1;
            QuantityAdjustment.UpdateQuantity(difference, _item.ID, userID);
            _item.AdjustQuantityByAmount(difference);
            _item.Quantity = Quantity;
            ReturnToPreviousScreen();
        }
    }
}
