using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class ViewCurrenciesViewModel : BaseViewModel, ICreatedEditedCurrency
    {
        private ObservableCollection<Currency> _currencies;
        private int _selectedCurrencyIndex;
        private Currency _selectedItem;
        private bool _isItemSelected;

        public ViewCurrenciesViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            Currencies = new ObservableCollection<Currency>(Currency.LoadCurrencies());
        }

        public ObservableCollection<Currency> Currencies
        {
            get { return _currencies; }
            set { _currencies = value; NotifyPropertyChanged(); }
        }

        public int SelectedCurrencyIndex
        {
            get { return _selectedCurrencyIndex; }
            set { _selectedCurrencyIndex = value; NotifyPropertyChanged(); IsItemSelected = value != -1; }
        }

        public Currency SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                NotifyPropertyChanged();
                RefreshCanDelete();
            }
        }

        public bool IsItemSelected
        {
            get { return _isItemSelected; }
            set 
            { 
                _isItemSelected = value; 
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(CanEdit));
                NotifyPropertyChanged(nameof(CanDelete)); 
            }
        }

        public bool CanEdit
        {
            get { return _isItemSelected && _selectedItem != null; }
        }

        public bool CanDelete
        {
            get { return _isItemSelected && _selectedItem != null 
                    && !_selectedItem.IsDefaultCurrency && _currencies.Count > 1 && _selectedItem.Abbreviation != "USD"; }
        }

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(ReturnToPreviousScreen); }
        }

        private void ReturnToPreviousScreen()
        {
            PopViewModel();
        }

        public ICommand MoveToAddCurrencyScreen
        {
            get { return new RelayCommand(LoadAddCurrencyScreen); }
        }

        private void LoadAddCurrencyScreen()
        {
            PushViewModel(new CreateOrEditCurrencyViewModel(ViewModelChanger, this) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToEditCurrencyScreen
        {
            get { return new RelayCommand(LoadEditCurrencyScreen); }
        }

        private void LoadEditCurrencyScreen()
        {
            PushViewModel(new CreateOrEditCurrencyViewModel(ViewModelChanger, this, SelectedItem) { CurrentUser = CurrentUser });
        }

        public void DeleteItem(Currency currency)
        {
            if (currency != null && !currency.IsDefaultCurrency && Currencies.Count != 1)
            {
                currency.Delete();
                Currencies.Remove(currency);
            }
        }

        public void CreatedEditedCurrency(Currency currency, bool wasCreated)
        {
            if (currency.IsDefaultCurrency)
            {
                foreach (var type in Currencies)
                {
                    type.IsDefaultCurrency = type.ID == currency.ID;
                }
            }
            if (wasCreated)
            {
               Currencies.Add(currency);
            }
            RefreshCanDelete();
        }

        private void RefreshCanDelete()
        {
            NotifyPropertyChanged(nameof(CanEdit));
            NotifyPropertyChanged(nameof(CanDelete));
        }
    }
}
