using SimpleInventory.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class ReportItemSold
    {
        public int InventoryItemID { get; set; }
        public ItemType ItemType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal QuantityPurchased { get; set; }
        public decimal CostPerItem { get; set; } // in theory, should be an average, as this could change over time
        public Currency CostCurrency { get; set; }
        public decimal CashPurchases { get; set; }
        public decimal QRCodePurchases { get; set; }

        public string CostWithCurrency
        {
            get
            {
                if (CostCurrency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", CostPerItem, CostCurrency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", CostPerItem);
            }
        }

        public decimal TotalCost { get; set; }

        public string TotalCostWithCurrency
        {
            get
            {
                if (CostCurrency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", TotalCost, CostCurrency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", TotalCost);
            }
        }

        public string TotalCashCostWithCurrency
        {
            get
            {
                if (CostCurrency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", CashPurchases, CostCurrency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", CashPurchases);
            }
        }

        public string TotalQRCodeCostWithCurrency
        {
            get
            {
                if (CostCurrency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", QRCodePurchases, CostCurrency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", QRCodePurchases);
            }
        }

        public decimal ProfitPerItem { get; set; } // in theory, should be an average, as this could change over time
        public Currency ProfitCurrency { get; set; }
        public decimal TotalProfit { get; set; }

        public string ProfitWithCurrency
        {
            get
            {
                if (ProfitCurrency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", ProfitPerItem, ProfitCurrency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", ProfitPerItem);
            }
        }

        public string TotalProfitWithCurrency
        {
            get
            {
                if (ProfitCurrency != null)
                {
                    return string.Format("{0:#,#0.##} ({1})", TotalProfit, ProfitCurrency?.Symbol);
                }
                return string.Format("{0:#,#0.##}", TotalProfit);
            }
        }
    }
}
