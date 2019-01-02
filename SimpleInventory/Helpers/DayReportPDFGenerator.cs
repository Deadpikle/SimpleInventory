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
            document.Info.Title = "SimpleInventory Daily Report -- " + sales.Date.ToString("yyyy-MM-dd");


            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Verdana", 20, XFontStyle.Bold);
            XUnit yCoord = XUnit.FromInch(1); // pixels
            gfx.DrawString("SimpleInventory Daily Report", font, XBrushes.Black,
                new XRect(0, yCoord, page.Width, page.Height), XStringFormats.TopCenter);
            gfx.DrawString(sales.Date.ToString("yyyy-MM-dd"), font, XBrushes.Black,
                new XRect(0, yCoord + XUnit.FromInch(0.4), page.Width, page.Height), XStringFormats.TopCenter);

            yCoord += XUnit.FromInch(1);

            // draw headers
            XUnit xCoord = XUnit.FromInch(1);
            XFont headerFont = new XFont("Verdana", 16, XFontStyle.Bold);
            gfx.DrawString("Name", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(2.5);
            gfx.DrawString("Quantity", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString("Income", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);
            xCoord += XUnit.FromInch(1.5);
            gfx.DrawString("Profit", headerFont, XBrushes.Black, new XPoint(xCoord, yCoord), XStringFormats.CenterLeft);

            // save the document and start the process for viewing the pdf
            document.Save(outputPath);
            Process.Start(outputPath);
        }
    }
}
