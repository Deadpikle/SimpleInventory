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
        private int _costRiel;
        private string _costDollars;
        private int _quantity;
        private string _barcodeNumber;

        public CreateOrEditItemViewModel(IChangeViewModel viewModelChanger, bool isCreating) : base(viewModelChanger)
        {
            _isCreating = isCreating;
            Name = "";
            Description = "";
            CostRiel = 0;
            CostDollars = "0.0";
            Quantity = 0;
            BarcodeNumber = "";
        }

        public User CurrentUser { get; set; }

        public int InventoryItemID
        {
            get { return _inventoryItemID; }
            set { _inventoryItemID = value; NotifyPropertyChanged(); }
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

        public int CostRiel
        {
            get { return _costRiel; }
            set { _costRiel = value; NotifyPropertyChanged(); }
        }

        public string CostDollars
        {
            get { return _costDollars; }
            set { _costDollars = value; NotifyPropertyChanged(); }
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
            decimal dollarCost = 0m;
            bool didParse = Decimal.TryParse(CostDollars, out dollarCost);
            item.CostDollars = didParse ? dollarCost : 0m;
            item.CostRiel = CostRiel;
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
                item.ID = InventoryItemID;
                item.SaveItemUpdates(userID);
            }
            PopViewModel();
        }
    }
}
