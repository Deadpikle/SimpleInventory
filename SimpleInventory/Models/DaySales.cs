using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class DaySales
    {
        public List<ReportItemSold> ItemsSold { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalProfit { get; set; }
        public Currency Currency { get; set; }
        public int TotalItemsSold { get; set; }

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

        public static DaySales GenerateDataForSingleDay(DateTime date)
        {
            var totalDaySaleInfo = new DaySales();
            totalDaySaleInfo.Date = date;
            totalDaySaleInfo.TotalIncome = 0;
            totalDaySaleInfo.TotalProfit = 0;
            totalDaySaleInfo.ItemsSold = new List<ReportItemSold>();
            totalDaySaleInfo.TotalItemsSold = 0;
            var currencies = Currency.LoadCurrencies();
            foreach (Currency currency in currencies)
            {
                if (currency.IsDefaultCurrency)
                {
                    totalDaySaleInfo.Currency = currency;
                    break;
                }
            }
            var data = ItemSoldInfo.LoadInfoForDate(date);

            var itemIDToReportSold = new Dictionary<int, ReportItemSold>();
            foreach (ItemSoldInfo singleItemInfo in data)
            {
                if (!itemIDToReportSold.ContainsKey(singleItemInfo.InventoryItemID))
                {
                    ReportItemSold itemSold = new ReportItemSold();
                    itemSold.Name = singleItemInfo.ItemName;
                    itemSold.QuantityPurchased = 0;
                    itemSold.CostPerItem = singleItemInfo.Cost; // TODO: should be handled as an average!
                    itemSold.CostCurrency = singleItemInfo.CostCurrency;
                    itemSold.TotalCost = 0;
                    itemSold.ProfitPerItem = singleItemInfo.ProfitPerItem; // TODO: should be handled as an average!
                    itemSold.ProfitCurrency = singleItemInfo.ProfitPerItemCurrency;
                    itemSold.TotalProfit = 0;
                    itemIDToReportSold[singleItemInfo.InventoryItemID] = itemSold;
                    totalDaySaleInfo.ItemsSold.Add(itemSold);
                }
                ReportItemSold itemSoldData = itemIDToReportSold[singleItemInfo.InventoryItemID];
                itemSoldData.QuantityPurchased += singleItemInfo.QuantitySold;
                totalDaySaleInfo.TotalItemsSold += singleItemInfo.QuantitySold;
                if (itemSoldData.CostCurrency.ID == singleItemInfo.CostCurrency.ID)
                {
                    itemSoldData.TotalCost += singleItemInfo.QuantitySold * singleItemInfo.Cost;
                }
                else
                {
                    itemSoldData.TotalCost += singleItemInfo.QuantitySold * 
                        Utilities.ConvertAmount(singleItemInfo.Cost, singleItemInfo.CostCurrency, itemSoldData.CostCurrency);
                }
                if (itemSoldData.ProfitCurrency.ID == singleItemInfo.ProfitPerItemCurrency.ID)
                {
                    itemSoldData.TotalProfit += singleItemInfo.QuantitySold * singleItemInfo.ProfitPerItem;
                }
                else
                {
                    itemSoldData.TotalProfit += singleItemInfo.QuantitySold * 
                        Utilities.ConvertAmount(singleItemInfo.ProfitPerItem, singleItemInfo.ProfitPerItemCurrency, itemSoldData.ProfitCurrency);
                }
                // now add to total income/profit
                if (totalDaySaleInfo.Currency.ID == singleItemInfo.CostCurrency.ID)
                {
                    totalDaySaleInfo.TotalIncome += singleItemInfo.QuantitySold * singleItemInfo.Cost;
                }
                else
                {
                    totalDaySaleInfo.TotalIncome += singleItemInfo.QuantitySold *
                        Utilities.ConvertAmount(singleItemInfo.Cost, singleItemInfo.CostCurrency, totalDaySaleInfo.Currency);
                }
                if (totalDaySaleInfo.Currency.ID == singleItemInfo.ProfitPerItemCurrency.ID)
                {
                    totalDaySaleInfo.TotalProfit += singleItemInfo.QuantitySold * singleItemInfo.ProfitPerItem;
                }
                else
                {
                    totalDaySaleInfo.TotalProfit += singleItemInfo.QuantitySold *
                        Utilities.ConvertAmount(singleItemInfo.ProfitPerItem, singleItemInfo.ProfitPerItemCurrency, totalDaySaleInfo.Currency);
                }
            }
            return totalDaySaleInfo;
        }
    }
}
