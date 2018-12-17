using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System.Collections.Generic;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class ManageItemsViewModel : BaseViewModel
    {
        private List<InventoryItem> _items;
        private int _selectedIndex = 0;
        private InventoryItem _selectedItem;

        public ManageItemsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            Items = InventoryItem.LoadItems();
        }

        public List<InventoryItem> Items
        {
            get { return _items; }
            set { _items = value; NotifyPropertyChanged(); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; NotifyPropertyChanged(); }
        }

        public InventoryItem SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; NotifyPropertyChanged(); }
        }

        public ICommand MoveToAddItemScreen
        {
            get { return new RelayCommand(LoadAddItemScreen); }
        }

        private void LoadAddItemScreen()
        {
            PushViewModel(new CreateOrEditItemViewModel(ViewModelChanger));
        }

        public ICommand MoveToEditItemScreen
        {
            get { return new RelayCommand(LoadEditItemScreen); }
        }

        private void LoadEditItemScreen()
        {
            if (SelectedItem != null)
            {
                PushViewModel(new CreateOrEditItemViewModel(ViewModelChanger, SelectedItem));
            }
        }

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }
    }
}
