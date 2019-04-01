using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class DaySales : IItemsSoldReportData
    {
        public List<ReportItemSold> ItemsSold { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalProfit { get; set; }
        public Currency Currency { get; set; }
        public int TotalItemsSold { get; set; }

        public List<ItemTypeMoneyInfo> ItemTypeMoneyBreakdown { get; set; }
        public Dictionary<int, ItemTypeMoneyInfo> ItemTypeIDToMoneyInfo { get; private set; }

        public DaySales()
        {
            ItemTypeMoneyBreakdown = new List<ItemTypeMoneyInfo>();
            ItemTypeIDToMoneyInfo = new Dictionary<int, ItemTypeMoneyInfo>();
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

        public static DaySales GenerateDataForSingleDay(DateTime date, int userID = -1)
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
            var data = ItemSoldInfo.LoadInfoForDate(date, userID);

            var itemIDToReportSold = new Dictionary<int, ReportItemSold>();
            foreach (ItemSoldInfo singleItemInfo in data)
            {
                if (!itemIDToReportSold.ContainsKey(singleItemInfo.InventoryItemID))
                {
                    ReportItemSold itemSold = new ReportItemSold();
                    itemSold.InventoryItemID = singleItemInfo.InventoryItemID;
                    itemSold.ItemType = singleItemInfo.ItemType;
                    itemSold.Name = singleItemInfo.ItemName;
                    itemSold.Description = singleItemInfo.ItemDescription;
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
                // now add to total income/profit after finding item type money info
                ItemTypeMoneyInfo moneyInfo = null;
                if (singleItemInfo.ItemType != null)
                {
                    if (!totalDaySaleInfo.ItemTypeIDToMoneyInfo.ContainsKey(singleItemInfo.ItemType.ID))
                    {
                        var itemTypeMoneyInfo = new ItemTypeMoneyInfo(singleItemInfo.ItemType);
                        itemTypeMoneyInfo.Currency = totalDaySaleInfo.Currency;
                        totalDaySaleInfo.ItemTypeIDToMoneyInfo[singleItemInfo.ItemType.ID] = itemTypeMoneyInfo;
                        totalDaySaleInfo.ItemTypeMoneyBreakdown.Add(itemTypeMoneyInfo);
                    }
                    moneyInfo = totalDaySaleInfo.ItemTypeIDToMoneyInfo[singleItemInfo.ItemType.ID];
                }
                moneyInfo.TotalItemsSold += singleItemInfo.QuantitySold;
                if (totalDaySaleInfo.Currency.ID == singleItemInfo.CostCurrency.ID)
                {
                    var amountIncrease = singleItemInfo.QuantitySold * singleItemInfo.Cost;
                    totalDaySaleInfo.TotalIncome += amountIncrease;
                    if (moneyInfo != null)
                    {
                        moneyInfo.TotalIncome += amountIncrease;
                    }
                }
                else
                {
                    var amountIncrease = singleItemInfo.QuantitySold *
                        Utilities.ConvertAmount(singleItemInfo.Cost, singleItemInfo.CostCurrency, totalDaySaleInfo.Currency);
                    totalDaySaleInfo.TotalIncome += amountIncrease;
                    if (moneyInfo != null)
                    {
                        moneyInfo.TotalIncome += amountIncrease;
                    }
                }
                if (totalDaySaleInfo.Currency.ID == singleItemInfo.ProfitPerItemCurrency.ID)
                {
                    var amountIncrease = singleItemInfo.QuantitySold * singleItemInfo.ProfitPerItem;
                    totalDaySaleInfo.TotalProfit += amountIncrease;
                    if (moneyInfo != null)
                    {
                        moneyInfo.TotalProfit += amountIncrease;
                    }
                }
                else
                {
                    var amountIncrease = singleItemInfo.QuantitySold *
                        Utilities.ConvertAmount(singleItemInfo.ProfitPerItem, singleItemInfo.ProfitPerItemCurrency, totalDaySaleInfo.Currency);
                    totalDaySaleInfo.TotalProfit += amountIncrease;
                    if (moneyInfo != null)
                    {
                        moneyInfo.TotalProfit += amountIncrease;
                    }
                }
            }
            totalDaySaleInfo.ItemsSold.Sort((left, right) => left.Name.ToLower().CompareTo(right.Name.ToLower()));
            totalDaySaleInfo.ItemTypeMoneyBreakdown.Sort((left, right) => left.Type.Name.ToLower().CompareTo(right.Type.Name.ToLower()));
            return totalDaySaleInfo;
        }

        #region IItemsSoldReportData

        public DateTime GetDate()
        {
            return Date;
        }

        public List<ReportItemSold> GetItemsSold()
        {
            return ItemsSold;
        }

        public string GetTotalIncomeWithCurrency()
        {
            return TotalIncomeWithCurrency;
        }

        public int GetTotalItemsSold()
        {
            return TotalItemsSold;
        }

        public string GetTotalProfitWithCurrency()
        {
            return TotalProfitWithCurrency;
        }

        public bool IsDailyReport()
        {
            return true;
        }

        public List<ItemTypeMoneyInfo> GetItemTypeMoneyInfo()
        {
            return ItemTypeMoneyBreakdown;
        }

        #endregion
    }
}
