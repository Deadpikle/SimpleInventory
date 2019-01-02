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
    // class for generating a PDF of X number of barcodes via PDFSharp and BarcodeLib
    class BarcodePDFGenerator
    {
        public BarcodePDFGenerator()
        {
            IsDryRun = false;
            BarcodeType = BarcodeLib.TYPE.CODE128;
            PageSize = PdfSharp.PageSize.A4;
            NumberOfPages = 1;
        }

        // http://james-ramsden.com/c-convert-image-bitmapimage/
        private BitmapImage ConvertImageToBitmapImage(Image img)
        {
            using (var memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        /// <summary>
        /// Defaults to false.
        /// If true, does not save anything to disk on barcode generate or update the database.
        /// Use for figuring out how many barcodes will be generated ahead of time.
        /// </summary>
        public bool IsDryRun { get; set; }

        /// <summary>
        /// Defaults to BarcodeLib.TYPE.CODE128
        /// </summary>
        public BarcodeLib.TYPE BarcodeType { get; set; }

        /// <summary>
        /// Defaults to PdfSharp.PageSize.A4
        /// </summary>
        public PdfSharp.PageSize PageSize { get; set; }

        /// <summary>
        /// Defaults to 1
        /// </summary>
        public int NumberOfPages { get; set; }

        /// <summary>
        /// Generates a barcode PDF and returns the number of barcodes generated
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="numberOfPages"></param>
        /// <returns></returns>
        public int GenerateBarcodes(string outputPath)
        {
            if (NumberOfPages > 0)
            {
                PdfDocument document = new PdfDocument();
                document.Info.Title = "SimpleInventory Barcodes";
                long barcodeToUse = GeneratedBarcode.GetLatestBarcodeNumber() + 1;
                var barcodesGenerated = new List<long>();
                for (int i = 0; i < NumberOfPages; i++)
                {
                    PdfPage page = document.AddPage();
                    page.Size = PageSize; // TODO: allow for A4 or 8.5x11

                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    XFont font = new XFont("Verdana", 20, XFontStyle.Bold);
                    XUnit yCoord = XUnit.FromInch(1); // pixels
                    gfx.DrawString("SimpleInventory Barcodes", font, XBrushes.Black,
                        new XRect(0, yCoord, page.Width, page.Height), XStringFormats.TopCenter);

                    yCoord += XUnit.FromInch(0.7);

                    // Generate a barcode
                    var barcodeCreator = new BarcodeLib.Barcode();
                    barcodeCreator.ImageFormat = ImageFormat.Jpeg;
                    barcodeCreator.IncludeLabel = true;
                    barcodeCreator.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER;
                    barcodeCreator.Alignment = BarcodeLib.AlignmentPositions.CENTER;

                    bool isPageFull = false;
                    XUnit imageHeight = XUnit.FromPoint(60);
                    while (!isPageFull)
                    {
                        var isWidthFull = false;
                        XUnit xCoord = XUnit.FromInch(1);
                        while (!isWidthFull)
                        {
                            var image = barcodeCreator.Encode(BarcodeType, barcodeToUse.ToString());
                            if (image != null)
                            {
                                XImage pdfImage = XImage.FromBitmapSource(ConvertImageToBitmapImage(image));
                                gfx.DrawImage(pdfImage, xCoord, yCoord);
                                xCoord += XUnit.FromPoint(pdfImage.PointWidth);
                                imageHeight = XUnit.FromPoint(pdfImage.PointHeight);
                                var blah = XUnit.FromPoint(image.Width);
                                XUnit spaceBetweenBarcodes = XUnit.FromInch(0.75);
                                if (xCoord + XUnit.FromPoint(pdfImage.PointWidth) + spaceBetweenBarcodes > page.Width - XUnit.FromInch(1))
                                {
                                    isWidthFull = true;
                                }
                                barcodesGenerated.Add(barcodeToUse);
                                barcodeToUse++;
                                xCoord += spaceBetweenBarcodes;
                            }
                            else
                            {
                                // failure case
                                isWidthFull = true;
                                isPageFull = true;
                                break;
                            }
                        }
                        yCoord += imageHeight;
                        yCoord += XUnit.FromInch(0.7);
                        if (yCoord + imageHeight > page.Height - XUnit.FromInch(1))
                        {
                            isPageFull = true;
                        }
                    }
                }
                if (!IsDryRun)
                {
                    // save the fact that we generated barcodes
                    GeneratedBarcode.AddGeneratedCodes(barcodesGenerated, DateTime.Now, 1);
                    // save the document and start the process for viewing the pdf
                    document.Save(outputPath);
                    Process.Start(outputPath);
                }
                return barcodesGenerated.Count;
            }
            return 0;
        }
    }
}
