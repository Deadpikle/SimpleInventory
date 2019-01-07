using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class WeekSales
    {
        public List<DaySales> AllDaySales { get; set; }
        public DateTime Date { get; set; }
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
                weekSales.TotalIncome += sales.TotalIncome; // TODO: CONCERN THYSELF WITH CURRENCIES!
                weekSales.TotalProfit += sales.TotalProfit; // TODO: CONCERN THYSELF WITH CURRENCIES!
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
            return weekSales;
        }
    }
}
