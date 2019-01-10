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
    class CreateOrEditItemTypeViewModel : BaseViewModel
    {
        private string _screenTitle;
        private bool _isDefaultCategoryCheckboxEnabled;

        private ICreatedEditedItemType _createdEditedItemType;
        private ItemType _itemBeingEdited;

        private string _name;
        private string _description;
        private bool _isDefault;

        public CreateOrEditItemTypeViewModel(IChangeViewModel viewModelChanger, ICreatedEditedItemType createdItemType) : base(viewModelChanger)
        {
            ScreenTitle = "Add Item Category";
            IsDefaultCategoryCheckboxEnabled = true;
            _itemBeingEdited = null;
            _createdEditedItemType = createdItemType;
            Name = "";
            Description = "";
            IsDefault = false;
        }

        public CreateOrEditItemTypeViewModel(IChangeViewModel viewModelChanger, ICreatedEditedItemType createdItemType, ItemType itemType) : base(viewModelChanger)
        {
            ScreenTitle = "Edit Item Category";
            IsDefaultCategoryCheckboxEnabled = !itemType.IsDefault;
            _itemBeingEdited = itemType;
            _createdEditedItemType = createdItemType;
            Name = itemType.Name;
            Description = itemType.Description;
            IsDefault = itemType.IsDefault;
        }

        public string ScreenTitle
        {
            get { return _screenTitle; }
            set { _screenTitle = value; NotifyPropertyChanged(); }
        }

        public bool IsDefaultCategoryCheckboxEnabled
        {
            get { return _isDefaultCategoryCheckboxEnabled; }
            set { _isDefaultCategoryCheckboxEnabled = value; NotifyPropertyChanged(); }
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

        public bool IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value; NotifyPropertyChanged(); }
        }

        public ICommand ReturnToManageTypes
        {
            get { return new RelayCommand(PopScreen); }
        }

        private void PopScreen()
        {
            PopViewModel();
        }

        public ICommand SaveItemType
        {
            get { return new RelayCommand(SaveData); }
        }

        private void SaveData()
        {
            var itemType = _itemBeingEdited != null ? _itemBeingEdited : new ItemType();
            itemType.Name = Name;
            itemType.Description = Description;
            itemType.IsDefault = IsDefault;
            if (_itemBeingEdited != null)
            {
                itemType.ID = _itemBeingEdited.ID;
                itemType.Save();
            }
            else
            {
                itemType.Create();
            }
            _createdEditedItemType?.CreatedEditedItemType(itemType, _itemBeingEdited == null);
            PopViewModel();
        }
    }
}
