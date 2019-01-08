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
        public int TotalItemsSold { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalProfit { get; set; }
        public Currency Currency { get; set; }

        public ItemTypeMoneyInfo()
        {
            TotalIncome = 0m;
            TotalProfit = 0m;
            TotalItemsSold = 0;
        }

        public ItemTypeMoneyInfo(ItemType type) : base()
        {
            Type = type;
        }

        public string TotalIncomeWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return TotalIncome.ToString() + " (" + Currency?.Symbol + ")";
                }
                return TotalIncome.ToString();
            }
        }

        public string TotalProfitWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return TotalProfit.ToString() + " (" + Currency?.Symbol + ")";
                }
                return TotalProfit.ToString();
            }
        }
    }
}
