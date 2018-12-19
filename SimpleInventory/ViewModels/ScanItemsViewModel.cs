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
        private Dictionary<int, int> _currencyIDToIndex;
        private string _itemPurchaseStatusMessage;
        private int _selectedPaidCurrencyIndex;
        private string _changeNeeded;
        private int _selectedChangeCurrencyIndex;
        private string _paidAmount;
        private int _quantity;
        private bool _hasPaidAmountChangedForCurrentItem;
        private string _dateTimePurchased;
        private bool _isChangingPaidFromQuantity;

        public ScanItemsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _currencies = Currency.LoadCurrencies();
            _currencyIDToIndex = new Dictionary<int, int>();
            for (int i = 0; i < _currencies.Count; i++)
            {
                var currency = _currencies[i];
                _currencyIDToIndex.Add(currency.ID, i);
            }
            PurchasedItem = null;
            PurchaseInfo = null;
            PurchaseInfoIsVisible = false;
            ItemPurchaseStatusMessage = "";
            SelectedPaidCurrencyIndex = -1;
        }

        #region Properties

        public User CurrentUser { get; set; }

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

        // TODO: until paid amount adjusted manually, update it when the quantity changes

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                NotifyPropertyChanged();
                if (value <= 0)
                {
                    Quantity = 1; // you can't buy 0 items
                }
                else if(!_hasPaidAmountChangedForCurrentItem)
                {
                    // assume that the user has paid in full
                    _isChangingPaidFromQuantity = true;
                    PaidAmount = (PurchasedItem.Cost * _quantity).ToString();
                    _isChangingPaidFromQuantity = false;
                }
            }
        }

        public string PaidAmount
        {
            get { return _paidAmount; }
            set
            {
                _paidAmount = value;
                NotifyPropertyChanged();
                if (!_isChangingPaidFromQuantity)
                {
                    _hasPaidAmountChangedForCurrentItem = true;
                }
                // update change if necessary
                UpdatePurchaseInfoCurrencies();
                var changeCurrency = PurchaseInfo.ChangeCurrency;
                var paidCurrency = PurchaseInfo.PaidCurrency;
                var paidAsDecimal = decimal.Parse(PaidAmount); // TODO: tryParse
                // if the amount paid doesn't equal the quantity, the user needs some change!
                if (paidAsDecimal != (PurchaseInfo.Cost * PurchaseInfo.QuantitySold) || paidCurrency.ID != PurchasedItem.CostCurrency.ID)
                {
                    // we want to put things in the change currency's amount
                    if (paidCurrency.ID != changeCurrency.ID)
                    {
                        // convert to dollar, then convert to currency
                        paidAsDecimal /= paidCurrency.ConversionRateToUSD;
                        paidAsDecimal *= changeCurrency.ConversionRateToUSD;
                    }
                    var amountNeededToPay = PurchaseInfo.Cost * PurchaseInfo.QuantitySold;
                    if (PurchaseInfo.CostCurrency.ID != changeCurrency.ID)
                    {
                        amountNeededToPay /= PurchaseInfo.CostCurrency.ConversionRateToUSD;
                        amountNeededToPay *= changeCurrency.ConversionRateToUSD;
                    }
                    ChangeNeeded = (paidAsDecimal - amountNeededToPay).ToString() + " " + changeCurrency.Symbol;
                    PurchaseInfo.Change = (paidAsDecimal - amountNeededToPay);
                }
                else
                {
                    ChangeNeeded = "0 " + changeCurrency.Symbol;
                    PurchaseInfo.Change = 0;
                }
            }
        }

        public string DateTimePurchased
        {
            get { return _dateTimePurchased; }
            set { _dateTimePurchased = value; NotifyPropertyChanged(); }
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
            var item = InventoryItem.LoadItemByBarcode(BarcodeNumber);
            if (item != null)
            {
                _hasPaidAmountChangedForCurrentItem = false;
                ItemPurchaseStatusMessage = "Item successfully found!";
                PurchasedItem = item;
                // create purchase data object and save to the db
                var purchaseData = new ItemSoldInfo();
                purchaseData.DateTimeSold = DateTime.Now;
                DateTimePurchased = purchaseData.DateTimeSold.ToString("dddd, d MMMM 'at' h:mm tt");
                purchaseData.InventoryItemID = item.ID;
                purchaseData.QuantitySold = 1;
                var userID = CurrentUser != null ? CurrentUser.ID : 1;
                purchaseData.SoldByUserID = userID;
                purchaseData.Cost = item.Cost;
                purchaseData.CostCurrency = item.CostCurrency;
                purchaseData.Paid = item.Cost; // default the amount the user paid to the exact cost
                purchaseData.PaidCurrency = item.CostCurrency;
                purchaseData.Change = 0; // by default, no change
                purchaseData.ChangeCurrency = item.CostCurrency;
                purchaseData.ProfitPerItem = item.ProfitPerItem;
                purchaseData.ProfitPerItemCurrency = item.ProfitPerItemCurrency;
                purchaseData.CreateNewSoldInfo();
                PurchaseInfo = purchaseData;

                // show info to the user for possible future editing
                ChangeNeeded = "0"; // TODO: update if paid updated -- might want to bind to a different property for set {} updates
                SelectedChangeCurrencyIndex = _currencyIDToIndex[purchaseData.ChangeCurrency.ID];
                SelectedPaidCurrencyIndex = _currencyIDToIndex[purchaseData.PaidCurrency.ID];
                Quantity = 1;
                PurchaseInfoIsVisible = true;
            }
            else
            {
                ItemPurchaseStatusMessage = "Item not found!";
                PurchaseInfoIsVisible = false;
            }
        }

        public ICommand SavePurchaseUpdates
        {
            get { return new RelayCommand(SavePurchaseUpdatesToDB); }
        }

        private void UpdatePurchaseInfoCurrencies()
        {
            PurchaseInfo.ChangeCurrency = _currencies[SelectedChangeCurrencyIndex];
            PurchaseInfo.PaidCurrency = _currencies[SelectedPaidCurrencyIndex];
        }

        private void SavePurchaseUpdatesToDB()
        {
            if (PurchaseInfoIsVisible && PurchaseInfo != null)
            {
                UpdatePurchaseInfoCurrencies();
                PurchaseInfo.QuantitySold = Quantity;
                PurchaseInfo.SaveUpdates();
            }
        }

        #endregion
    }
}
