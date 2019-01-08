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
        public decimal TotalDrinkIncome { get; set; }
        public decimal TotalDrinkProfit { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalProfit { get; set; }
        public Currency Currency { get; set; }
        public int TotalItemsSold { get; set; }

        public List<ReportItemSold> AllItemsSold { get; private set; }

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

        public string TotalDrinkIncomeWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return TotalDrinkIncome.ToString() + " (" + Currency?.Symbol + ")";
                }
                return TotalDrinkIncome.ToString();
            }
        }

        public string TotalDrinkProfitWithCurrency
        {
            get
            {
                if (Currency != null)
                {
                    return TotalDrinkProfit.ToString() + " (" + Currency?.Symbol + ")";
                }
                return TotalDrinkProfit.ToString();
            }
        }

        public WeekSales()
        {
            AllDaySales = new List<DaySales>();
            AllItemsSold = new List<ReportItemSold>();
        }

        public static WeekSales GenerateDataForWeek(DateTime date)
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
                DaySales sales = DaySales.GenerateDataForSingleDay(date.AddDays(i));
                weekSales.AllDaySales.Add(sales);
                if (weekSales.Currency.ID == sales.Currency.ID)
                {
                    weekSales.TotalIncome += sales.TotalIncome;
                    weekSales.TotalDrinkIncome += sales.TotalDrinkIncome;
                }
                else
                {
                    weekSales.TotalIncome += Utilities.ConvertAmount(sales.TotalIncome, sales.Currency, weekSales.Currency);
                    weekSales.TotalDrinkIncome += Utilities.ConvertAmount(sales.TotalDrinkIncome, sales.Currency, weekSales.Currency);
                }

                if (weekSales.Currency.ID == sales.Currency.ID)
                {
                    weekSales.TotalProfit += sales.TotalProfit;
                    weekSales.TotalDrinkProfit += sales.TotalDrinkIncome;
                }
                else
                {
                    weekSales.TotalProfit += Utilities.ConvertAmount(sales.TotalProfit, sales.Currency, weekSales.Currency);
                    weekSales.TotalDrinkProfit += Utilities.ConvertAmount(sales.TotalDrinkProfit, sales.Currency, weekSales.Currency);
                }
                weekSales.TotalItemsSold += sales.TotalItemsSold;
                allItemsSoldReports.AddRange(sales.ItemsSold);
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
            // need to sort allItemsSoldData by name :)
            allItemsSoldReports.Sort((left, right) => left.Name.CompareTo(right.Name));
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

        public string GetTotalDrinkIncomeWithCurrency()
        {
            return TotalDrinkIncomeWithCurrency;
        }

        public string GetTotalDrinkProfitWithCurrency()
        {
            return TotalDrinkProfitWithCurrency;
        }

        #endregion
    }
}
