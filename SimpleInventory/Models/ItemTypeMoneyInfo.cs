using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class ItemTypeMoneyInfo
    {
        public ItemType Type { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalProfit { get; set; }

        public ItemTypeMoneyInfo()
        {
            TotalIncome = 0m;
            TotalProfit = 0m;
        }

        public ItemTypeMoneyInfo(ItemType type)
        {
            Type = type;
        }
    }
}
