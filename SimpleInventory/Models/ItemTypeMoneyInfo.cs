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
        public decimal TotalCashIncome { get; set; }
        public decimal TotalQRCodeIncome { get; set; }
        public decimal TotalProfit { get; set; }
        public Currency Currency { get; set; }

        public ItemTypeMoneyInfo()
        {
            TotalIncome = 0m;
            TotalCashIncome = 0m;
            TotalQRCodeIncome = 0m;
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
                    return string.Format("{0:#,#0.##} ({1})", TotalIncome, Currency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", TotalIncome);
            }
        }

        public string TotalProfitWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", TotalProfit, Currency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", TotalProfit);
            }
        }
    }
}
