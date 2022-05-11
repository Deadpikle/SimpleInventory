using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Interfaces
{
    public interface IPurchaseInfoChanged
    {
        void QuantityChanged(ItemSoldInfo info, int quantity);
    }
}
