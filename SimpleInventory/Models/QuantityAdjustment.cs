using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class QuantityAdjustment
    {
        public int ID { get; set; }
        public int AmountChanged { get; set; }
        public DateTime DateTimeChanged { get; set; }
        public int InventoryItemID { get; set; }
        public int UserID { get; set; }

        public string FriendlyDateTime
        {
            get { return DateTimeChanged.ToString(Utilities.DateTimeToFriendlyStringFormat()); }
        }

        public static void UpdateQuantity(int quantity, int itemID, int userID)
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "INSERT INTO QuantityAdjustments (AmountChanged, DateTimeChanged, InventoryItemID, AdjustedByUserID)" +
                        " VALUES (@amount, @dateTime, @itemID, @userID);";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@amount", quantity);
                    command.Parameters.AddWithValue("@dateTime", DateTime.Now.ToString(Utilities.DateTimeToStringFormat()));
                    command.Parameters.AddWithValue("@itemID", itemID);
                    command.Parameters.AddWithValue("@userID", userID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public static List<QuantityAdjustment> LoadQuantityAdjustments(InventoryItem item)
        {
            var adjustments = new List<QuantityAdjustment>();
            if (item == null)
            {
                return adjustments;
            }
            string query = "" +
                "SELECT ID, AmountChanged, DateTimeChanged, AdjustedByUserID " +
                "FROM QuantityAdjustments " +
                "WHERE InventoryItemID = @id " +
                "ORDER BY DateTimeChanged";
            var currencies = Currency.GetKeyValueCurrencyList();
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@id", item.ID);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var adjustment = new QuantityAdjustment();
                            adjustment.ID = dbHelper.ReadInt(reader, "ID");
                            adjustment.AmountChanged = dbHelper.ReadInt(reader, "AmountChanged");
                            adjustment.DateTimeChanged = Convert.ToDateTime(dbHelper.ReadString(reader, "DateTimeChanged"));
                            adjustment.UserID = dbHelper.ReadInt(reader, "AdjustedByUserID");
                            adjustment.InventoryItemID = item.ID;
                            adjustments.Add(adjustment);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return adjustments;
        }
    }
}
