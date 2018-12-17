using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class InventoryItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PicturePath { get; set; }

        public decimal CostDollars { get; set; }
        public int CostRiel { get; set; }
        
        public int Quantity { get; set; }

        // BarcodeNumber is a string just in case we need to change from #'s later or it's really long or something
        public string BarcodeNumber { get; set; } 
    }
}
