using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class UserPermissions
    {
        public int ID { get; set; }
        public int UserID { get; set; }

        public bool CanAddEditItems { get; set; }
        public bool CanAdjustItemQuantity { get; set; }
        public bool CanViewDetailedItemQuantityAdjustments { get; set; }
        public bool CanScanItems { get; set; }
        public bool CanGenerateBarcodes { get; set; }
        public bool CanViewReports { get; set; }
        public bool CanViewDetailedItemSoldInfo { get; set; }
        public bool CanSaveReportsToPDF { get; set; }
        public bool CanDeleteItemsFromInventory { get; set; }
        public bool CanManageItemCategories { get; set; }
        public bool CanManageUsers { get; set; }
        public bool CanDeleteItemsSold { get; set; }
        public bool CanViewManageInventoryQuantity { get; set; }

        public UserPermissions()
        {
            ID = -1;
            UserID = -1;

            CanAddEditItems = false;
            CanAdjustItemQuantity = false;
            CanViewDetailedItemQuantityAdjustments = false;
            CanScanItems = false;
            CanGenerateBarcodes = false;
            CanViewReports = false;
            CanViewDetailedItemSoldInfo = false;
            CanSaveReportsToPDF = false;
            CanDeleteItemsFromInventory = false;
            CanManageItemCategories = false;
            CanManageUsers = false;
            CanDeleteItemsSold = false;
            CanViewManageInventoryQuantity = false;
        }
    }
}
