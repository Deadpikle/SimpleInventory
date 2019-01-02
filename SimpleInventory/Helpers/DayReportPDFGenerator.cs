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

namespace SimpleInventory.Helpers
{
    class DayReportPDFGenerator
    {
        private void AddTitle(DaySales sales, XUnit margin, PdfPage page, XGraphics gfx)
        {
            XUnit yCoord = margin;
            XFont font = new XFont("Verdana", 20, XFontStyle.Bold);
            gfx.DrawString("Daily Inventory Report", font, XBrushes.Black,
                new XRect(0, yCoord, page.Width, page.Height), XStringFormats.TopCenter);
            gfx.DrawString(sales.Date.ToString("d MMMM, yyyy"), font, XBrushes.Black,
                new XRect(0, yCoord + XUnit.FromInch(0.4), page.Width, page.Height), XStringFormats.TopCenter);
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

        public void GeneratePDF(DaySales sales, string outputPath)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Daily Inventory Report -- " + sales.Date.ToString("yyyy-MM-dd");

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

            //for (int i = 0; i < 40; i++) // for creating lots of PDF dummy data
            //{
                foreach (ReportItemSold itemSold in sales.ItemsSold)
                {
                    if (yCoord + XUnit.FromInch(0.4) >= page.Height - margin)
                    {
                        page = document.AddPage();
                        page.Size = PdfSharp.PageSize.A4;
                        gfx = XGraphics.FromPdfPage(page);
                        AddTitle(sales, margin, page, gfx);
                        DrawPageNumber(++pageNumber, margin, page, gfx);
                        yCoord = margin + XUnit.FromInch(1.3);
                        DrawHeaders(yCoord, margin, gfx);
                    }
                    yCoord += XUnit.FromInch(0.3);
                    xCoord = margin;
                    // these could be centered nicely or something, but *shrug*
                    gfx.DrawString(itemSold.Name, itemFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
                    xCoord += XUnit.FromInch(2.5);
                    gfx.DrawString(itemSold.QuantityPurchased.ToString(), itemFont, XBrushes.Black, 
                        new XPoint(xCoord + XUnit.FromInch(0.65), yCoord), XStringFormats.CenterRight);
                    xCoord += XUnit.FromInch(1.5);
                    gfx.DrawString(itemSold.TotalCostWithCurrency, itemFont, XBrushes.Black, 
                        new XPoint(xCoord + XUnit.FromInch(0.85), yCoord), XStringFormats.CenterRight);
                    xCoord += XUnit.FromInch(1.5);
                    gfx.DrawString(itemSold.TotalProfitWithCurrency, itemFont, XBrushes.Black, 
                        new XPoint(page.Width - margin - XUnit.FromInch(0.05), yCoord), XStringFormats.CenterRight);

                    XUnit yCoordForLine = +XUnit.FromInch(0.15);
                    gfx.DrawLine(XPens.Black, margin, yCoord + yCoordForLine, page.Width - margin, yCoord + yCoordForLine);
                }
            //}
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
            gfx.DrawString(sales.TotalItemsSold.ToString(), totalDataFont, XBrushes.Black,
                        new XPoint(xCoord + XUnit.FromInch(0.65), yCoord), XStringFormats.CenterRight);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString(sales.TotalIncomeWithCurrency, totalDataFont, XBrushes.Black,
                        new XPoint(xCoord + XUnit.FromInch(0.85), yCoord), XStringFormats.CenterRight);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString(sales.TotalProfitWithCurrency, totalDataFont, XBrushes.Black,
                        new XPoint(page.Width - margin - XUnit.FromInch(0.05), yCoord), XStringFormats.CenterRight);


            // save the document and start the process for viewing the pdf
            document.Save(outputPath);
            Process.Start(outputPath);
        }
    }
}
