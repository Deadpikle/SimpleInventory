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
        public ItemType Type { get; set; }

        public decimal Cost { get; set; }
        public Currency CostCurrency { get; set; }
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

        public decimal ProfitPerItem { get; set; }
        public Currency ProfitPerItemCurrency { get; set; }
        public string ProfitPerItemWithCurrency
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

        public int Quantity { get; set; }

        /// BarcodeNumber is a string just in case we need to change from #'s later or it's really long or something
        public string BarcodeNumber { get; set; }
        public bool WasDeleted { get; set; }

        public static List<InventoryItem> LoadItems(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var items = new List<InventoryItem>();
            string query = "" +
                "SELECT ii.ID, ii.Name, ii.Description, PicturePath, Cost, CostCurrencyID, Quantity, BarcodeNumber, CreatedByUserID," +
                "       ProfitPerItem, ProfitPerItemCurrencyID, WasDeleted, " +
                "       it.ID AS ItemTypeID, it.Name AS ItemTypeName, it.Description AS ItemTypeDescription," +
                "       it.IsDefault AS ItemTypeIsDefault " +
                "FROM InventoryItems ii " +
                "   LEFT JOIN Users u ON ii.CreatedByUserID = u.ID " +
                "   LEFT JOIN ItemTypes it ON ii.ItemTypeID = it.ID " +
                (string.IsNullOrEmpty(whereClause) ? " " : whereClause) + " " +
                "ORDER BY ii.Name, Cost, ii.Description";
            var currencies = Currency.GetKeyValueCurrencyList();
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
                            item.Cost = dbHelper.ReadDecimal(reader, "Cost");
                            var costCurrencyID = dbHelper.ReadInt(reader, "CostCurrencyID");
                            item.CostCurrency = currencies.ContainsKey(costCurrencyID) ? currencies[costCurrencyID] : null;
                            item.ProfitPerItem = dbHelper.ReadDecimal(reader, "ProfitPerItem");
                            var profitCurrencyID = dbHelper.ReadInt(reader, "ProfitPerItemCurrencyID");
                            item.ProfitPerItemCurrency = currencies.ContainsKey(profitCurrencyID) ? currencies[profitCurrencyID] : null;
                            item.Quantity = dbHelper.ReadInt(reader, "Quantity");
                            item.BarcodeNumber = dbHelper.ReadString(reader, "BarcodeNumber");
                            item.CreatedByUserID = dbHelper.ReadInt(reader, "CreatedByUserID");
                            item.Type = new ItemType(
                                dbHelper.ReadInt(reader, "ItemTypeID"),
                                dbHelper.ReadString(reader, "ItemTypeName"),
                                dbHelper.ReadString(reader, "ItemTypeDescription"),
                                dbHelper.ReadBool(reader, "ItemTypeIsDefault"));
                            item.WasDeleted = dbHelper.ReadBool(reader, "WasDeleted");
                            items.Add(item);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return items;
        }

        public static List<InventoryItem> LoadItemsNotDeleted()
        {
            return LoadItems(" WHERE WasDeleted = 0");
        }

        public static InventoryItem LoadItemByID(int id)
        {
            var data = LoadItems(" WHERE WasDeleted = 0 AND ID = @id ", 
                new List<Tuple<string, string>>() { new Tuple<string, string>("@id", id.ToString()) });
            return data.Count > 0 ? data[0] : null;
        }

        public static InventoryItem LoadItemByBarcode(string barcode)
        {
            var data = LoadItems(" WHERE WasDeleted = 0 AND BarcodeNumber = @barcode ",
                new List<Tuple<string, string>>() { new Tuple<string, string>("@barcode", barcode) });
            return data.Count > 0 ? data[0] : null;
        }

        public void CreateNewItem(int userID)
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "INSERT INTO InventoryItems (Name, Description, PicturePath, Cost, " +
                        "CostCurrencyID, Quantity, BarcodeNumber, CreatedByUserID, ProfitPerItem, ProfitPerItemCurrencyID, ItemTypeID) VALUES " +
                        "(@name, @description, @picturePath, @cost, @costCurrencyID, @quantity, @barcodeNumber, @createdByUserID," +
                        " @profitPerItem, @profitPerItemCurrencyID, @itemTypeID)";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@description", Description);
                    command.Parameters.AddWithValue("@picturePath", PicturePath);
                    command.Parameters.AddWithValue("@cost", Cost.ToString());
                    command.Parameters.AddWithValue("@costCurrencyID", CostCurrency?.ID);
                    command.Parameters.AddWithValue("@quantity", Quantity);
                    command.Parameters.AddWithValue("@barcodeNumber", BarcodeNumber);
                    command.Parameters.AddWithValue("@createdByUserID", userID);
                    command.Parameters.AddWithValue("@profitPerItem", ProfitPerItem.ToString());
                    command.Parameters.AddWithValue("@profitPerItemCurrencyID", ProfitPerItemCurrency?.ID);
                    command.Parameters.AddWithValue("@itemTypeID", Type?.ID);
                    command.ExecuteNonQuery();
                    ID = (int)conn.LastInsertRowId;
                    conn.Close();
                }
            }
        }

        public void SaveItemUpdates(int userID)
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "UPDATE InventoryItems SET Name = @name, Description = @description, PicturePath = @picturePath, " +
                        "Cost = @cost, CostCurrencyID = @costCurrencyID, BarcodeNumber = @barcodeNumber, " +
                        "CreatedByUserID = @createdByUserID, ProfitPerItem = @profitPerItem, ProfitPerItemCurrencyID = @profitPerItemCurrencyID, " +
                        "ItemTypeID = @itemTypeID " +
                        " WHERE ID = @id";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@description", Description);
                    command.Parameters.AddWithValue("@picturePath", PicturePath);
                    command.Parameters.AddWithValue("@cost", Cost.ToString());
                    command.Parameters.AddWithValue("@costCurrencyID", CostCurrency?.ID);
                    command.Parameters.AddWithValue("@barcodeNumber", BarcodeNumber);
                    command.Parameters.AddWithValue("@createdByUserID", userID);
                    command.Parameters.AddWithValue("@profitPerItem", ProfitPerItem.ToString());
                    command.Parameters.AddWithValue("@profitPerItemCurrencyID", ProfitPerItemCurrency?.ID);
                    command.Parameters.AddWithValue("@itemTypeID", Type?.ID);
                    command.Parameters.AddWithValue("@id", ID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void AdjustQuantityByAmount(int amount = 1)
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    var mathOperator = amount < 0 ? "-" : "+";
                    amount = Math.Abs(amount);
                    string query = "UPDATE InventoryItems SET Quantity = Quantity " + mathOperator + 
                        " " + amount.ToString() + " WHERE ID = @id";
                    command.CommandText = query;
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
                    string query = "UPDATE InventoryItems SET WasDeleted = 1 WHERE ID = @id";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@id", ID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public static List<InventoryItem> GetStockByEndOfDate(DateTime date)
        {
            // this function is terribly unoptimized. There's probably some smart-o
            // way we can do this all in one query. TODO: optimize GetStockByEndOfDate
            var items = LoadItems();
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    var dateForQuery = date.AddDays(1).Date;
                    // current quantity will be the sum of QuantityAdjustments and ItemsSoldInfo for the given
                    // inventory item ID by the end of the given date
                    command.CommandText = "" +
                        "SELECT IFNULL(SUM(qa.AmountChanged), 0) " +
                        "FROM QuantityAdjustments qa " +
                        "WHERE qa.InventoryItemID = @itemID AND qa.DateTimeChanged < @date";

                    using (var secondSumCommand = dbHelper.GetSQLiteCommand(conn))
                    {
                        secondSumCommand.CommandText = "" +
                            "SELECT IFNULL(SUM(isi.QuantitySold), 0) " +
                            "FROM ItemsSoldInfo isi " +
                            "WHERE isi.InventoryItemID = @itemID AND isi.DateTimeSold < @date";

                        foreach (InventoryItem item in items)
                        {
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@itemID", item.ID);
                            command.Parameters.AddWithValue("@date", dateForQuery.ToString(Utilities.DateTimeToStringFormat()));

                            int quantity = 0;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.HasRows && reader.Read())
                                {
                                    quantity += dbHelper.ReadInt(reader, 0);
                                }
                                reader.Close(); // have to close it now otherwise we can't execute commands
                            }

                            secondSumCommand.Parameters.Clear();
                            secondSumCommand.Parameters.AddWithValue("@itemID", item.ID);
                            secondSumCommand.Parameters.AddWithValue("@date", dateForQuery.ToString(Utilities.DateTimeToStringFormat()));
                            using (var reader = secondSumCommand.ExecuteReader())
                            {
                                if (reader.HasRows && reader.Read())
                                {
                                    quantity -= dbHelper.ReadInt(reader, 0);
                                }
                                reader.Close(); // have to close it now otherwise we can't execute commands
                            }
                            item.Quantity = quantity;
                        }
                    }
                    conn.Close();
                }
            }
            items.RemoveAll((item) => item.WasDeleted && item.Quantity == 0);
            items.Sort((left, right) => left.Name.CompareTo(right.Name));
            return items;
        }
    }
}
