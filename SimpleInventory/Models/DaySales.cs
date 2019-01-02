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

        public static DaySales GenerateDataForSingleDay(DateTime date)
        {
            var totalDaySaleInfo = new DaySales();
            totalDaySaleInfo.Date = date;
            totalDaySaleInfo.TotalIncome = 0;
            totalDaySaleInfo.TotalProfit = 0;
            totalDaySaleInfo.ItemsSold = new List<ReportItemSold>();
            var data = ItemSoldInfo.LoadInfoForDate(date);

            var itemIDToReportSold = new Dictionary<int, ReportItemSold>();
            foreach (ItemSoldInfo info in data)
            {
                if (!itemIDToReportSold.ContainsKey(info.InventoryItemID))
                {
                    ReportItemSold itemSold = new ReportItemSold();
                    itemSold.Name = info.ItemName;
                    itemSold.QuantityPurchased = 0;
                    itemSold.CostPerItem = info.Cost;
                    itemSold.CostCurrency = info.CostCurrency;
                    itemSold.TotalCost = 0;
                    itemSold.ProfitPerItem = info.ProfitPerItem;
                    itemSold.ProfitCurrency = info.ProfitPerItemCurrency;
                    itemSold.TotalProfit = 0;
                    itemIDToReportSold[info.InventoryItemID] = itemSold;
                    totalDaySaleInfo.ItemsSold.Add(itemSold);
                }
                // TODO: handle differences in currency!
                ReportItemSold itemSoldData = itemIDToReportSold[info.InventoryItemID];
                itemSoldData.QuantityPurchased += info.QuantitySold;
                itemSoldData.TotalCost += info.QuantitySold * info.Cost;
                itemSoldData.TotalProfit += info.QuantitySold * info.ProfitPerItem;

                totalDaySaleInfo.TotalIncome += info.QuantitySold * info.Cost;
                totalDaySaleInfo.TotalProfit += info.QuantitySold * info.ProfitPerItem;
            }
            return totalDaySaleInfo;
        }
    }
}
