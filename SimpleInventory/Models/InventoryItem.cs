using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInventory.Helpers;

namespace SimpleInventory.Models
{
    class InventoryItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PicturePath { get; set; }
        public int CreatedByUserID { get; set; }

        public decimal CostDollars { get; set; }
        public int CostRiel { get; set; }
        
        public int Quantity { get; set; }

        /// BarcodeNumber is a string just in case we need to change from #'s later or it's really long or something
        public string BarcodeNumber { get; set; }
        public bool WasDeleted { get; set; }

        public void CreateNewItem(int userID)
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "INSERT INTO InventoryItems (Name, Description, PicturePath, CostDollars, " +
                        "CostRiel, Quantity, BarcodeNumber, CreatedByUserID) VALUES " +
                        "(@name, @description, @picturePath, @costDollars, @costRiel, @quantity, @barcodeNumber, @createdByUserID)";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@description", Description);
                    command.Parameters.AddWithValue("@picturePath", PicturePath);
                    command.Parameters.AddWithValue("@costDollars", CostDollars.ToString());
                    command.Parameters.AddWithValue("@costRiel", CostRiel);
                    command.Parameters.AddWithValue("@quantity", Quantity);
                    command.Parameters.AddWithValue("@barcodeNumber", BarcodeNumber);
                    command.Parameters.AddWithValue("@createdByUserID", userID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void SaveItemUpdates(int userID)
        {

        }
    }
}
