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
        public decimal DollarsPaid { get; set; }
        public int RielPaid { get; set; } // payment defaults to Riel

        public decimal DollarsChange { get; set; }
        public int RielChange { get; set; }
    }
}
