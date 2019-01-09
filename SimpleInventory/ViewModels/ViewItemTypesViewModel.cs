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
    class ViewItemTypesViewModel : BaseViewModel
    {
        private List<ItemType> _itemTypes;
        private int _selectedItemTypeIndex;
        private ItemType _selectedItem;

        public ViewItemTypesViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            ItemTypes = ItemType.LoadItemTypes();
        }

        public List<ItemType> ItemTypes
        {
            get { return _itemTypes; }
            set { _itemTypes = value; NotifyPropertyChanged(); }
        }

        public int SelectedItemTypeIndex
        {
            get { return _selectedItemTypeIndex; }
            set { _selectedItemTypeIndex = value; NotifyPropertyChanged(); }
        }

        public ItemType SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; NotifyPropertyChanged(); }
        }

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(ReturnToPreviousScreen); }
        }

        private void ReturnToPreviousScreen()
        {
            PopViewModel();
        }
    }
}
