using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleInventory.ViewModels
{
    class ManageItemsViewModel : BaseViewModel
    {
        public ManageItemsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
        }

        public ICommand MoveToAddItemScreen
        {
            get { return new RelayCommand(LoadAddItemScreen); }
        }

        private void LoadAddItemScreen()
        {
            PushViewModel(new CreateOrEditItemViewModel(ViewModelChanger, true));
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
