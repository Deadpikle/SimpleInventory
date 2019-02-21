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
        public string UserName { get; set; }
        public string Explanation { get; set; }
        public bool WasAdjustedForStockPurchase { get; set; }

        public string FriendlyDateTime
        {
            get { return DateTimeChanged.ToString(Utilities.DateTimeToFriendlyFullDateTimeStringFormat()); }
        }

        public static void UpdateQuantity(int quantity, int itemID, int userID, string explanation, bool wasAdjustedForStockPurchase)
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "INSERT INTO QuantityAdjustments (AmountChanged, DateTimeChanged, InventoryItemID, AdjustedByUserID, " +
                        "Explanation, WasAdjustedForStockPurchase)" +
                        " VALUES (@amount, @dateTime, @itemID, @userID, @explanation, @wasAdjustedForStockPurchase);";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@amount", quantity);
                    command.Parameters.AddWithValue("@dateTime", DateTime.Now.ToString(Utilities.DateTimeToStringFormat()));
                    command.Parameters.AddWithValue("@itemID", itemID);
                    command.Parameters.AddWithValue("@userID", userID);
                    command.Parameters.AddWithValue("@explanation", explanation);
                    command.Parameters.AddWithValue("@wasAdjustedForStockPurchase", wasAdjustedForStockPurchase);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void SaveUpdates()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "UPDATE QuantityAdjustments SET Explanation = @explanation, WasAdjustedForStockPurchase = @wasAdjustedForStockPurchase " +
                        " WHERE ID = @id";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@id", ID);
                    command.Parameters.AddWithValue("@explanation", Explanation);
                    command.Parameters.AddWithValue("@wasAdjustedForStockPurchase", WasAdjustedForStockPurchase);
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
                "SELECT qa.ID, AmountChanged, DateTimeChanged, AdjustedByUserID, u.Name AS UserName, Explanation, WasAdjustedForStockPurchase " +
                "FROM QuantityAdjustments qa JOIN Users u ON qa.AdjustedByUserID = u.ID " +
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
                            adjustment.UserName = dbHelper.ReadString(reader, "UserName");
                            adjustment.Explanation = dbHelper.ReadString(reader, "Explanation");
                            adjustment.WasAdjustedForStockPurchase = dbHelper.ReadBool(reader, "WasAdjustedForStockPurchase");
                            adjustments.Add(adjustment);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return adjustments;
        }

        public static List<QuantityAdjustment> LoadQuantityAdjustmentsBetweenDates(InventoryItem item, DateTime startTime, DateTime endTime)
        {
            var adjustments = new List<QuantityAdjustment>();
            if (item == null)
            {
                return adjustments;
            }
            string query = "" +
                "SELECT qa.ID, AmountChanged, DateTimeChanged, AdjustedByUserID, u.Name AS UserName, Explanation, WasAdjustedForStockPurchase " +
                "FROM QuantityAdjustments qa JOIN Users u ON qa.AdjustedByUserID = u.ID " +
                "WHERE InventoryItemID = @id AND DateTimeChanged >= @startDate AND DateTimeChanged <= @endDate " +
                "ORDER BY DateTimeChanged";
            var currencies = Currency.GetKeyValueCurrencyList();
            var dbHelper = new DatabaseHelper();
            var startDateForQueryString = startTime.ToString(Utilities.DateTimeToStringFormat());
            var endDateForQueryString = endTime.ToString(Utilities.DateTimeToStringFormat());
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@id", item.ID);
                    command.Parameters.AddWithValue("@startDate", startDateForQueryString);
                    command.Parameters.AddWithValue("@endDate", endDateForQueryString);
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
                            adjustment.UserName = dbHelper.ReadString(reader, "UserName");
                            adjustment.Explanation = dbHelper.ReadString(reader, "Explanation");
                            adjustment.WasAdjustedForStockPurchase = dbHelper.ReadBool(reader, "WasAdjustedForStockPurchase");
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
