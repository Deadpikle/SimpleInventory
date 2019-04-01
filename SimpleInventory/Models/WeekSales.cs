using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class WeekSales : IItemsSoldReportData
    {
        public List<DaySales> AllDaySales { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalProfit { get; set; }
        public Currency Currency { get; set; }
        public int TotalItemsSold { get; set; }

        public List<ReportItemSold> AllItemsSold { get; private set; }

        public List<ItemTypeMoneyInfo> ItemTypeMoneyBreakdown { get; set; }
        public Dictionary<int, ItemTypeMoneyInfo> ItemTypeIDToMoneyInfo { get; private set; }

        public WeekSales()
        {
            ItemTypeMoneyBreakdown = new List<ItemTypeMoneyInfo>();
            ItemTypeIDToMoneyInfo = new Dictionary<int, ItemTypeMoneyInfo>();
            AllDaySales = new List<DaySales>();
            AllItemsSold = new List<ReportItemSold>();
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

        public static WeekSales GenerateDataForWeek(DateTime date, int userID = -1)
        {
            WeekSales weekSales = new WeekSales();
            weekSales.Date = date;
            var allItemsSoldReports = new List<ReportItemSold>();

            var currencies = Currency.LoadCurrencies();
            foreach (Currency currency in currencies)
            {
                if (currency.IsDefaultCurrency)
                {
                    weekSales.Currency = currency;
                    break;
                }
            }

            for (int i = 0; i < 7; i++) // get all sales for the week
            {
                DaySales sales = DaySales.GenerateDataForSingleDay(date.AddDays(i), userID);
                weekSales.AllDaySales.Add(sales);
                if (weekSales.Currency.ID == sales.Currency.ID)
                {
                    weekSales.TotalIncome += sales.TotalIncome;
                }
                else
                {
                    weekSales.TotalIncome += Utilities.ConvertAmount(sales.TotalIncome, sales.Currency, weekSales.Currency);
                }

                if (weekSales.Currency.ID == sales.Currency.ID)
                {
                    weekSales.TotalProfit += sales.TotalProfit;
                }
                else
                {
                    weekSales.TotalProfit += Utilities.ConvertAmount(sales.TotalProfit, sales.Currency, weekSales.Currency);
                }
                weekSales.TotalItemsSold += sales.TotalItemsSold;
                allItemsSoldReports.AddRange(sales.ItemsSold);
                // must add up item type category incomes & profits now
                foreach (ItemTypeMoneyInfo moneyInfo in sales.ItemTypeMoneyBreakdown)
                {
                    // if we don't have info on that item type already, create it
                    if (!weekSales.ItemTypeIDToMoneyInfo.ContainsKey(moneyInfo.Type.ID))
                    {
                        var createdMoneyInfo = new ItemTypeMoneyInfo(moneyInfo.Type);
                        createdMoneyInfo.Currency = weekSales.Currency;
                        weekSales.ItemTypeIDToMoneyInfo[moneyInfo.Type.ID] = createdMoneyInfo;
                        weekSales.ItemTypeMoneyBreakdown.Add(createdMoneyInfo);
                    }
                    var moneyInfoToAdjust = weekSales.ItemTypeIDToMoneyInfo[moneyInfo.Type.ID];
                    moneyInfoToAdjust.TotalItemsSold += moneyInfo.TotalItemsSold;
                    // need to add in the income and profit
                    if (weekSales.Currency.ID == sales.Currency.ID)
                    {
                        moneyInfoToAdjust.TotalIncome += moneyInfo.TotalIncome;
                    }
                    else
                    {
                        moneyInfoToAdjust.TotalIncome += Utilities.ConvertAmount(moneyInfo.TotalIncome, sales.Currency, weekSales.Currency);
                    }
                    if (weekSales.Currency.ID == sales.Currency.ID)
                    {
                        moneyInfoToAdjust.TotalProfit += moneyInfo.TotalProfit;
                    }
                    else
                    {
                        moneyInfoToAdjust.TotalProfit += Utilities.ConvertAmount(moneyInfo.TotalProfit, sales.Currency, weekSales.Currency);
                    }
                }
            }
            // now we need to set up the AllItemsSold array
            var itemIDToReportSold = new Dictionary<int, ReportItemSold>();
            foreach (ReportItemSold singleItemSoldReport in allItemsSoldReports)
            {
                if (!itemIDToReportSold.ContainsKey(singleItemSoldReport.InventoryItemID))
                {
                    itemIDToReportSold[singleItemSoldReport.InventoryItemID] = singleItemSoldReport;
                    weekSales.AllItemsSold.Add(singleItemSoldReport);
                }
                else
                {
                    ReportItemSold allItemsSoldData = itemIDToReportSold[singleItemSoldReport.InventoryItemID];
                    allItemsSoldData.QuantityPurchased += singleItemSoldReport.QuantityPurchased;
                    if (allItemsSoldData.CostCurrency.ID == singleItemSoldReport.CostCurrency.ID)
                    {
                        allItemsSoldData.TotalCost += singleItemSoldReport.QuantityPurchased * singleItemSoldReport.CostPerItem;
                    }
                    else
                    {
                        allItemsSoldData.TotalCost += singleItemSoldReport.QuantityPurchased *
                            Utilities.ConvertAmount(singleItemSoldReport.CostPerItem, singleItemSoldReport.CostCurrency, allItemsSoldData.CostCurrency);
                    }
                    if (allItemsSoldData.ProfitCurrency.ID == singleItemSoldReport.ProfitCurrency.ID)
                    {
                        allItemsSoldData.TotalProfit += singleItemSoldReport.QuantityPurchased * singleItemSoldReport.ProfitPerItem;
                    }
                    else
                    {
                        allItemsSoldData.TotalProfit += singleItemSoldReport.QuantityPurchased *
                            Utilities.ConvertAmount(singleItemSoldReport.ProfitPerItem, singleItemSoldReport.ProfitCurrency, allItemsSoldData.ProfitCurrency);
                    }
                }
            }
            // sort final arrays for nice display
            weekSales.AllItemsSold.Sort((left, right) => left.Name.ToLower().CompareTo(right.Name.ToLower()));
            weekSales.ItemTypeMoneyBreakdown.Sort((left, right) => left.Type.Name.ToLower().CompareTo(right.Type.Name.ToLower()));
            return weekSales;
        }

        #region IItemsSoldReportData

        public DateTime GetDate()
        {
            return Date;
        }

        public List<ReportItemSold> GetItemsSold()
        {
            return AllItemsSold;
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
            return false;
        }

        public List<ItemTypeMoneyInfo> GetItemTypeMoneyInfo()
        {
            return ItemTypeMoneyBreakdown;
        }

        #endregion
    }
}
