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
    class AdjustQuantityViewModel : BaseViewModel
    {
        public AdjustQuantityViewModel(IChangeViewModel viewModelChanger, InventoryItem item) : base(viewModelChanger)
        {

        }

        public ICommand GoToManageItems
        {
            get { return new RelayCommand(ReturnToPreviousScreen); }
        }

        private void ReturnToPreviousScreen()
        {
            PopViewModel();
        }
    }
}
