using ClosedXML.Excel;
using SimpleInventory.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SimpleInventory.Helpers
{
    class StockInfoExcelGenerator
    {

        public void ExportStockInfo(List<DetailedStockReportInfo> items, DateTime startDate, DateTime endDate, string path)
        {
            items.Sort((a, b) => (a.Item.Name + a.Item.Description).ToLower().CompareTo((b.Item.Name + b.Item.Description).ToLower()));
            var startDateString = startDate.ToString(Utilities.DateTimeToFriendlyJustDateStringFormat());
            var endDateString = endDate.ToString(Utilities.DateTimeToFriendlyJustDateStringFormat());
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Stock Info");
                worksheet.Cell("A1").Value = "SimpleInventory -- Stock Info Report for Sold Items";
                worksheet.Cell("A1").Style.Font.Bold = true;
                worksheet.Cell("A2").Value = startDateString + " - " + endDateString;

                // table headers
                worksheet.Cell("A4").SetValue("Name").Style.Font.SetBold(true);
                worksheet.Cell("B4").SetValue("Description").Style.Font.SetBold(true);
                worksheet.Cell("C4").SetValue("Beginning Stock (Computer)").Style.Font.SetBold(true);
                worksheet.Cell("D4").SetValue("Ending Stock (Computer)").Style.Font.SetBold(true);
                worksheet.Cell("E4").SetValue("Ending Stock (Manual Entry)").Style.Font.SetBold(true);
                worksheet.Cell("F4").SetValue("Computer Difference").Style.Font.SetBold(true);
                worksheet.Cell("G4").SetValue("Manual Difference").Style.Font.SetBold(true);
                worksheet.Cell("H4").SetValue("Stock Difference").Style.Font.SetBold(true);

                // start exporting data
                var currentCell = worksheet.Cell("A5");
                foreach (DetailedStockReportInfo item in items)
                {
                    currentCell.Value = item.Item.Name;
                    currentCell.CellRight(1).Value = item.Item.Description;
                    currentCell.CellRight(2).Value = item.StartStockWithPurchaseStockIncrease;
                    currentCell.CellRight(3).Value = item.EndStock; // computer
                    currentCell.CellRight(4).Value = ""; // manual entry
                    currentCell.CellRight(5).FormulaA1 = "=SUM(-" + currentCell.CellRight(2).Address.ToStringFixed() + "," 
                        + currentCell.CellRight(3).Address.ToStringFixed() + ")"; // computer diff
                    currentCell.CellRight(6).FormulaA1 = "=IF(" + currentCell.CellRight(3).Address.ToStringFixed() + "=\"\", \"-\", "
                            + "SUM(-" + currentCell.CellRight(2).Address.ToStringFixed() + ","
                                + currentCell.CellRight(4).Address.ToStringFixed() + "))"; // manual diff
                    // conditional formatting
                    currentCell.CellRight(5).AddConditionalFormat()
                        .WhenNotEquals("=" + currentCell.CellRight(6).Address.ToStringFixed())
                        .Fill.SetBackgroundColor(XLColor.LightPink);
                    currentCell.CellRight(6).AddConditionalFormat()
                        .WhenNotEquals("=" + currentCell.CellRight(5).Address.ToStringFixed())
                        .Fill.SetBackgroundColor(XLColor.LightPink);
                    currentCell.CellRight(7).SetFormulaA1("=ABS(SUM(" + currentCell.CellRight(5).Address.ToStringFixed() + ", -" 
                        + currentCell.CellRight(6).Address.ToStringFixed() + "))").AddConditionalFormat()
                        .WhenNotEquals("0")
                        .Fill.SetBackgroundColor(XLColor.LightPink);

                    // go to next row
                    currentCell = currentCell.CellBelow();
                }
                //// auto fit width
                worksheet.Columns().AdjustToContents(4, 4, 10, 25);

                workbook.SaveAs(path);
                Process.Start(path);
            }
        }
    }
}
