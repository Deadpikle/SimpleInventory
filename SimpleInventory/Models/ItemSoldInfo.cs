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
        public decimal Cost { get; set; }
        public Currency CostCurrency { get; set; }
        public decimal Paid { get; set; }
        public Currency PaidCurrency { get; set; }
        public decimal Change { get; set; }
        public Currency ChangeCurrency { get; set; }
        public decimal ProfitPerItem { get; set; }
        public Currency ProfitPerItemCurrency { get; set; }
    }
}
