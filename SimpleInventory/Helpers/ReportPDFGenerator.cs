using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SimpleInventory.Models;
using SimpleInventory.Interfaces;

namespace SimpleInventory.Helpers
{
    class ReportPDFGenerator
    {
        private void AddTitle(IItemsSoldReportData sales, XUnit margin, PdfPage page, XGraphics gfx)
        {
            XUnit yCoord = margin;
            XFont font = new XFont("Verdana", 20, XFontStyle.Bold);
            var titleWord = sales.IsDailyReport() ? "Daily" : "Weekly";
            gfx.DrawString(titleWord + " Inventory Sold Report", font, XBrushes.Black,
                new XRect(0, yCoord, page.Width, page.Height), XStringFormats.TopCenter);
            if (sales.IsDailyReport())
            {
                gfx.DrawString(sales.GetDate().ToString("d MMMM, yyyy"), font, XBrushes.Black,
                    new XRect(0, yCoord + XUnit.FromInch(0.4), page.Width, page.Height), XStringFormats.TopCenter);
            }
            else
            {
                XFont smallerTitleFont = new XFont("Verdana", 16, XFontStyle.Bold);
                gfx.DrawString(sales.GetDate().ToString("d MMMM, yyyy") + " - " +  sales.GetDate().AddDays(6).ToString("d MMMM, yyyy"), 
                    smallerTitleFont, XBrushes.Black,
                    new XRect(0, yCoord + XUnit.FromInch(0.4), page.Width, page.Height), XStringFormats.TopCenter);
            }
        }

        private void DrawHeaders(XUnit yCoord, XUnit margin, XGraphics gfx)
        {
            XUnit xCoord = margin;
            XFont headerFont = new XFont("Verdana", 16, XFontStyle.Bold);
            gfx.DrawString("Name", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(2.5);
            gfx.DrawString("Quantity", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString("Income", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString("Profit", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
        }

        private void DrawPageNumber(int number, XUnit margin, PdfPage page, XGraphics gfx)
        {
            XFont headerFont = new XFont("Verdana", 10, XFontStyle.Regular);
            gfx.DrawString("-" + number.ToString() + "-", headerFont, XBrushes.Black, new XRect(0, page.Height - margin, page.Width, margin), XStringFormats.Center);
        }

        public void GeneratePDF(IItemsSoldReportData sales, string outputPath)
        {
            PdfDocument document = new PdfDocument();
            var titleWord = sales.IsDailyReport() ? "Daily" : "Weekly";
            document.Info.Title = titleWord + " Inventory Sold Report -- " + sales.GetDate().ToString("yyyy-MM-dd");

            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            XUnit margin = XUnit.FromInch(1);
            XGraphics gfx = XGraphics.FromPdfPage(page);
            AddTitle(sales, margin, page, gfx);
            int pageNumber = 1;
            DrawPageNumber(pageNumber, margin, page, gfx);

            XUnit yCoord = margin + XUnit.FromInch(1.3);

            // draw headers
            DrawHeaders(yCoord, margin, gfx);

            XUnit xCoord = margin;
            XFont itemFont = new XFont("Segoe UI", 14, XFontStyle.Regular);
            XFont itemDescriptionFont = new XFont("Segoe UI", 12, XFontStyle.Regular);

            //for (int i = 0; i < 40; i++) // for creating lots of PDF dummy data
            //{
            foreach (ReportItemSold itemSold in sales.GetItemsSold())
                {
                    if (yCoord + XUnit.FromInch(0.6) >= page.Height - margin)
                    {
                        page = document.AddPage();
                        page.Size = PdfSharp.PageSize.A4;
                        gfx = XGraphics.FromPdfPage(page);
                        AddTitle(sales, margin, page, gfx);
                        DrawPageNumber(++pageNumber, margin, page, gfx);
                        yCoord = margin + XUnit.FromInch(1.3);
                        DrawHeaders(yCoord, margin, gfx);
                    }
                    yCoord += XUnit.FromInch(0.5);
                    xCoord = margin;
                    // these could be centered nicely or something, but *shrug*
                    if (string.IsNullOrWhiteSpace(itemSold.Description))
                    {
                        gfx.DrawString(itemSold.Name, itemFont, XBrushes.Black, 
                            new XRect(xCoord, yCoord, XUnit.FromInch(0), XUnit.FromInch(0.25)), XStringFormats.CenterLeft);
                    }
                    else
                    {
                        gfx.DrawString(itemSold.Name, itemFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
                        gfx.DrawString(itemSold.Description, itemDescriptionFont, XBrushes.Black,
                                new XPoint(xCoord, yCoord + XUnit.FromInch(0.25)), XStringFormats.CenterLeft);
                    }
                    xCoord += XUnit.FromInch(2.5);
                    gfx.DrawString(itemSold.QuantityPurchased.ToString(), itemFont, XBrushes.Black,
                        new XRect(xCoord + XUnit.FromInch(0.65), yCoord, XUnit.FromInch(0), XUnit.FromInch(.25)), XStringFormats.CenterRight);
                        //new XPoint(xCoord + XUnit.FromInch(0.65), yCoord), XStringFormats.CenterRight);
                    xCoord += XUnit.FromInch(1.5);
                    gfx.DrawString(itemSold.TotalCostWithCurrency, itemFont, XBrushes.Black, 
                        //new XPoint(xCoord + XUnit.FromInch(0.85), yCoord), XStringFormats.CenterRight);
                        new XRect(xCoord + XUnit.FromInch(0.85), yCoord, XUnit.FromInch(0), XUnit.FromInch(.25)), XStringFormats.CenterRight);
                    xCoord += XUnit.FromInch(1.5);
                    gfx.DrawString(itemSold.TotalProfitWithCurrency, itemFont, XBrushes.Black, 
                        //new XPoint(page.Width - margin - XUnit.FromInch(0.05), yCoord), XStringFormats.CenterRight);
                        new XRect(page.Width - margin - XUnit.FromInch(0.05), yCoord, XUnit.FromInch(0), XUnit.FromInch(.25)), XStringFormats.CenterRight);

                    XUnit yCoordForLine = XUnit.FromInch(0.38);
                    gfx.DrawLine(XPens.Black, margin, yCoord + yCoordForLine, page.Width - margin, yCoord + yCoordForLine);
                }
            //}
            yCoord += XUnit.FromInch(0.15);
            // print category totals
            var itemTypeMoneyInfoList = sales.GetItemTypeMoneyInfo();
            foreach (var moneyInfo in itemTypeMoneyInfoList)
            {
                if (yCoord + XUnit.FromInch(0.5) >= page.Height - margin)
                {
                    // GOTTA AADDDDDD A NEWWWW PAGEEE....
                    page = document.AddPage();
                    page.Size = PdfSharp.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    AddTitle(sales, margin, page, gfx);
                    DrawPageNumber(++pageNumber, margin, page, gfx);
                    yCoord = margin + XUnit.FromInch(1.3);
                    DrawHeaders(yCoord, margin, gfx);
                }
                yCoord += XUnit.FromInch(0.5);
                XFont totalCategoryFont = new XFont("Segoe UI", 14, XFontStyle.Bold);
                XFont totalCategoryDataFont = new XFont("Segoe UI", 14, XFontStyle.Bold);
                xCoord = margin;
                gfx.DrawString(moneyInfo.Type.Name + " total", totalCategoryFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
                xCoord += XUnit.FromInch(2.5);
                gfx.DrawString(moneyInfo.TotalItemsSold.ToString(), totalCategoryDataFont, XBrushes.Black,
                            new XPoint(xCoord + XUnit.FromInch(0.65), yCoord), XStringFormats.CenterRight);
                xCoord += XUnit.FromInch(1.5);
                gfx.DrawString(moneyInfo.TotalIncomeWithCurrency.ToString(), totalCategoryDataFont, XBrushes.Black,
                            new XPoint(xCoord + XUnit.FromInch(0.85), yCoord), XStringFormats.CenterRight);
                xCoord += XUnit.FromInch(1.5);
                gfx.DrawString(moneyInfo.TotalProfitWithCurrency.ToString(), totalCategoryDataFont, XBrushes.Black,
                            new XPoint(page.Width - margin - XUnit.FromInch(0.05), yCoord), XStringFormats.CenterRight);
            }
            // print totals
            if (yCoord + XUnit.FromInch(0.5) >= page.Height - margin)
            {
                // GOTTA AADDDDDD A NEWWWW PAGEEE....
                page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                gfx = XGraphics.FromPdfPage(page);
                AddTitle(sales, margin, page, gfx);
                DrawPageNumber(++pageNumber, margin, page, gfx);
                yCoord = margin + XUnit.FromInch(1.3);
                DrawHeaders(yCoord, margin, gfx);
            }
            yCoord += XUnit.FromInch(0.5);
            XFont totalFont = new XFont("Segoe UI", 16, XFontStyle.Bold);
            XFont totalDataFont = new XFont("Segoe UI", 14, XFontStyle.Bold);
            xCoord = margin;
            gfx.DrawString("TOTAL", totalFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(2.5);
            gfx.DrawString(sales.GetTotalItemsSold().ToString(), totalDataFont, XBrushes.Black,
                        new XPoint(xCoord + XUnit.FromInch(0.65), yCoord), XStringFormats.CenterRight);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString(sales.GetTotalIncomeWithCurrency(), totalDataFont, XBrushes.Black,
                        new XPoint(xCoord + XUnit.FromInch(0.85), yCoord), XStringFormats.CenterRight);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString(sales.GetTotalProfitWithCurrency(), totalDataFont, XBrushes.Black,
                        new XPoint(page.Width - margin - XUnit.FromInch(0.05), yCoord), XStringFormats.CenterRight);


            // save the document and start the process for viewing the pdf
            document.Save(outputPath);
            Process.Start(outputPath);
        }
    }
}
