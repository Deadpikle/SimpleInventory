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
    class ScanAndPurchaseViewModel : BaseViewModel, IPurchaseInfoChanged, IFinishedPurchase
    {
        private string _barcodeNumber;

        private List<Currency> _currencies;
        private Dictionary<int, int> _currencyIDToIndex;
        private string _itemPurchaseStatusMessage;
        private Brush _itemPurchaseStatusBrush;


        private SoundPlayer _successSoundPlayer;
        private SoundPlayer _failureSoundPlayer;

        private string _purchaseErrorMessage;

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
            PurchasedItems = new ObservableCollection<ItemSoldInfo>();
            ItemPurchaseStatusMessage = "";
            ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Black);
            _failureSoundPlayer = new SoundPlayer("Sounds/failure-tbone.wav");
            _successSoundPlayer = new SoundPlayer("Sounds/success.wav");
        }

        #region Properties
        
        public IFinishedPurchase FinishedPurchasedListener { get; set; }

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

        public ObservableCollection<ItemSoldInfo> PurchasedItems
        {
            get { return _purchaseInfo; }
            set { _purchaseInfo = value; NotifyPropertyChanged(); }
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

        public decimal TotalPurchaseCost
        {
            get
            {
                var currency = Utilities.CurrencyForOrder(PurchasedItems.ToList());
                if (currency == null)
                {
                    currency = Currency.LoadDefaultCurrency();
                }
                if (currency != null)
                {
                    decimal cost = 0.0m;
                    foreach (var item in PurchasedItems)
                    {
                        if (item.CostCurrency != null)
                        {
                            cost += Utilities.ConvertAmount(item.TotalCost, item.CostCurrency, currency);
                        }
                    }
                    return Math.Round(cost, 2);
                }
                return 0.0m;
            }
        }

        public string TotalPurchaseCostWithCurrency
        {
            get
            {
                if (PurchasedItems.Count > 0)
                {
                    var totalPurchaseCost = TotalPurchaseCost;
                    var currency = Utilities.CurrencyForOrder(PurchasedItems.ToList());
                    var defaultCurrency = Currency.LoadDefaultCurrency();
                    if (defaultCurrency == null)
                    {
                        return "Error: could not find default currency";
                    }
                    return currency != null
                        ? string.Format("{0:n}", totalPurchaseCost) + " (" + currency.Symbol + ")"
                        : string.Format("{0:n}", totalPurchaseCost) + " (" + defaultCurrency.Symbol + ")";
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
                foreach (var item in PurchasedItems)
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
                if (PurchasedItems.Count > 0 && !PurchaseErrorMessageIsVisible && !TotalPurchaseCostWithCurrency.ToLower().Contains("error"))
                {
                    foreach (var item in PurchasedItems)
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
                if (PurchasedItems.Count > 0)
                {
                    foreach (var item in PurchasedItems)
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

        public string PurchaseErrorMessage
        {
            get { return _purchaseErrorMessage; }
            set
            {
                _purchaseErrorMessage = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(PurchaseErrorMessageIsVisible));
            }
        }

        public bool PurchaseErrorMessageIsVisible
        {
            get => !string.IsNullOrWhiteSpace(PurchaseErrorMessage);
        }

        #endregion

        public void QuantityChanged(ItemSoldInfo info, int quantity)
        {
            UpdateTotalsAndFinalizeButtons();
        }

        private void CheckForQuantityErrors()
        {
            PurchaseErrorMessage = "";
            foreach (var item in PurchasedItems)
            {
                if (item.QuantitySold > item.MaxQuantity)
                {
                    PurchaseErrorMessage = "Error: Number of items sold for " + item.ItemName + " exceeds maximum of " + item.MaxQuantity;
                    break;
                }
            }
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
                        _failureSoundPlayer.Play();
                    }
                    else
                    {
                        ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Green);
                        ItemPurchaseStatusMessage = "Item successfully found and added! Barcode: " + BarcodeNumber;
                        // create purchase data object
                        var existingPurchaseData = PurchasedItems.Where(x => x.InventoryItemID == item.ID).FirstOrDefault();
                        if (existingPurchaseData != null)
                        {
                            if (existingPurchaseData.QuantitySold + 1 <= item.Quantity)
                            {
                                existingPurchaseData.QuantitySold += 1;
                                _successSoundPlayer.Play();
                            }
                            else
                            {
                                ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Red);
                                ItemPurchaseStatusMessage = "The maximum number of items left to purchase for this item has already been reached!";
                                _failureSoundPlayer.Play();
                            }
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
                            purchaseData.MaxQuantity = item.Quantity;
                            purchaseData.ItemType = item.Type;
                            PurchasedItems.Add(purchaseData);
                            _successSoundPlayer.Play();
                        }
                        UpdateTotalsAndFinalizeButtons();
                    }
                }
                else
                {
                    ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Red);
                    ItemPurchaseStatusMessage = "Item not found! Barcode: " + BarcodeNumber;
                    _failureSoundPlayer.Play();
                }
            }
            BarcodeNumber = ""; // empty the field so that something can be scanned again
        }

        public ICommand CancelPurchase
        {
            get { return new RelayCommand(PerformPurchaseCancel); }
        }

        private void PerformPurchaseCancel()
        {
            BarcodeNumber = "";
            ItemPurchaseStatusMessage = "";
            PurchasedItems = new ObservableCollection<ItemSoldInfo>();
            UpdateTotalsAndFinalizeButtons();
        }

        private void UpdateTotalsAndFinalizeButtons()
        {
            CheckForQuantityErrors();
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
            PurchasedItems.Remove(info);
            ItemPurchaseStatusMessage = "";
            UpdateTotalsAndFinalizeButtons();
        }

        public ICommand FinalizePurchase
        {
            get { return new RelayCommand(MoveToFinalizePurchaseScreen); }
        }

        private void MoveToFinalizePurchaseScreen()
        {
            PushViewModel(new FinalizePurchaseViewModel(ViewModelChanger, PurchasedItems.ToList())
            {
                FinishedPurchasedListener = this,
                CurrentUser = CurrentUser
            });
        }

        public void FinishedPurchase(Purchase purchase)
        {
            PurchasedItems.Clear();
            ItemPurchaseStatusBrush = new SolidColorBrush(Colors.Green);
            ItemPurchaseStatusMessage = "Purchase successfully completed!";
            BarcodeNumber = "";
            UpdateTotalsAndFinalizeButtons();
            FinishedPurchasedListener?.FinishedPurchase(purchase);
        }

        #endregion
    }
}
