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

        public CreateOrEditItemTypeViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            ScreenTitle = "Add Item Type";
        }

        public CreateOrEditItemTypeViewModel(IChangeViewModel viewModelChanger, ItemType itemType) : base(viewModelChanger)
        {
            ScreenTitle = "Edit Item Type";
        }

        public string ScreenTitle
        {
            get { return _screenTitle; }
            set { _screenTitle = value; NotifyPropertyChanged(); }
        }

        public ICommand ReturnToManageTypes
        {
            get { return new RelayCommand(PopScreen); }
        }

        private void PopScreen()
        {
            PopViewModel();
        }
    }
}
