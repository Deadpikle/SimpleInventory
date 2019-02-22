using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class DetailedStockReportInfo
    {
        public InventoryItem Item { get; set; }
        public int StartStock { get; set; }
        public int EndStock { get; set; }
        public int AmountChangedFromPurchaseStockIncrease { get; set; }
        public int AmountFromOtherQuantityAdjustments { get; set; }

        public int DifferenceInStockWithoutPurchaseStockIncrease
        {
            get { return EndStock - StartStock; }
        }

        public int StartStockWithPurchaseStockIncrease
        {
            get { return StartStock + AmountChangedFromPurchaseStockIncrease; }
        }

        public int DifferenceInStockWithPurchaseStockIncrease
        {
            get { return EndStock - StartStockWithPurchaseStockIncrease; }
        }
    }
}
