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
        public void GeneratePDF(DaySales sales, string outputPath)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Inventory Daily Report -- " + sales.Date.ToString("yyyy-MM-dd");


            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Verdana", 20, XFontStyle.Bold);
            XUnit margin = XUnit.FromInch(1);
            XUnit yCoord = margin;
            gfx.DrawString("Inventory Daily Report", font, XBrushes.Black,
                new XRect(0, yCoord, page.Width, page.Height), XStringFormats.TopCenter);
            gfx.DrawString(sales.Date.ToString("d MMMM, yyyy"), font, XBrushes.Black,
                new XRect(0, yCoord + XUnit.FromInch(0.4), page.Width, page.Height), XStringFormats.TopCenter);


            yCoord += XUnit.FromInch(1.3);

            // draw headers
            XUnit xCoord = margin;
            XFont headerFont = new XFont("Verdana", 16, XFontStyle.Bold);
            gfx.DrawString("Name", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(2.5);
            gfx.DrawString("Quantity", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString("Income", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString("Profit", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);

            XFont itemFont = new XFont("Segoe UI", 14, XFontStyle.Regular);

            foreach (ReportItemSold itemSold in sales.ItemsSold)
            {
                yCoord += XUnit.FromInch(0.3);
                xCoord = margin;
                gfx.DrawString(itemSold.Name, itemFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
                xCoord += XUnit.FromInch(2.5);
                gfx.DrawString(itemSold.QuantityPurchased.ToString(), itemFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
                xCoord += XUnit.FromInch(1.5);
                gfx.DrawString(itemSold.TotalCostWithCurrency, itemFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
                xCoord += XUnit.FromInch(1.5);
                gfx.DrawString(itemSold.TotalProfitWithCurrency, itemFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            }
            // print totals
            yCoord += XUnit.FromInch(0.5);
            XFont totalFont = new XFont("Segoe UI", 16, XFontStyle.Bold);
            XFont totalDataFont = new XFont("Segoe UI", 14, XFontStyle.Bold);
            xCoord = margin;
            gfx.DrawString("TOTAL", totalFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(2.5);
            gfx.DrawString(sales.TotalItemsSold.ToString(), totalDataFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString(sales.TotalIncomeWithCurrency, totalDataFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString(sales.TotalProfitWithCurrency, totalDataFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);


            // save the document and start the process for viewing the pdf
            document.Save(outputPath);
            Process.Start(outputPath);
        }
    }
}
