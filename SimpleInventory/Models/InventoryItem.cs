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
        public string CreatedByUserName { get; set; }
        public ItemType Type { get; set; }

        public decimal ItemPurchaseCost { get; set; }
        public Currency ItemPurchaseCostCurrency { get; set; }
        public int ItemsPerPurchase { get; set; }

        public string ItemPurchaseCostWithCurrency
        {
            get
            {
                if (ItemPurchaseCostCurrency != null)
                {
                    return ItemPurchaseCost.ToString() + " (" + ItemPurchaseCostCurrency?.Symbol + ")";
                }
                return ItemPurchaseCost.ToString();
            }
        }

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
                "       ProfitPerItem, ProfitPerItemCurrencyID, ii.WasDeleted, " +
                "       it.ID AS ItemTypeID, it.Name AS ItemTypeName, it.Description AS ItemTypeDescription," +
                "       it.IsDefault AS ItemTypeIsDefault, u.Name AS UserName, " +
                "       ItemPurchaseCost, ItemPurchaseCostCurrencyID, ItemsPerPurchase " +
                "FROM InventoryItems ii " +
                "   LEFT JOIN Users u ON ii.CreatedByUserID = u.ID " +
                "   LEFT JOIN ItemTypes it ON ii.ItemTypeID = it.ID " +
                (string.IsNullOrEmpty(whereClause) ? " " : whereClause) + " " +
                "ORDER BY lower(ii.Name), lower(ii.Description), Cost";
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
                            item.CreatedByUserName = dbHelper.ReadString(reader, "UserName");
                            item.Type = new ItemType(
                                dbHelper.ReadInt(reader, "ItemTypeID"),
                                dbHelper.ReadString(reader, "ItemTypeName"),
                                dbHelper.ReadString(reader, "ItemTypeDescription"),
                                dbHelper.ReadBool(reader, "ItemTypeIsDefault"));
                            item.ItemPurchaseCost = dbHelper.ReadDecimal(reader, "ItemPurchaseCost");
                            var itemPurchaseCostCurrencyID = dbHelper.ReadInt(reader, "ItemPurchaseCostCurrencyID");
                            item.ItemPurchaseCostCurrency = currencies.ContainsKey(itemPurchaseCostCurrencyID) ? currencies[itemPurchaseCostCurrencyID] : null;
                            item.ItemsPerPurchase = dbHelper.ReadInt(reader, "ItemsPerPurchase");
                            item.WasDeleted = dbHelper.ReadBool(reader, "WasDeleted");
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
            return LoadItems(" WHERE ii.WasDeleted = 0");
        }

        public static InventoryItem LoadItemByID(int id)
        {
            var data = LoadItems(" WHERE ii.WasDeleted = 0 AND ii.ID = @id ",
                new List<Tuple<string, string>>() { new Tuple<string, string>("@id", id.ToString()) });
            return data.Count > 0 ? data[0] : null;
        }

        public static InventoryItem LoadItemByBarcode(string barcode)
        {
            var data = LoadItems(" WHERE ii.WasDeleted = 0 AND BarcodeNumber = @barcode ",
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
                        "CostCurrencyID, Quantity, BarcodeNumber, CreatedByUserID, ProfitPerItem, ProfitPerItemCurrencyID, ItemTypeID, " +
                        "ItemPurchaseCost, ItemPurchaseCostCurrencyID, ItemsPerPurchase) VALUES " +
                        "(@name, @description, @picturePath, @cost, @costCurrencyID, @quantity, @barcodeNumber, @createdByUserID," +
                        " @profitPerItem, @profitPerItemCurrencyID, @itemTypeID," +
                        " @itemPurchaseCost, @itemPurchaseCostCurrencyID, @itemsPerPurchase)";
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
                    command.Parameters.AddWithValue("@itemPurchaseCost", ItemPurchaseCost.ToString());
                    command.Parameters.AddWithValue("@itemPurchaseCostCurrencyID", ItemPurchaseCostCurrency?.ID);
                    command.Parameters.AddWithValue("@itemsPerPurchase", ItemsPerPurchase);
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
                    string query = 
                        "UPDATE InventoryItems SET Name = @name, Description = @description, PicturePath = @picturePath, " +
                            "Cost = @cost, CostCurrencyID = @costCurrencyID, BarcodeNumber = @barcodeNumber, " +
                            "CreatedByUserID = @createdByUserID, ProfitPerItem = @profitPerItem, ProfitPerItemCurrencyID = @profitPerItemCurrencyID, " +
                            "ItemTypeID = @itemTypeID, " +
                            "ItemPurchaseCost = @itemPurchaseCost, " +
                            "ItemPurchaseCostCurrencyID = @itemPurchaseCostCurrencyID, ItemsPerPurchase = @itemsPerPurchase " +
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
                    command.Parameters.AddWithValue("@itemPurchaseCost", ItemPurchaseCost.ToString());
                    command.Parameters.AddWithValue("@itemPurchaseCostCurrencyID", ItemPurchaseCostCurrency?.ID);
                    command.Parameters.AddWithValue("@itemsPerPurchase", ItemsPerPurchase);
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

        public static List<InventoryItem> GetStockByDateTime(DateTime date, bool ignoreTime = true, bool removeItemsWithNoQuantity = true)
        {
            var items = LoadItemsNotDeleted();
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    var dateForQuery = ignoreTime ? date.AddDays(1).Date : date; 
                    // date.AddDays(1).Date makes the "end date" basically 12:00 AM the next day so you get everything from the prior day
                    var dateForQueryString = dateForQuery.ToString(Utilities.DateTimeToStringFormat());
                    // current quantity will be the sum of QuantityAdjustments and ItemsSoldInfo for the given
                    // inventory item ID by the end of the given date

                    command.CommandText = "" +
                        "SELECT" +
                        "   (SELECT IFNULL(SUM(qa.AmountChanged), 0) " +
                        "   FROM QuantityAdjustments qa " +
                        "   WHERE qa.InventoryItemID = @itemID AND qa.DateTimeChanged < @date)" +
                        " - " +
                        "   (SELECT IFNULL(SUM(isi.QuantitySold), 0) " +
                        "    FROM ItemsSoldInfo isi " +
                        "    WHERE isi.InventoryItemID = @itemID AND isi.DateTimeSold < @date)" +
                        "AS CurrentStock";

                    foreach (InventoryItem item in items)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@itemID", item.ID);
                        command.Parameters.AddWithValue("@date", dateForQueryString);

                        int quantity = 0;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                            {
                                quantity = dbHelper.ReadInt(reader, 0);
                            }
                            reader.Close(); // have to close it now otherwise we can't execute commands
                        }
                        item.Quantity = quantity;
                    }
                    conn.Close();
                }
            }
            if (removeItemsWithNoQuantity)
            {
                items.RemoveAll((item) => item.Quantity == 0);
            }
            items.Sort((left, right) => left.Name.ToLower().CompareTo(right.Name.ToLower()));
            return items;
        }


        public static List<DetailedStockReportInfo> GetStockOnDates(DateTime firstDate, DateTime secondDate)
        {
            if (secondDate < firstDate)
            {
                return new List<DetailedStockReportInfo>(); // dates are invalid
            }
            var firstStock = GetStockByDateTime(firstDate, false, false);
            var lastStock = GetStockByDateTime(secondDate, false, false);
            var output = new List<DetailedStockReportInfo>();
            // this isn't very optimized for matching up the items, but it will do for now~
            for (int i = 0; i < firstStock.Count; i++)
            {
                var firstStockItem = firstStock[i];
                var reportInfo = new DetailedStockReportInfo();
                reportInfo.Item = firstStockItem;
                reportInfo.StartStock = firstStockItem.Quantity;

                var adjustments = QuantityAdjustment.LoadQuantityAdjustmentsBetweenDates(firstStockItem, firstDate, secondDate);
                foreach (QuantityAdjustment adjustment in adjustments)
                {
                    if (adjustment.WasAdjustedForStockPurchase)
                    {
                        reportInfo.AmountChangedFromPurchaseStockIncrease += adjustment.AmountChanged;
                    }
                    else
                    {
                        reportInfo.AmountFromOtherQuantityAdjustments += adjustment.AmountChanged;
                    }
                }

                var secondStockItem = lastStock.Where(x => x.ID == firstStockItem.ID).FirstOrDefault();
                if (secondStockItem != null)
                {
                    reportInfo.EndStock = secondStockItem.Quantity;
                }
                output.Add(reportInfo);
            }
            output.Sort((left, right) => left.Item.Name.ToLower().CompareTo(right.Item.Name.ToLower()));
            return output;
        }
    }
}
