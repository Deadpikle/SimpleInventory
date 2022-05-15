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
                    return string.Format("{0:n} ({1})", TotalIncome, Currency?.Symbol);
                }
                return string.Format("{0:n}", TotalIncome);
            }
        }

        public string TotalProfitWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return string.Format("{0:n} ({1})", TotalProfit, Currency?.Symbol);
                }
                return string.Format("{0:n}", TotalProfit);
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
                itemSoldData.TotalCost +=
                    Utilities.ConvertAmount(singleItemInfo.QuantitySold * singleItemInfo.Cost, singleItemInfo.CostCurrency, itemSoldData.CostCurrency);
                itemSoldData.TotalProfit +=
                    Utilities.ConvertAmount(singleItemInfo.QuantitySold * singleItemInfo.ProfitPerItem,
                    singleItemInfo.ProfitPerItemCurrency, itemSoldData.ProfitCurrency);
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
                if (moneyInfo != null)
                {
                    moneyInfo.TotalItemsSold += singleItemInfo.QuantitySold;
                }
                var amountIncrease =
                    Utilities.ConvertAmount(singleItemInfo.QuantitySold * singleItemInfo.Cost,
                    singleItemInfo.CostCurrency, totalDaySaleInfo.Currency);
                totalDaySaleInfo.TotalIncome += amountIncrease;
                if (moneyInfo != null)
                {
                    moneyInfo.TotalIncome += amountIncrease;
                }
                // calc profit
                amountIncrease =
                    Utilities.ConvertAmount(singleItemInfo.QuantitySold * singleItemInfo.ProfitPerItem,
                    singleItemInfo.ProfitPerItemCurrency, totalDaySaleInfo.Currency);
                totalDaySaleInfo.TotalProfit += amountIncrease;
                if (moneyInfo != null)
                {
                    moneyInfo.TotalProfit += amountIncrease;
                }
            }
            // now need to run the SAME report data for PurchasedItem based on Purchases!
            var purchaseData = Purchase.LoadInfoForDate(date, userID);
            foreach (Purchase purchase in purchaseData)
            {
                foreach (PurchasedItem item in purchase.Items)
                {
                    if (!itemIDToReportSold.ContainsKey(item.InventoryItemID))
                    {
                        ReportItemSold itemSold = new ReportItemSold();
                        itemSold.InventoryItemID = item.InventoryItemID;
                        itemSold.ItemType = item.ItemType;
                        itemSold.Name = item.Name;
                        itemSold.Description = "";
                        itemSold.QuantityPurchased = 0;
                        itemSold.CostPerItem = item.Cost / item.Quantity;
                        itemSold.CostCurrency = new Currency()
                        {
                            Symbol = item.CostCurrencySymbol,
                            ConversionRateToUSD = item.CostCurrencyConversionRate
                        };
                        itemSold.TotalCost = 0;
                        itemSold.ProfitPerItem = item.Profit / item.Quantity;
                        itemSold.ProfitCurrency = new Currency()
                        {
                            Symbol = item.ProfitCurrencySymbol,
                            ConversionRateToUSD = item.ProfitCurrencyConversionRate
                        };
                        itemSold.TotalProfit = 0;
                        itemIDToReportSold[item.InventoryItemID] = itemSold;
                        totalDaySaleInfo.ItemsSold.Add(itemSold);
                    }
                    ReportItemSold itemSoldData = itemIDToReportSold[item.InventoryItemID];
                    itemSoldData.QuantityPurchased += item.Quantity;
                    totalDaySaleInfo.TotalItemsSold += item.Quantity;
                    itemSoldData.TotalCost += item.Cost * item.Quantity; // I don't think this needs a currency conversion...
                    itemSoldData.TotalProfit += item.Profit;
                    // now add to total income/profit after finding item type money info
                    ItemTypeMoneyInfo moneyInfo = null;
                    if (item.Type != null)
                    {
                        if (!totalDaySaleInfo.ItemTypeIDToMoneyInfo.ContainsKey(item.ItemType.ID))
                        {
                            var itemTypeMoneyInfo = new ItemTypeMoneyInfo(item.ItemType);
                            itemTypeMoneyInfo.Currency = totalDaySaleInfo.Currency;
                            totalDaySaleInfo.ItemTypeIDToMoneyInfo[item.ItemType.ID] = itemTypeMoneyInfo;
                            totalDaySaleInfo.ItemTypeMoneyBreakdown.Add(itemTypeMoneyInfo);
                        }
                        moneyInfo = totalDaySaleInfo.ItemTypeIDToMoneyInfo[item.ItemType.ID];
                    }
                    if (moneyInfo != null)
                    {
                        moneyInfo.TotalItemsSold += item.Quantity;
                    }
                    var amountIncrease = Utilities.ConvertAmountWithRates(item.Cost * item.Quantity, item.CostCurrencyConversionRate,
                        totalDaySaleInfo.Currency.ConversionRateToUSD);
                    totalDaySaleInfo.TotalIncome += amountIncrease;
                    if (moneyInfo != null)
                    {
                        moneyInfo.TotalIncome += amountIncrease;
                    }
                    // calc profit
                    amountIncrease = Utilities.ConvertAmountWithRates(item.Profit, item.ProfitCurrencyConversionRate,
                        totalDaySaleInfo.Currency.ConversionRateToUSD);
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
