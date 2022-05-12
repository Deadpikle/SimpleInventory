using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Media;
using System.Windows.Input;
using System.Windows.Media;

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
        private Brush _itemPurchaseStatusBrush;
        private string _otherPaidAmount = "";

        private int _amountInventoryChanged;

        private SoundPlayer _successSoundPlayer;
        private SoundPlayer _failureSoundPlayer;

        private string _quantityErrorMessage;

        public ScanItemsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _currencies = Currency.LoadCurrencies();
            _currencyIDToIndex = new Dictionary<int, int>();
            _amountInventoryChanged = 0;
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
            ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Black);
            _failureSoundPlayer = new SoundPlayer("Sounds/failure-tbone.wav");
            _successSoundPlayer = new SoundPlayer("Sounds/success.wav");
        }

        #region Properties

        public List<Currency> Currencies
        {
            get { return _currencies; }
        }

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
                NotifyPropertyChanged(nameof(CanFinalize));
            }
        }

        public int SelectedPaidCurrencyIndex
        {
            get { return _selectedPaidCurrencyIndex; }
            set
            {
                _selectedPaidCurrencyIndex = value;
                NotifyPropertyChanged();
                UpdatePurchaseInfoCurrencies();
                UpdateChange();
                NotifyPropertyChanged(nameof(CanFinalize));
            }
        }

        public int SelectedChangeCurrencyIndex
        {
            get { return _selectedChangeCurrencyIndex; }
            set
            {
                _selectedChangeCurrencyIndex = value;
                NotifyPropertyChanged();
                UpdatePurchaseInfoCurrencies();
                UpdateChange();
                NotifyPropertyChanged(nameof(CanFinalize));
            }
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                NotifyPropertyChanged();
                if (PurchaseInfo != null)
                {
                    PurchaseInfo.QuantitySold = value;
                }
                if (value <= 0)
                {
                    Quantity = 1; // you can't buy 0 items
                }
                else if (!_hasPaidAmountChangedForCurrentItem)
                {
                    // assume that the user has paid in full
                    _isChangingPaidFromQuantity = true;
                    PaidAmount = (PurchasedItem.Cost * _quantity).ToString();
                    _isChangingPaidFromQuantity = false;
                }
                else
                {
                    UpdateChange();
                }
                UpdatePurchaseInfoCurrencies();
                NotifyPropertyChanged(nameof(CanFinalize));
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
                UpdateChange();
                NotifyPropertyChanged(nameof(CanFinalize));
            }
        }

        public string OtherPaidAmount
        {
            get => _otherPaidAmount;
            set
            {
                _otherPaidAmount = value;
                NotifyPropertyChanged();
            }
        }

        private void UpdateChange()
        {
            if (PurchaseInfo != null)
            {
                UpdatePurchaseInfoCurrencies();
                var changeCurrency = PurchaseInfo.ChangeCurrency;
                var paidCurrency = PurchaseInfo.PaidCurrency;
                var paidAsDecimal = 0m;
                if (!decimal.TryParse(PaidAmount, out paidAsDecimal))
                {
                    paidAsDecimal = 0;
                }
                if (paidCurrency.ID != PurchaseInfo.CostCurrency.ID)
                {
                    // convert to dollar, then convert to currency
                    paidAsDecimal /= paidCurrency.ConversionRateToUSD;
                    paidAsDecimal *= PurchaseInfo.CostCurrency.ConversionRateToUSD;
                }
                var amountNeededToPay = Math.Round(PurchaseInfo.TotalCost, 2);
                var changeNumber = paidAsDecimal - amountNeededToPay;
                // if the amount paid doesn't equal the quantity, the user needs some change!
                if (paidAsDecimal != amountNeededToPay || paidCurrency.ID != PurchasedItem.CostCurrency.ID)
                {
                    if (PurchaseInfo.CostCurrency.ID != changeCurrency.ID)
                    {
                        changeNumber /= PurchaseInfo.CostCurrency.ConversionRateToUSD;
                        changeNumber *= changeCurrency.ConversionRateToUSD;
                    }
                    if (changeNumber >= 0)
                    {
                        PurchaseInfo.Change = Math.Round(changeNumber, 2);
                        ChangeNeeded = string.Format("{0:n} {1}", PurchaseInfo.Change, changeCurrency.Symbol);
                    }
                    else
                    {
                        PurchaseInfo.Change = 0.0m;
                        ChangeNeeded = "N/A";
                    }
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

        public string QuantityErrorMessage
        {
            get { return _quantityErrorMessage; }
            set { _quantityErrorMessage = value; NotifyPropertyChanged(); }
        }

        public bool CanFinalize
        {
            get
            {
                if (PurchaseInfo != null && SelectedPaidCurrencyIndex >= 0 && SelectedPaidCurrencyIndex < _currencies.Count)
                {
                    var paidAsDecimal = 0m;
                    if (!decimal.TryParse(PaidAmount, out paidAsDecimal))
                    {
                        paidAsDecimal = 0m;
                    }
                    var amountNeededToPay = PurchaseInfo.TotalCost;
                    var paidCurrency = _currencies[SelectedPaidCurrencyIndex];
                    var currency = PurchaseInfo.CostCurrency;
                    if (paidCurrency.ID != currency.ID)
                    {
                        // we want to put things in the change currency's amount
                        // convert to dollar, then convert to currency
                        paidAsDecimal /= paidCurrency.ConversionRateToUSD; // convert to USD
                        paidAsDecimal *= currency.ConversionRateToUSD; // convert to other amount
                    }

                    return paidAsDecimal >= amountNeededToPay;
                }
                return false;
            }
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
            if (!string.IsNullOrWhiteSpace(BarcodeNumber))
            {
                var item = InventoryItem.LoadItemByBarcode(BarcodeNumber);
                if (item != null)
                {
                    if (item.Quantity <= 0)
                    {
                        ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Red);
                        ItemPurchaseStatusMessage = "There are no items left to purchase for this item! Barcode: " + BarcodeNumber;
                        PurchaseInfoIsVisible = false;
                        // play failure sound
                        _failureSoundPlayer.Play();
                    }
                    else
                    {
                        _hasPaidAmountChangedForCurrentItem = false;
                        ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Green);
                        ItemPurchaseStatusMessage = "Item successfully found and purchased! Barcode: " + BarcodeNumber;
                        PurchasedItem = item;
                        // create purchase data object and save to the db
                        var purchaseData = new ItemSoldInfo();
                        purchaseData.DateTimeSold = DateTime.Now;
                        DateTimePurchased = purchaseData.DateTimeSold.ToString("dddd, d MMMM 'at' h:mm:ss tt");
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
                        // decrease quantity by 1
                        _amountInventoryChanged = 1;
                        item.AdjustQuantityByAmount(-_amountInventoryChanged);
                        // show info to the user for possible future editing
                        ChangeNeeded = "0"; // TODO: update if paid updated -- might want to bind to a different property for set {} updates
                        SelectedChangeCurrencyIndex = _currencyIDToIndex[purchaseData.ChangeCurrency.ID];
                        SelectedPaidCurrencyIndex = _currencyIDToIndex[purchaseData.PaidCurrency.ID];
                        Quantity = 1;
                        PurchaseInfoIsVisible = true;
                        // play success sound
                        _successSoundPlayer.Play();
                        UpdatePurchaseInfoCurrencies();
                    }
                }
                else
                {
                    ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Red);
                    ItemPurchaseStatusMessage = "Item not found! Barcode: " + BarcodeNumber;
                    PurchaseInfoIsVisible = false;
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
            if (PurchasedItem != null && PurchaseInfo != null)
            {
                PurchaseInfo.Delete();
                PurchasedItem.AdjustQuantityByAmount(PurchaseInfo.QuantitySold);
                PurchaseInfoIsVisible = false;
                BarcodeNumber = "";
                ItemPurchaseStatusMessage = "";
            }
        }

        public ICommand SavePurchaseUpdates
        {
            get { return new RelayCommand(SavePurchaseUpdatesToDB); }
        }

        private void UpdatePurchaseInfoCurrencies()
        {
            if (PurchaseInfo != null)
            {
                if (SelectedChangeCurrencyIndex >= 0 && SelectedChangeCurrencyIndex < _currencies.Count)
                {
                    PurchaseInfo.ChangeCurrency = _currencies[SelectedChangeCurrencyIndex];
                }
                if (SelectedPaidCurrencyIndex >= 0 && SelectedPaidCurrencyIndex < _currencies.Count)
                {
                     PurchaseInfo.PaidCurrency = _currencies[SelectedPaidCurrencyIndex];
                    if (PurchasedItem != null)
                    {
                        var cost = Utilities.ConvertAmount(PurchaseInfo.TotalCost, PurchasedItem.CostCurrency, _currencies[SelectedPaidCurrencyIndex]);
                        OtherPaidAmount = string.Format("{0:n} ({1})", Math.Round(cost, 2), _currencies[SelectedPaidCurrencyIndex].Symbol);
                    }
                    else
                    {
                        OtherPaidAmount = "";
                    }
                }
            }
        }

        private void SavePurchaseUpdatesToDB()
        {
            if (PurchaseInfoIsVisible && PurchaseInfo != null)
            {
                UpdatePurchaseInfoCurrencies();
                if (Quantity > PurchasedItem.Quantity)
                {
                    // too much!! can't buy this many. :(
                    _failureSoundPlayer.Play();
                    QuantityErrorMessage = "Quantity to purchase is higher than the number of available items";
                }
                else
                {
                    QuantityErrorMessage = "";
                    PurchaseInfo.QuantitySold = Quantity;
                    var paidAsDecimal = 0m;
                    if (!decimal.TryParse(PaidAmount, out paidAsDecimal))
                    {
                        paidAsDecimal = 0;
                    }
                    PurchaseInfo.Paid = paidAsDecimal;
                    if (_amountInventoryChanged != Quantity)
                    {
                        if (_amountInventoryChanged < Quantity) // e.g. 1 -> 3 (= diff of 2; needs to be adjusted by -2)
                        {
                            PurchasedItem.AdjustQuantityByAmount(_amountInventoryChanged - Quantity);
                        }
                        else // e.g. 3 -> 1 (= diff of 2; needs to be adjusted by 2)
                        {
                            PurchasedItem.AdjustQuantityByAmount(Quantity - _amountInventoryChanged);
                        }
                        _amountInventoryChanged = Quantity;
                    }
                    PurchaseInfo.SaveUpdates();
                }
            }
        }

        #endregion
    }
}
