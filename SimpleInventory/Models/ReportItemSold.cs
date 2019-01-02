using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class ReportItemSold
    {
        public string Name { get; set; }
        public decimal QuantityPurchased { get; set; }
        public decimal CostPerItem { get; set; }
        public Currency CostCurrency { get; set; }
        public decimal TotalCost { get; set; }
        public decimal ProfitPerItem { get; set; }
        public Currency ProfitCurrency { get; set; }
        public decimal TotalProfit { get; set; }
    }
}
