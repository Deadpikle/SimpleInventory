using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleInventory.ViewModels
{
    // TODO: when currencies are different, the +/- sign for change isn't right
    // TODO: on selected index changed for currencies, update change
    class ScanAndPurchaseViewModel : BaseViewModel, IPurchaseInfoChanged
    {
        private string _barcodeNumber;
        private bool _purchaseInfoIsVisible;

        private List<Currency> _currencies;
        private Dictionary<int, int> _currencyIDToIndex;
        private string _itemPurchaseStatusMessage;
        private int _selectedPaidCurrencyIndex;
        private string _changeNeeded;
        private int _selectedChangeCurrencyIndex;
        private Brush _itemPurchaseStatusBrush;


        private SoundPlayer _successSoundPlayer;
        private SoundPlayer _failureSoundPlayer;

        private string _quantityErrorMessage;

        private ObservableCollection<ItemSoldInfo> _purchaseInfo;

        public ScanAndPurchaseViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _currencies = Currency.LoadCurrencies();
            _currencyIDToIndex = new Dictionary<int, int>();
            for (int i = 0; i < _currencies.Count; i++)
            {
                var currency = _currencies[i];
                _currencyIDToIndex.Add(currency.ID, i);
            }
            PurchaseInfo = new ObservableCollection<ItemSoldInfo>();
            PurchaseInfoIsVisible = false;
            ItemPurchaseStatusMessage = "";
            SelectedPaidCurrencyIndex = -1;
            ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Black);
            _failureSoundPlayer = new SoundPlayer("Sounds/failure-tbone.wav");
            _successSoundPlayer = new SoundPlayer("Sounds/success.wav");
        }

        #region Properties

        public IConfirmDelete<ItemSoldInfo> DeleteItemSoldInfoConfirmer { get; set; }

        public List<Currency> Currencies
        {
            get { return _currencies; }
        }

        public string BarcodeNumber
        {
            get { return _barcodeNumber; }
            set { _barcodeNumber = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<ItemSoldInfo> PurchaseInfo
        {
            get { return _purchaseInfo; }
            set { _purchaseInfo = value; NotifyPropertyChanged(); }
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

        public Brush ItemPurchaseStatusBrush
        {
            get { return _itemPurchaseStatusBrush; }
            set { _itemPurchaseStatusBrush = value; NotifyPropertyChanged(); }
        }

        public string ChangeNeeded
        {
            get { return _changeNeeded; }
            set
            {
                _changeNeeded = value;
                NotifyPropertyChanged();
            }
        }

        public int SelectedPaidCurrencyIndex
        {
            get { return _selectedPaidCurrencyIndex; }
            set
            {
                _selectedPaidCurrencyIndex = value;
                NotifyPropertyChanged();
                //UpdatePurchaseInfoCurrencies();
               // UpdateChange();
            }
        }

        public int SelectedChangeCurrencyIndex
        {
            get { return _selectedChangeCurrencyIndex; }
            set
            {
                _selectedChangeCurrencyIndex = value;
                NotifyPropertyChanged();
                //UpdatePurchaseInfoCurrencies();
               // UpdateChange();
            }
        }

        public string QuantityErrorMessage
        {
            get { return _quantityErrorMessage; }
            set { _quantityErrorMessage = value; NotifyPropertyChanged(); }
        }

        public decimal TotalPurchaseCost
        {
            get
            {
                var currency = Utilities.CurrencyForOrder(PurchaseInfo.ToList());
                if (currency != null)
                {
                    decimal cost = 0.0m;
                    foreach (var item in PurchaseInfo)
                    {
                        if (item.CostCurrency != null)
                        {
                            cost += item.TotalCost;
                        }
                    }
                    return cost;
                }
                else
                {
                    // convert to USD
                    var usdCurrency = Currency.LoadUSDCurrency();
                    decimal cost = 0.0m;
                    foreach (var item in PurchaseInfo)
                    {
                        if (item.CostCurrency != null)
                        {
                            cost += Utilities.ConvertAmount(item.TotalCost, item.CostCurrency, usdCurrency);
                        }
                    }
                    return cost;
                }
            }
        }

        public string TotalPurchaseCostWithCurrency
        {
            get
            {
                if (PurchaseInfo.Count > 0)
                {
                    var totalPurchaseCost = TotalPurchaseCost;
                    var usdCurrency = Currency.LoadUSDCurrency();
                    if (usdCurrency == null)
                    {
                        return "Error: could not find USD currency";
                    }
                    var currency = Utilities.CurrencyForOrder(PurchaseInfo.ToList());
                    return currency == null
                        ? totalPurchaseCost.ToString("0.00") + " (" + currency.Symbol + ")"
                        : totalPurchaseCost.ToString("0.00") + " (" + usdCurrency.Symbol + ")";
                }
                else
                {
                    return "--";
                }
            }
        }

        public int TotalItemCount
        {
            get
            {
                var count = 0;
                foreach (var item in PurchaseInfo)
                {
                    count += item.QuantitySold;
                }
                return count;
            }
        }

        public bool CanFinalize
        {
            get 
            {
                if (PurchaseInfo.Count > 0 && !TotalPurchaseCostWithCurrency.ToLower().Contains("error"))
                {
                    foreach (var item in PurchaseInfo)
                    {
                        if (item.QuantitySold > 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            } 
        }

        public bool CanCancel
        {
            get
            {
                if (PurchaseInfo.Count > 0)
                {
                    foreach (var item in PurchaseInfo)
                    {
                        if (item.QuantitySold > 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        #endregion

        public void QuantityChanged(ItemSoldInfo info, int quantity)
        {
            UpdateTotalsAndFinalizeButtons();
        }

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
            if (!string.IsNullOrWhiteSpace(BarcodeNumber))
            {
                var item = InventoryItem.LoadItemByBarcode(BarcodeNumber);
                if (item != null)
                {
                    if (item.Quantity <= 0)
                    {
                        ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Red);
                        ItemPurchaseStatusMessage = "There are no items left to purchase for this item! Barcode: " + BarcodeNumber;
                        //PurchaseInfoIsVisible = false;
                        // play failure sound
                        _failureSoundPlayer.Play();
                    }
                    else
                    {
                        ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Green);
                        ItemPurchaseStatusMessage = "Item successfully found and added! Barcode: " + BarcodeNumber;
                        // create purchase data object
                        var existingPurchaseData = PurchaseInfo.Where(x => x.InventoryItemID == item.ID).FirstOrDefault();
                        if (existingPurchaseData != null)
                        {
                            existingPurchaseData.QuantitySold += 1; // TODO: quantity check -- make sure enough items!
                        }
                        else
                        {
                            var purchaseData = new ItemSoldInfo();
                            purchaseData.DateTimeSold = DateTime.Now;
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
                            purchaseData.ItemName = item.Name;
                            purchaseData.ItemDescription = item.Description;
                            purchaseData.PurchaseInfoChanged = this;
                            PurchaseInfo.Add(purchaseData);
                            // show info to the user for possible future editing
                            ChangeNeeded = "0"; // TODO: update if paid updated -- might want to bind to a different property for set {} updates
                            SelectedChangeCurrencyIndex = _currencyIDToIndex[purchaseData.ChangeCurrency.ID];
                            SelectedPaidCurrencyIndex = _currencyIDToIndex[purchaseData.PaidCurrency.ID];
                            //PurchaseInfoIsVisible = true;
                        }
                        // play success sound
                        _successSoundPlayer.Play();
                        UpdateTotalsAndFinalizeButtons();
                    }
                }
                else
                {
                    ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Red);
                    ItemPurchaseStatusMessage = "Item not found! Barcode: " + BarcodeNumber;
                    //PurchaseInfoIsVisible = false;
                    // play failure sound
                    _failureSoundPlayer.Play();
                }
            }
            BarcodeNumber = ""; // empty the field so that something can be scanned again
            QuantityErrorMessage = "";
        }

        public ICommand CancelPurchase
        {
            get { return new RelayCommand(PerformPurchaseCancel); }
        }

        private void PerformPurchaseCancel()
        {
            BarcodeNumber = "";
            ItemPurchaseStatusMessage = "";
            PurchaseInfo = new ObservableCollection<ItemSoldInfo>();
            UpdateTotalsAndFinalizeButtons();
        }

        private void UpdateTotalsAndFinalizeButtons()
        {
            NotifyPropertyChanged(nameof(TotalPurchaseCostWithCurrency));
            NotifyPropertyChanged(nameof(TotalItemCount));
            NotifyPropertyChanged(nameof(CanFinalize));
            NotifyPropertyChanged(nameof(CanCancel));
        }

        public ICommand ConfirmDeleteItemSoldInfo
        {
            get { return new RelayCommand<ItemSoldInfo>(item => CheckBeforeDeletingItemSoldInfo(item)); }
        }

        private void CheckBeforeDeletingItemSoldInfo(ItemSoldInfo item)
        {
            DeleteItemSoldInfoConfirmer?.ConfirmDelete(item);
        }

        public void DeleteItemSoldInfo(ItemSoldInfo info)
        {
            PurchaseInfo.Remove(info);
            ItemPurchaseStatusMessage = "";
            UpdateTotalsAndFinalizeButtons();
        }

        #endregion
    }
}
