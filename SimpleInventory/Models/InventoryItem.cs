using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

        public static List<InventoryItem> LoadItems(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var items = new List<InventoryItem>();
            string query = "" +
                "SELECT ii.ID, ii.Name, Description, PicturePath, CostDollars, CostDollars, CostRiel, Quantity, BarcodeNumber, CreatedByUserID " +
                "FROM InventoryItems ii LEFT JOIN Users u ON ii.CreatedByUserID = u.ID " +
                (string.IsNullOrEmpty(whereClause) ? "" : whereClause) + " " +
                "ORDER BY ii.Name, CostRiel, Description, CostRiel, CostDollars";

            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    command.CommandText = query;
                    if (!string.IsNullOrEmpty(whereClause) && whereParams != null)
                    {
                        foreach (Tuple<string, string> keyValuePair in whereParams)
                        {
                            command.Parameters.AddWithValue(keyValuePair.Item1, keyValuePair.Item2);
                        }
                    }
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new InventoryItem();
                            item.ID = dbHelper.ReadInt(reader, "ID");
                            item.Name = dbHelper.ReadString(reader, "Name");
                            item.Description = dbHelper.ReadString(reader, "Description");
                            item.PicturePath = dbHelper.ReadString(reader, "PicturePath");
                            item.CostDollars = dbHelper.ReadDecimal(reader, "CostDollars");
                            item.CostRiel = dbHelper.ReadInt(reader, "CostRiel");
                            item.Quantity = dbHelper.ReadInt(reader, "Quantity");
                            item.BarcodeNumber = dbHelper.ReadString(reader, "BarcodeNumber");
                            item.CreatedByUserID = dbHelper.ReadInt(reader, "CreatedByUserID");
                            items.Add(item);
                        }
                    }
                }
            }
            return items;
        }

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
