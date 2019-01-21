using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SimpleInventory.Models;

namespace SimpleInventory.ViewModels
{
    class ViewQuantityAdjustmentsViewModel : BaseViewModel
    {

        private List<QuantityAdjustment> _adjustments;

        public ViewQuantityAdjustmentsViewModel(IChangeViewModel viewModelChanger, InventoryItem item) : base(viewModelChanger)
        {
            _adjustments = QuantityAdjustment.LoadQuantityAdjustments(item);
        }

        public List<QuantityAdjustment> Adjustments
        {
            get { return _adjustments; }
            set { _adjustments = value; NotifyPropertyChanged(); }
        }

        public ICommand ReturnToManageItems
        {
            get { return new RelayCommand(PopToManageItems); }
        }

        private void PopToManageItems()
        {
            PopViewModel();
        }

        public ICommand AdjustExplanation
        {
            get { return new RelayCommand<QuantityAdjustment>(o => LoadEditQuantityScreen(o)); }
        }

        private void LoadEditQuantityScreen(QuantityAdjustment adjustment)
        {
            PushViewModel(new AdjustQuantityViewModel(ViewModelChanger, adjustment) { CurrentUser = CurrentUser });
        }
    }
}
