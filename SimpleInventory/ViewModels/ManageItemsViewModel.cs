using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using SimpleInventory.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class ManageItemsViewModel : BaseViewModel, ICreatedInventoryItem
    {
        private ObservableCollection<InventoryItem> _items;
        private int _selectedIndex = 0;
        private InventoryItem _selectedItem;

        private bool _isItemSelected;

        public ManageItemsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            Items = new ObservableCollection<InventoryItem>(InventoryItem.LoadItemsNotDeleted());
            IsItemSelected = false;
        }

        public ObservableCollection<InventoryItem> Items
        {
            get { return _items; }
            set { _items = value; NotifyPropertyChanged(); }
        }

        public bool IsItemSelected
        {
            get { return _isItemSelected; }
            set { _isItemSelected = value; NotifyPropertyChanged(); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; NotifyPropertyChanged(); IsItemSelected = value != -1; }
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
            PushViewModel(new CreateOrEditItemViewModel(ViewModelChanger, this) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToEditItemScreen
        {
            get { return new RelayCommand(LoadEditItemScreen); }
        }

        private void LoadEditItemScreen()
        {
            if (SelectedItem != null)
            {
                PushViewModel(new CreateOrEditItemViewModel(ViewModelChanger, SelectedItem) { CurrentUser = CurrentUser });
            }
        }

        public ICommand MoveToAdjustQuantityScreen
        {
            get { return new RelayCommand(LoadAdjustQuantityScreen); }
        }

        private void LoadAdjustQuantityScreen()
        {
            PushViewModel(new AdjustQuantityViewModel(ViewModelChanger, SelectedItem) { CurrentUser = CurrentUser });
        }

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }

        public void CreatedInventoryItem(InventoryItem item)
        {
            Items.Add(item);
        }

        public void DeleteItem(InventoryItem item)
        {
            if (item != null)
            {
                item.Delete();
                Items.Remove(item);
            }
        }

        public ICommand MoveToViewQuantityChangesScreen
        {
            get { return new RelayCommand(LoadViewQuantityChangesScreen); }
        }

        private void LoadViewQuantityChangesScreen()
        {
            if (SelectedItem != null)
            {
                PushViewModel(new ViewQuantityAdjustmentsViewModel(ViewModelChanger, SelectedItem) { CurrentUser = CurrentUser });
            }
        }
    }
}
