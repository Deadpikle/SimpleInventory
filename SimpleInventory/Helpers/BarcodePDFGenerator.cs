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
        private BitmapSource ConvertImageToBitmapImage(Image img)
        {
            using (var memory = new MemoryStream())
            {
                img.Save(memory, ImageFormat.Jpeg);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        // based on https://stackoverflow.com/a/24199315/3938401
        private Bitmap ResizeImage(Image image, int width, int height, int resolution = 96)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            destImage.SetResolution(resolution, resolution);
            return destImage;
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
        /// <returns>The number of barcodes generated</returns>
        public int GenerateBarcodes(string outputPath)
        {
            if (NumberOfPages > 0)
            {
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Inventory Barcodes";
                long barcodeToUse = GeneratedBarcode.GetLatestBarcodeNumber() + 1;
                var barcodesGenerated = new List<long>();
                for (int i = 0; i < NumberOfPages; i++)
                {
                    PdfPage page = document.AddPage();
                    page.Size = PageSize;

                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    XFont font = new XFont("Verdana", 20, XFontStyle.Bold);
                    XUnit yCoord = XUnit.FromInch(1); // pixels
                    gfx.DrawString("Inventory Barcodes", font, XBrushes.Black,
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
                                // make sure images are a good size based on DPI
                                // TODO: There has got to be a better way to make things fairly consistent across computers 
                                // with different DPI. This is ridiculous. I love WPF most of the time with its DPI
                                // help, but in this case.......ugh. Images come out a little blurry this way
                                // on computers with a non-192 DPI.
                                double ratioTo192 = (192 / image.VerticalResolution);
                                int resizeHeight = (int)(image.Height / ratioTo192);
                                int resizeWidth = (int)(image.Width / ratioTo192);
                                image = ResizeImage(image, resizeWidth, resizeHeight, (int)image.VerticalResolution);
                                // ok, now we can draw.
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
