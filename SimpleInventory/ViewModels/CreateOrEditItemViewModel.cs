using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private bool _isDrink;

        private int _indexOfDefaultCurrency;
        private int _selectedCostCurrencyIndex;
        private int _selectedProfitCurrencyIndex;

        private int _quantity;
        private string _barcodeNumber;

        private InventoryItem _currentItemBeingEdited;
        private ICreatedInventoryItem _createdItemListener;

        private List<Currency> _currencies;

        public CreateOrEditItemViewModel(IChangeViewModel viewModelChanger, ICreatedInventoryItem createdItemListener) : base(viewModelChanger)
        {
            _isCreating = true;
            _currencies = Currency.LoadCurrencies();
            _currentItemBeingEdited = null;
            SetupCurrencyIndices();
            Name = "";
            Description = "";
            Cost = "0";
            ProfitPerItem = "0";
            Quantity = 0;
            BarcodeNumber = "";
            IsDrink = false;
            _createdItemListener = createdItemListener;
        }

        public CreateOrEditItemViewModel(IChangeViewModel viewModelChanger, InventoryItem item) : base(viewModelChanger)
        {
            _isCreating = false;
            _currentItemBeingEdited = item;
            _inventoryItemID = item.ID;
            _currencies = Currency.LoadCurrencies();
            Name = item.Name;
            Description = item.Description;
            Cost = item.Cost.ToString();
            ProfitPerItem = item.ProfitPerItem.ToString();
            Quantity = item.Quantity;
            BarcodeNumber = item.BarcodeNumber;
            IsDrink = item.IsDrink;
        }

        #region Properties

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

        public bool IsDrink
        {
            get { return _isDrink; }
            set { _isDrink = value; NotifyPropertyChanged(); }
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

        public bool IsCreating
        {
            get { return _isCreating; }
            set { _isCreating = value; NotifyPropertyChanged(); }
        }

        #endregion

        #region ICommands

        public ICommand PopBack
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
            var item = _currentItemBeingEdited != null ? _currentItemBeingEdited : new InventoryItem();
            // validate
            bool didValidate = true;
            string errorMessage = "";
            if (!string.IsNullOrWhiteSpace(BarcodeNumber))
            {
                var loadedItem = InventoryItem.LoadItemByBarcode(BarcodeNumber);
                if (loadedItem != null && (_isCreating || (!_isCreating && _inventoryItemID != loadedItem.ID)))
                {
                    didValidate = false;
                    errorMessage = "Barcode already exists for item named " + loadedItem.Name;
                }
            }
            if (didValidate)
            {
                // create/save
                item.Name = Name;
                item.Description = Description;
                item.IsDrink = IsDrink;
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
                if (_isCreating) // any further adjustments have to be made via the adjust quantity screen
                {
                    item.Quantity = Quantity;
                }
                var userID = CurrentUser != null ? CurrentUser.ID : 1;
                if (_isCreating)
                {
                    item.CreateNewItem(userID);
                    QuantityAdjustment.UpdateQuantity(Quantity, item.ID, userID);
                    _createdItemListener?.CreatedInventoryItem(item);
                }
                else
                {
                    item.ID = _inventoryItemID;
                    item.SaveItemUpdates(userID);
                }
                PopViewModel();
            }
            else if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                MessageBox.Show(errorMessage, "Error!", MessageBoxButton.OK);
            }
        }
    }
}
