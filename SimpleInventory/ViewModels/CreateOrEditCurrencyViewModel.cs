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
    class CreateOrEditCurrencyViewModel : BaseViewModel
    {
        private string _screenTitle;
        private bool _isDefaultCurrencyCheckboxEnabled;

        private ICreatedEditedCurrency _createdEditedItemType;
        private Currency _itemBeingEdited;

        private string _name;
        private string _abbreviation;
        private string _symbol;
        private decimal _conversionRate;
        private bool _isDefault;

        private bool _isEditingUSD;

        public CreateOrEditCurrencyViewModel(IChangeViewModel viewModelChanger, ICreatedEditedCurrency createdCurrency) : base(viewModelChanger)
        {
            ScreenTitle = "Add Currency";
            IsDefaultCurrencyCheckboxEnabled = true;
            _itemBeingEdited = null;
            _createdEditedItemType = createdCurrency;
            Name = "";
            Abbreviation = "";
            Symbol = "";
            ConversionRateToUSD = 0.0m;
            IsDefault = false;
            IsEditingUSD = false;
        }

        public CreateOrEditCurrencyViewModel(IChangeViewModel viewModelChanger, ICreatedEditedCurrency createdCurrency, 
            Currency currency) : base(viewModelChanger)
        {
            ScreenTitle = "Edit Currency";
            IsDefaultCurrencyCheckboxEnabled = !currency.IsDefaultCurrency;
            _itemBeingEdited = currency;
            _createdEditedItemType = createdCurrency;
            Name = currency.Name;
            Abbreviation = currency.Abbreviation;
            Symbol = currency.Symbol;
            ConversionRateToUSD = currency.ConversionRateToUSD;
            IsDefault = currency.IsDefaultCurrency;
            IsEditingUSD = currency.Abbreviation == "USD";
        }

        public string ScreenTitle
        {
            get { return _screenTitle; }
            set { _screenTitle = value; NotifyPropertyChanged(); }
        }

        public bool IsDefaultCurrencyCheckboxEnabled
        {
            get { return _isDefaultCurrencyCheckboxEnabled; }
            set { _isDefaultCurrencyCheckboxEnabled = value; NotifyPropertyChanged(); }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
        }

        public string Abbreviation
        {
            get { return _abbreviation; }
            set { _abbreviation = value; NotifyPropertyChanged(); }
        }

        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; NotifyPropertyChanged(); }
        }

        public decimal ConversionRateToUSD
        {
            get { return _conversionRate; }
            set { _conversionRate = value; NotifyPropertyChanged(); }
        }

        public bool IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value; NotifyPropertyChanged(); }
        }

        public bool IsEditingUSD
        {
            get { return _isEditingUSD; }
            set { _isEditingUSD = value; NotifyPropertyChanged(); NotifyPropertyChanged(nameof(IsNotEditingUSD)); }
        }

        public bool IsNotEditingUSD
        {
            get { return !IsEditingUSD; }
        }

        public ICommand ReturnToManageCurrencies
        {
            get { return new RelayCommand(PopScreen); }
        }

        private void PopScreen()
        {
            PopViewModel();
        }

        public ICommand SaveCurrency
        {
            get { return new RelayCommand(SaveData); }
        }

        private void SaveData()
        {
            var currency = _itemBeingEdited ?? new Currency();
            currency.Name = Name;
            if (!IsEditingUSD)
            {
                currency.Abbreviation = Abbreviation;
                currency.ConversionRateToUSD = ConversionRateToUSD;
                currency.Symbol = Symbol;
            }
            currency.IsDefaultCurrency = IsDefault;
            if (_itemBeingEdited != null)
            {
                currency.ID = _itemBeingEdited.ID;
                currency.Save();
            }
            else
            {
                currency.Create();
            }
            _createdEditedItemType?.CreatedEditedCurrency(currency, _itemBeingEdited == null);
            PopViewModel();
        }
    }
}
