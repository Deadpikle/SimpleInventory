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

        public string CostWithCurrency
        {
            get
            {
                if (CostCurrency != null)
                {
                    return CostPerItem.ToString() + " (" + CostCurrency?.Symbol + ")";
                }
                return CostPerItem.ToString();
            }
        }

        public decimal TotalCost { get; set; }

        public string TotalCostWithCurrency
        {
            get
            {
                if (CostCurrency != null)
                {
                    return TotalCost.ToString() + " (" + CostCurrency?.Symbol + ")";
                }
                return TotalCost.ToString();
            }
        }

        public decimal ProfitPerItem { get; set; }
        public Currency ProfitCurrency { get; set; }
        public decimal TotalProfit { get; set; }

        public string ProfitWithCurrency
        {
            get
            {
                if (ProfitCurrency != null)
                {
                    return ProfitPerItem.ToString() + " (" + ProfitCurrency?.Symbol + ")";
                }
                return ProfitPerItem.ToString();
            }
        }

        public string TotalProfitWithCurrency
        {
            get
            {
                if (ProfitCurrency != null)
                {
                    return TotalProfit.ToString() + " (" + ProfitCurrency?.Symbol + ")";
                }
                return TotalProfit.ToString();
            }
        }
    }
}
