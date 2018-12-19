using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class ItemSoldInfo
    {
        public int ID { get; set; }
        public DateTime DateTimeSold { get; set; }
        public int QuantitySold { get; set; } // defaults to 1
        public int InventoryItemID { get; set; }
        public int SoldByUserID { get; set; }

        // we remember the cost of things in case it changes over time so we have the original info :)
        public decimal Cost { get; set; }
        public Currency CostCurrency { get; set; }
        public decimal Paid { get; set; }
        public Currency PaidCurrency { get; set; }
        public decimal Change { get; set; }
        public Currency ChangeCurrency { get; set; }
        public decimal ProfitPerItem { get; set; }
        public Currency ProfitPerItemCurrency { get; set; }


        public void CreateNewSoldInfo()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "INSERT INTO ItemsSoldInfo (DateTimeSold, QuantitySold, Cost, CostCurrencyID, " +
                        "Paid, PaidCurrencyID, Change, ChangeCurrencyID, ProfitPerItem, ProfitPerItemCurrencyID, InventoryItemID, SoldByUserID) " +
                        " VALUES (@dateTime, @quantity, @cost, @costCurrency, @paid, @paidCurrency, @change, @changeCurrency, @profit, " +
                        "@profitCurrency, @inventoryID, @userID) ";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@dateTime", DateTimeSold.ToString(Utilities.DateTimeToStringFormat()));
                    command.Parameters.AddWithValue("@quantity", QuantitySold);
                    command.Parameters.AddWithValue("@cost", Cost);
                    command.Parameters.AddWithValue("@costCurrency", CostCurrency?.ID);
                    command.Parameters.AddWithValue("@paid", Paid);
                    command.Parameters.AddWithValue("@paidCurrency", PaidCurrency?.ID);
                    command.Parameters.AddWithValue("@change", Change);
                    command.Parameters.AddWithValue("@changeCurrency", ChangeCurrency?.ID);
                    command.Parameters.AddWithValue("@profit", ProfitPerItem);
                    command.Parameters.AddWithValue("@profitCurrency", ProfitPerItemCurrency?.ID);
                    command.Parameters.AddWithValue("@inventoryID", InventoryItemID);
                    command.Parameters.AddWithValue("@userID", SoldByUserID);
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
                    string query = "UPDATE InventoryItems SET Name = @name, Description = @description, PicturePath = @picturePath, " +
                        "Cost = @cost, CostCurrencyID = @costCurrencyID, Quantity = @quantity, BarcodeNumber = @barcodeNumber, " +
                        "CreatedByUserID = @createdByUserID, ProfitPerItem = @profitPerItem, ProfitPerItemCurrencyID = @profitPerItemCurrencyID" +
                        " WHERE ID = @id";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@id", ID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
    }
}
