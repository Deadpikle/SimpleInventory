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
    class AddItemViewModel : BaseViewModel
    {
        public AddItemViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
        }

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(o => PopToMainMenu()); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }
    }
}
