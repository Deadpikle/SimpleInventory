using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
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
        public ItemType ItemType { get; set; }

        public string ItemName { get; set; }
        public string ItemDescription { get; set; }

        public string FriendlyTime
        {
            get { return DateTimeSold.ToLongTimeString(); }
        }

        public string FriendlyDateTime
        {
            get { return DateTimeSold.ToString(Utilities.DateTimeToFriendlyFullDateTimeStringFormat()); }
        }

        public string CostWithCurrency
        {
            get
            {
                if (CostCurrency != null)
                {
                    return Cost.ToString() + " (" + CostCurrency?.Symbol + ")";
                }
                return Cost.ToString();
            }
        }

        public string TotalCostWithCurrency
        {
            get
            {
                if (CostCurrency != null)
                {
                    return (Cost * QuantitySold).ToString() + " (" + CostCurrency?.Symbol + ")";
                }
                return (Cost * QuantitySold).ToString();
            }
        }

        public string ProfitWithCurrency
        {
            get
            {
                if (ProfitPerItemCurrency != null)
                {
                    return ProfitPerItem.ToString() + " (" + ProfitPerItemCurrency?.Symbol + ")";
                }
                return ProfitPerItem.ToString();
            }
        }

        public string TotalProfitWithCurrency
        {
            get
            {
                if (ProfitPerItemCurrency != null)
                {
                    return (ProfitPerItem * QuantitySold).ToString() + " (" + ProfitPerItemCurrency?.Symbol + ")";
                }
                return (ProfitPerItem * QuantitySold).ToString();
            }
        }

        public static List<ItemSoldInfo> LoadInfoForDate(DateTime date, string extraWhereClause = "", List<Tuple<string, string>> extraWhereParams = null)
        {
            var items = new List<ItemSoldInfo>();
            var currencies = Currency.GetKeyValueCurrencyList();
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "" +
                        "SELECT isi.ID, DateTimeSold, QuantitySold, isi.Cost, isi.CostCurrencyID, isi.ProfitPerItem, isi.ProfitPerItemCurrencyID, " +
                        "       isi.InventoryItemID, isi.SoldByUserID, i.Name, i.Description, isi.Paid, isi.PaidCurrencyID, isi.Change, isi.ChangeCurrencyID, " +
                        "       it.ID AS ItemTypeID, it.Name AS ItemTypeName, it.Description AS ItemTypeDescription," +
                        "       it.IsDefault AS ItemTypeIsDefault " +
                        "FROM ItemsSoldInfo isi JOIN InventoryItems i ON isi.InventoryItemID = i.ID " +
                        "   LEFT JOIN ItemTypes it ON i.ItemTypeID = it.ID " +
                        "WHERE DateTimeSold LIKE '" + date.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + "%' " +
                        (string.IsNullOrWhiteSpace(extraWhereClause) ? "" : extraWhereClause + " ") +
                        "ORDER BY i.Name, isi.DateTimeSold";

                    command.CommandText = query;
                    if (!string.IsNullOrEmpty(extraWhereClause) && extraWhereParams != null)
                    {
                        foreach (Tuple<string, string> keyValuePair in extraWhereParams)
                        {
                            command.Parameters.AddWithValue(keyValuePair.Item1, keyValuePair.Item2);
                        }
                    }
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new ItemSoldInfo();
                            item.ID = dbHelper.ReadInt(reader, "ID");
                            item.ItemName = dbHelper.ReadString(reader, "Name");
                            item.ItemDescription = dbHelper.ReadString(reader, "Description");
                            item.ItemType = new ItemType(
                                dbHelper.ReadInt(reader, "ItemTypeID"),
                                dbHelper.ReadString(reader, "ItemTypeName"),
                                dbHelper.ReadString(reader, "ItemTypeDescription"),
                                dbHelper.ReadBool(reader, "ItemTypeIsDefault"));
                            item.InventoryItemID = dbHelper.ReadInt(reader, "InventoryItemID");
                            item.SoldByUserID = dbHelper.ReadInt(reader, "SoldByUserID");
                            string dateTimeSold = dbHelper.ReadString(reader, "DateTimeSold");
                            item.DateTimeSold = Convert.ToDateTime(dateTimeSold); // DateTime.ParseExact(dateTimeSold, 
                                //Utilities.DateTimeToDateOnlyStringFormat(), System.Globalization.CultureInfo.InvariantCulture);
                            item.QuantitySold = dbHelper.ReadInt(reader, "QuantitySold");
                            item.Cost = dbHelper.ReadDecimal(reader, "Cost");
                            var costCurrencyID = dbHelper.ReadInt(reader, "CostCurrencyID");
                            item.CostCurrency = currencies.ContainsKey(costCurrencyID) ? currencies[costCurrencyID] : null;
                            item.ProfitPerItem = dbHelper.ReadDecimal(reader, "ProfitPerItem");
                            var profitCurrencyID = dbHelper.ReadInt(reader, "ProfitPerItemCurrencyID");
                            item.ProfitPerItemCurrency = currencies.ContainsKey(profitCurrencyID) ? currencies[profitCurrencyID] : null;

                            item.Paid = dbHelper.ReadDecimal(reader, "Paid");
                            var paidCurrencyID = dbHelper.ReadInt(reader, "PaidCurrencyID");
                            item.PaidCurrency = currencies.ContainsKey(paidCurrencyID) ? currencies[paidCurrencyID] : null;

                            item.Change = dbHelper.ReadDecimal(reader, "Change");
                            var changeCurrencyID = dbHelper.ReadInt(reader, "ChangeCurrencyID");
                            item.ChangeCurrency = currencies.ContainsKey(changeCurrencyID) ? currencies[changeCurrencyID] : null;

                            items.Add(item);
                        }
                        reader.Close();
                    }

                    conn.Close();
                }
            }
            return items;
        }


        public static List<ItemSoldInfo> LoadInfoForDateAndItem(DateTime date, int inventoryItemID)
        {
            return LoadInfoForDate(date, " AND InventoryItemID = @itemID",
                new List<Tuple<string, string>>() { new Tuple<string, string>("@itemID", inventoryItemID.ToString()) });
        }

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
                    ID = (int)conn.LastInsertRowId;
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
                    string query = "UPDATE ItemsSoldInfo SET DateTimeSold = @dateTime, QuantitySold = @quantity, Cost = @cost, " +
                        "CostCurrencyID = @costCurrency, Paid = @paid, PaidCurrencyID = @paidCurrency, Change = @change, " +
                        "ChangeCurrencyID = @changeCurrency, ProfitPerItem = @profit, ProfitPerItemCurrencyID = @profitCurrency, " +
                        "InventoryItemID = @inventoryID, SoldByUserID = @userID " +
                        " WHERE ID = @id";
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
                    command.Parameters.AddWithValue("@id", ID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void Delete()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "DELETE FROM ItemsSoldInfo WHERE ID = @id";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@id", ID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
    }
}
