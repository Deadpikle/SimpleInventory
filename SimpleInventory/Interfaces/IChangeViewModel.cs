using SimpleInventory.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Interfaces
{
    interface IChangeViewModel
    {
        void PushViewModel(BaseViewModel model);
        void PopViewModel();
    }
}
