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
    class HomeScreenViewModel : BaseViewModel
    {

        public HomeScreenViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
        }

        public ICommand MoveToManageItemsScreen
        {
            get { return new RelayCommand(LoadManageItemsScreen); }
        }

        private void LoadManageItemsScreen()
        {
            PushViewModel(new ManageItemsViewModel(ViewModelChanger));
        }
    }
}
