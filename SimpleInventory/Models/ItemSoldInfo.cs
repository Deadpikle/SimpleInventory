using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class ItemSoldInfo
    {
        public int ID { get; set; }
        public DateTime DateTimeSold { get; set; }
        public int QuantitySold { get; set; } // defaults to 1
        public int InventoryItemID { get; set; }
        public int SoldByUserID { get; set; }

        // we remember the cost of things in case it changes over time so we have the original info :)
        public decimal CostDollars { get; set; }
        public int CostRiel { get; set; } // payment defaults to Riel
        public decimal PaidDollars { get; set; }
        public int PaidRiel { get; set; } // payment defaults to Riel
        public decimal ChangeDollars { get; set; }
        public int ChangeRiel { get; set; }
    }
}
