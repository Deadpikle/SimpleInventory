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
    class CreateOrEditItemViewModel : BaseViewModel
    {
        private bool _isCreating;
        private int _inventoryItemID;

        private string _name;
        private string _description;
        private string _cost;
        private string _profitPerItem;

        private int _indexOfDefaultCurrency;
        private int _selectedCostCurrencyIndex;
        private int _selectedProfitCurrencyIndex;

        private int _quantity;
        private string _barcodeNumber;

        private List<Currency> _currencies;

        public CreateOrEditItemViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _isCreating = true;
            _currencies = Currency.LoadCurrencies();
            SetupCurrencyIndices();
            Name = "";
            Description = "";
            Cost = "0.0";
            ProfitPerItem = "0.0";
            Quantity = 0;
            BarcodeNumber = "";
        }

        public CreateOrEditItemViewModel(IChangeViewModel viewModelChanger, InventoryItem item) : base(viewModelChanger)
        {
            _isCreating = false;
            _inventoryItemID = item.ID;
            _currencies = Currency.LoadCurrencies();
            Name = item.Name;
            Description = item.Description;
            Cost = item.Cost.ToString();
            ProfitPerItem = item.ProfitPerItem.ToString();
            Quantity = item.Quantity;
            BarcodeNumber = item.BarcodeNumber;
        }

        public User CurrentUser { get; set; }

        public List<Currency> Currencies
        {
            get { return _currencies; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; NotifyPropertyChanged(); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; NotifyPropertyChanged(); }
        }

        public string Cost
        {
            get { return _cost; }
            set { _cost = value; NotifyPropertyChanged(); }
        }

        public int SelectedCostCurrencyIndex
        {
            get { return _selectedCostCurrencyIndex; }
            set { _selectedCostCurrencyIndex = value; NotifyPropertyChanged(); }
        }

        public string ProfitPerItem
        {
            get { return _profitPerItem; }
            set { _profitPerItem = value; NotifyPropertyChanged(); }
        }

        public int SelectedProfitCurrencyIndex
        {
            get { return _selectedProfitCurrencyIndex; }
            set { _selectedProfitCurrencyIndex = value; NotifyPropertyChanged(); }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; NotifyPropertyChanged(); }
        }

        public string BarcodeNumber
        {
            get { return _barcodeNumber; }
            set { _barcodeNumber = value; NotifyPropertyChanged(); }
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

        public ICommand SaveItem
        {
            get { return new RelayCommand(CreateOrSaveItem); }
        }

        #endregion

        private void SetupCurrencyIndices(InventoryItem item = null)
        {
            var didSetCostCurrencyIndex = false;
            var didSetProfitCurrencyIndex = false;
            for (int i = 0; i < _currencies.Count; i++)
            {
                var currency = _currencies[i];
                if (currency.IsDefaultCurrency)
                {
                    _indexOfDefaultCurrency = i;
                    if (item == null)
                    {
                        _selectedCostCurrencyIndex = i;
                        _selectedProfitCurrencyIndex = i;
                        didSetCostCurrencyIndex = didSetProfitCurrencyIndex = true;
                    }
                }
                if (item != null)
                {
                    if (item.CostCurrency != null && item.CostCurrency.ID == currency.ID)
                    {
                        _selectedCostCurrencyIndex = i;
                        didSetCostCurrencyIndex = true;
                    }
                    if (item.ProfitPerItemCurrency != null && item.ProfitPerItemCurrency.ID == currency.ID)
                    {
                        _selectedProfitCurrencyIndex = i;
                        didSetProfitCurrencyIndex = true;
                    }
                }
            }
            if (!didSetCostCurrencyIndex)
            {
                _selectedCostCurrencyIndex = _indexOfDefaultCurrency;
            }
            if (!didSetProfitCurrencyIndex)
            {
                _selectedProfitCurrencyIndex = _indexOfDefaultCurrency;
            }
        }

        private void CreateOrSaveItem()
        {
            // validate
            // create/save
            var item = new InventoryItem();
            item.Name = Name;
            item.Description = Description;
            decimal cost = 0m;
            bool didParse = Decimal.TryParse(Cost, out cost);
            item.Cost = didParse ? cost : 0m;
            item.CostCurrency = _currencies[_selectedCostCurrencyIndex];

            decimal profit = 0m;
            didParse = Decimal.TryParse(ProfitPerItem, out profit);
            item.ProfitPerItem = didParse ? profit : 0m;
            item.ProfitPerItemCurrency = _currencies[_selectedProfitCurrencyIndex];

            item.BarcodeNumber = BarcodeNumber;
            item.PicturePath = "";
            item.Quantity = Quantity;
            var userID = CurrentUser != null ? CurrentUser.ID : 1;
            if (_isCreating)
            {
                item.CreateNewItem(userID);
            }
            else
            {
                item.ID = _inventoryItemID;
                item.SaveItemUpdates(userID);
            }
            PopViewModel();
        }
    }
}
