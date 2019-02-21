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
        private string _title;
        private InventoryItem _item;
        private int _quantity;
        private string _explanation;
        private bool _isCreating;
        private bool _wasAdjustedForStockPurchase;
        private bool _canMarkAdjustedForStockPurchase;

        private QuantityAdjustment _adjustment;

        public AdjustQuantityViewModel(IChangeViewModel viewModelChanger, InventoryItem item) : base(viewModelChanger)
        {
            _item = item;
            CanMarkAdjustedForStockPurchase = true;
            Quantity = item?.Quantity ?? 0;
            _isCreating = true;
            WasAdjustedForStockPurchase = false;
            Title = "Change Quantity";
        }

        /// <summary>
        /// To be used when updating quantity adjustment explanation
        /// </summary>
        /// <param name="viewModelChanger"></param>
        /// <param name="adjustment"></param>
        public AdjustQuantityViewModel(IChangeViewModel viewModelChanger, QuantityAdjustment adjustment) : base(viewModelChanger)
        {
            WasAdjustedForStockPurchase = adjustment.WasAdjustedForStockPurchase;
            Quantity = adjustment.AmountChanged;
            _isCreating = false;
            _adjustment = adjustment;
            Explanation = adjustment.Explanation;
            Title = "Edit";
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; NotifyPropertyChanged(); }
        }

        public string ItemName
        {
            get { return _item?.Name; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; NotifyPropertyChanged(); CanMarkAdjustedForStockPurchase = _quantity >= 0; }
        }

        public string Explanation
        {
            get { return _explanation; }
            set { _explanation = value; NotifyPropertyChanged(); }
        }

        public bool WasAdjustedForStockPurchase
        {
            get { return _wasAdjustedForStockPurchase; }
            set { _wasAdjustedForStockPurchase = value; NotifyPropertyChanged(); }
        }

        public bool CanMarkAdjustedForStockPurchase
        {
            get { return _canMarkAdjustedForStockPurchase; }
            set { _canMarkAdjustedForStockPurchase = value; NotifyPropertyChanged(); }
        }

        public bool IsCreating
        {
            get { return _isCreating; }
        }

        public bool IsEditing
        {
            get { return !_isCreating; }
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
            if (IsCreating)
            {
                var difference = Quantity - _item.Quantity;
                var userID = CurrentUser != null ? CurrentUser.ID : 1;
                var wasAdjusted = CanMarkAdjustedForStockPurchase ? WasAdjustedForStockPurchase : false;
                QuantityAdjustment.UpdateQuantity(difference, _item.ID, userID, Explanation, wasAdjusted);
                _item.AdjustQuantityByAmount(difference);
                _item.Quantity = Quantity;
                ReturnToPreviousScreen();
            }
            else
            {
                _adjustment.Explanation = Explanation;
                _adjustment.WasAdjustedForStockPurchase = WasAdjustedForStockPurchase;
                _adjustment.SaveUpdates();
                ReturnToPreviousScreen();
            }
        }
    }
}
