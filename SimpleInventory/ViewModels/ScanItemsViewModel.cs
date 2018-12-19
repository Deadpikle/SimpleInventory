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
    class ScanItemsViewModel : BaseViewModel
    {
        private string _barcodeNumber;
        private InventoryItem _purchasedItem;
        private ItemSoldInfo _itemSoldInfo;
        private bool _purchaseInfoIsVisible;

        private List<Currency> _currencies;
        private string _itemPurchaseStatusMessage;
        private int _selectedPaidCurrencyIndex;
        private string _changeNeeded;
        private int _selectedChangeCurrencyIndex;

        public ScanItemsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _currencies = Currency.LoadCurrencies();
            PurchasedItem = null;
            PurchaseInfo = null;
            PurchaseInfoIsVisible = false;
            ItemPurchaseStatusMessage = "";
            SelectedPaidCurrencyIndex = -1;
        }

        #region Properties

        public string BarcodeNumber
        {
            get { return _barcodeNumber; }
            set { _barcodeNumber = value; NotifyPropertyChanged(); }
        }

        public InventoryItem PurchasedItem
        {
            get { return _purchasedItem; }
            set { _purchasedItem = value; NotifyPropertyChanged(); }
        }

        public ItemSoldInfo PurchaseInfo
        {
            get { return _itemSoldInfo; }
            set { _itemSoldInfo = value; NotifyPropertyChanged(); }
        }

        public bool PurchaseInfoIsVisible
        {
            get { return _purchaseInfoIsVisible; }
            set { _purchaseInfoIsVisible = value; NotifyPropertyChanged(); }
        }

        public string ItemPurchaseStatusMessage
        {
            get { return _itemPurchaseStatusMessage; }
            set { _itemPurchaseStatusMessage = value; NotifyPropertyChanged(); }
        }

        public int SelectedPaidCurrencyIndex
        {
            get { return _selectedPaidCurrencyIndex; }
            set { _selectedPaidCurrencyIndex = value; NotifyPropertyChanged(); }
        }

        public string ChangeNeeded
        {
            get { return _changeNeeded; }
            set { _changeNeeded = value; NotifyPropertyChanged(); }
        }

        public int SelectedChangeCurrencyIndex
        {
            get { return _selectedChangeCurrencyIndex; }
            set { _selectedChangeCurrencyIndex = value; NotifyPropertyChanged(); }
        }

        #endregion

        #region ICommands

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }

        public ICommand MarkItemPurchased
        {
            get { return new RelayCommand(ItemWasPurchased); }
        }

        private void ItemWasPurchased()
        {

        }

        public ICommand SavePurchaseUpdates
        {
            get { return new RelayCommand(SavePurchaseUpdatesToDB); }
        }

        private void SavePurchaseUpdatesToDB()
        {

        }

        #endregion
    }
}
