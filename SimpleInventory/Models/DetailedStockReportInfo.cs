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

        public int DifferenceInStock
        {
            get { return EndStock - StartStock; }
        }
    }
}
