using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class QuantityAdjustment
    {
        int ID { get; set; }
        int AmountChanged { get; set; }
        DateTime DateTimeChanged { get; set; }
        int InventoryItemID { get; set; }
        int UserID { get; set; }

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
    }
}
