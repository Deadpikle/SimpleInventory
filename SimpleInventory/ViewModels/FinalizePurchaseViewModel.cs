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
        private ObservableCollection<ItemSoldInfo> _purchaseInfo;

        private string _customerName;
        private string _customerPhone;
        private string _customerEmail;

        private string _paidAmount;
        private int _selectedPaidCurrencyIndex;
        private string _changeNeeded;
        private int _selectedChangeCurrencyIndex;

        public FinalizePurchaseViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _currencies = Currency.LoadCurrencies();
            _currencyIDToIndex = new Dictionary<int, int>();
            for (int i = 0; i < _currencies.Count; i++)
            {
                var currency = _currencies[i];
                _currencyIDToIndex.Add(currency.ID, i);
            }
        }

        #region Properties

        public IConfirmDelete<ItemSoldInfo> DeleteItemSoldInfoConfirmer { get; set; }

        public List<Currency> Currencies
        {
            get { return _currencies; }
        }

        public ObservableCollection<ItemSoldInfo> PurchasedItems
        {
            get { return _purchaseInfo; }
            set { _purchaseInfo = value; NotifyPropertyChanged(); }
        }

        public decimal TotalPurchaseCost
        {
            get
            {
                var currency = Utilities.CurrencyForOrder(PurchasedItems.ToList());
                if (currency != null)
                {
                    decimal cost = 0.0m;
                    foreach (var item in PurchasedItems)
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
                    foreach (var item in PurchasedItems)
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
                if (PurchasedItems.Count > 0)
                {
                    var totalPurchaseCost = TotalPurchaseCost;
                    var usdCurrency = Currency.LoadUSDCurrency();
                    if (usdCurrency == null)
                    {
                        return "Error: could not find USD currency";
                    }
                    var currency = Utilities.CurrencyForOrder(PurchasedItems.ToList());
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
            }
        }

        private void UpdateChange()
        {
        }

        private void UpdatePurchaseInfoCurrencies()
        {
            //if (PurchaseInfo != null)
            //{
            //    if (SelectedChangeCurrencyIndex >= 0 && SelectedChangeCurrencyIndex < _currencies.Count)
            //    {
            //        PurchaseInfo.ChangeCurrency = _currencies[SelectedChangeCurrencyIndex];
            //    }
            //    if (SelectedPaidCurrencyIndex >= 0 && SelectedPaidCurrencyIndex < _currencies.Count)
            //    {
            //        PurchaseInfo.PaidCurrency = _currencies[SelectedPaidCurrencyIndex];
            //    }
            //}
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

        #endregion
    }
}
