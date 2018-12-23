using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class QuantityAdjustment
    {
        int ID { get; set; }
        int AmountChanged { get; set; }
        DateTime DateTimeChanged { get; set; }
        int InventoryItemID { get; set; }
        int UserID { get; set; }
    }
}
