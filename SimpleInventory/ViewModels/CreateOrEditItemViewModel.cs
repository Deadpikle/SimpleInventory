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
        private int _quantity;
        private string _barcodeNumber;

        public CreateOrEditItemViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _isCreating = true;
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
            Name = item.Name;
            Description = item.Description;
            Cost = item.Cost.ToString();
            ProfitPerItem = item.ProfitPerItem.ToString();
            Quantity = item.Quantity;
            BarcodeNumber = item.BarcodeNumber;
        }

        public User CurrentUser { get; set; }

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

        public string ProfitPerItem
        {
            get { return _profitPerItem; }
            set { _profitPerItem = value; NotifyPropertyChanged(); }
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

            decimal profit = 0m;
            didParse = Decimal.TryParse(ProfitPerItem, out profit);
            item.ProfitPerItem = didParse ? profit : 0m;

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
