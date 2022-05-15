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
    class FinalizePurchaseViewModel : BaseViewModel
    {
        private List<Currency> _currencies;
        private Dictionary<int, int> _currencyIDToIndex;
        private List<ItemSoldInfo> _purchaseInfo;

        private string _customerName;
        private string _customerPhone;
        private string _customerEmail;

        private string _paidAmount;
        private int _selectedPaidCurrencyIndex;
        private string _changeNeeded;
        private int _selectedChangeCurrencyIndex;
        private SoundPlayer _successSoundPlayer;

        public FinalizePurchaseViewModel(IChangeViewModel viewModelChanger, List<ItemSoldInfo> itemsTobeSold) : base(viewModelChanger)
        {
            PurchasedItems = itemsTobeSold;
            _currencies = Currency.LoadCurrencies();
            _currencyIDToIndex = new Dictionary<int, int>();
            for (int i = 0; i < _currencies.Count; i++)
            {
                var currency = _currencies[i];
                _currencyIDToIndex.Add(currency.ID, i);
            }
            if (PurchaseCurrency != null)
            {
                SelectedChangeCurrencyIndex = _currencyIDToIndex[PurchaseCurrency.ID];
                SelectedPaidCurrencyIndex = _currencyIDToIndex[PurchaseCurrency.ID];
            }
            PaidAmount = string.Format("{0:n}", TotalPurchaseCost);
            _successSoundPlayer = new SoundPlayer("Sounds/success.wav");
        }

        #region Properties

        public IFinishedPurchase FinishedPurchasedListener { get; set; }

        public IConfirmDelete<ItemSoldInfo> DeleteItemSoldInfoConfirmer { get; set; }

        public List<Currency> Currencies
        {
            get { return _currencies; }
        }

        public List<ItemSoldInfo> PurchasedItems
        {
            get { return _purchaseInfo; }
            set { _purchaseInfo = value; NotifyPropertyChanged(); }
        }

        private Currency PurchaseCurrency
        {
            get
            {
                var currency = Utilities.CurrencyForOrder(PurchasedItems.ToList());
                return currency ?? Currency.LoadDefaultCurrency();
            }
        }

        public decimal TotalPurchaseCost
        {
            get
            {
                var currency = PurchaseCurrency;
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
                    var defaultCurrency = Currency.LoadDefaultCurrency();
                    if (defaultCurrency == null)
                    {
                        return "Error: could not find default currency";
                    }
                    var currency = Utilities.CurrencyForOrder(PurchasedItems.ToList());
                    return currency != null
                        ? string.Format("{0:n} ({1})", totalPurchaseCost, currency.Symbol)
                        : string.Format("{0:n} ({1})", totalPurchaseCost, defaultCurrency.Symbol);
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

        public string CustomerName
        {
            get => _customerName;
            set { _customerName = value; NotifyPropertyChanged(); }
        }

        public string CustomerPhone
        {
            get => _customerPhone;
            set { _customerPhone = value; NotifyPropertyChanged(); }
        }

        public string CustomerEmail
        {
            get => _customerEmail;
            set { _customerEmail = value; NotifyPropertyChanged(); }
        }

        public bool CanFinalize
        {
            get
            {
                var paidAsDecimal = 0m;
                if (!decimal.TryParse(PaidAmount, out paidAsDecimal))
                {
                    paidAsDecimal = 0m;
                }

                var amountNeededToPay = TotalPurchaseCost;
                var paidCurrency = _currencies[SelectedPaidCurrencyIndex];
                var currency = PurchaseCurrency;
                if (paidCurrency.ID != currency.ID)
                {
                    // we want to put things in the change currency's amount
                    // convert to dollar, then convert to currency
                    paidAsDecimal /= paidCurrency.ConversionRateToUSD; // convert to USD
                    paidAsDecimal *= currency.ConversionRateToUSD; // convert to other amount
                }

                return paidAsDecimal >= amountNeededToPay;
            }
        }

        private string _otherPaidAmount = "";
        public string OtherPaidAmount
        {
            get => _otherPaidAmount;
            set
            {
                _otherPaidAmount = value;
                NotifyPropertyChanged();
            }
        }

        public string PaidAmount
        {
            get { return _paidAmount; }
            set
            {
                _paidAmount = value;
                NotifyPropertyChanged();
                //if (!_isChangingPaidFromQuantity)
                //{
                //    _hasPaidAmountChangedForCurrentItem = true;
                //}
                UpdateChange();
            }
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
                UpdateChange();
                var cost = Utilities.ConvertAmount(TotalPurchaseCost, PurchaseCurrency, _currencies[value]);
                OtherPaidAmount = string.Format("{0:n} ({1})", Math.Round(cost, 2), _currencies[value].Symbol);
            }
        }

        public int SelectedChangeCurrencyIndex
        {
            get { return _selectedChangeCurrencyIndex; }
            set
            {
                _selectedChangeCurrencyIndex = value;
                NotifyPropertyChanged();
                UpdateChange();
            }
        }

        private void UpdateChange()
        {
            var changeCurrency = _currencies[SelectedChangeCurrencyIndex];
            var paidCurrency = _currencies[SelectedPaidCurrencyIndex];
            var paidAsDecimal = 0m;
            if (!decimal.TryParse(PaidAmount, out paidAsDecimal))
            {
                paidAsDecimal = 0m;
            }
            // see if need to convert to sale currency
            var currency = PurchaseCurrency;
            if (paidCurrency.ID != currency.ID)
            {
                // convert to dollar, then convert to currency
                paidAsDecimal /= paidCurrency.ConversionRateToUSD;
                paidAsDecimal *= currency.ConversionRateToUSD;
            }
            var amountNeededToPay = TotalPurchaseCost;
            // if the amount paid doesn't equal the quantity, the user needs some change!
            if (paidAsDecimal != amountNeededToPay || paidCurrency.ID != currency.ID)
            {
                // we want to put things in the change currency's amount
                var changeNumber = paidAsDecimal - amountNeededToPay;
                if (currency.ID != changeCurrency.ID)
                {
                    changeNumber /= currency.ConversionRateToUSD;
                    changeNumber *= changeCurrency.ConversionRateToUSD;
                }
                if (changeNumber >= 0)
                {
                    ChangeNeeded = string.Format("{0:n} {1}", Math.Round(changeNumber, 2), changeCurrency.Symbol);
                }
                else
                {
                    ChangeNeeded = "N/A";
                }
                //PurchaseInfo.Change = (paidAsDecimal - amountNeededToPay);
            }
            else
            {
                ChangeNeeded = "0 " + changeCurrency.Symbol;
                //PurchaseInfo.Change = 0;
            }
        }

        #endregion

        #region ICommands

        public ICommand GoBack
        {
            get { return new RelayCommand(PopToPrevious); }
        }

        private void PopToPrevious()
        {
            PopViewModel();
        }

        public ICommand FinishPurchase
        {
            get { return new RelayCommand(FinishPurchaseAndGoBack); }
        }

        private void FinishPurchaseAndGoBack()
        {
            var changeCurrency = _currencies[SelectedChangeCurrencyIndex];
            var purchase = new Purchase()
            {
                DateTimePurchased = DateTime.Now,
                TotalCost = TotalPurchaseCost,
                CostCurrencySymbol = PurchaseCurrency.Symbol,
                CostCurrencyConversionRate = PurchaseCurrency.ConversionRateToUSD,
                ChangeCurrencySymbol = changeCurrency.Symbol,
                ChangeCurrencyConversionRate = changeCurrency.ConversionRateToUSD,
                CustomerName = CustomerName,
                CustomerEmail = CustomerEmail,
                CustomerPhone = CustomerPhone,
                UserID = CurrentUser.ID
            };
            purchase.Create();
            foreach (var item in PurchasedItems)
            {
                var purchasedItem = new PurchasedItem()
                {
                    Quantity = item.QuantitySold,
                    Name = item.ItemName,
                    Type = item.ItemType.Name,
                    Cost = item.Cost,
                    CostCurrencySymbol = item.CostCurrency.Symbol,
                    CostCurrencyConversionRate = item.CostCurrency.ConversionRateToUSD,
                    Profit = item.TotalProfit,
                    ProfitCurrencySymbol = item.ProfitPerItemCurrency.Symbol,
                    ProfitCurrencyConversionRate = item.ProfitPerItemCurrency.ConversionRateToUSD,
                    PurchaseID = purchase.ID,
                    InventoryItemID = item.InventoryItemID
                };
                purchasedItem.Create();
                var inventoryItem = InventoryItem.LoadItemByID(item.InventoryItemID);
                inventoryItem.AdjustQuantityByAmount(-item.QuantitySold);
            }
            _successSoundPlayer.Play();
            FinishedPurchasedListener?.FinishedPurchase(purchase);
            PopViewModel();
        }

        #endregion
    }
}
