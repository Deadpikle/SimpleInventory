using SimpleInventory.Helpers;
using SimpleInventory.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    public class PurchasedItem : ChangeNotifier, IItemSoldInfo
    {
        private int _id;
        private int _quantity;
        private int _inventoryItemID;
        private string _name;
        private string _type;
        private decimal _cost;
        private string _costCurrencySymbol;
        private decimal _costCurrencyConversionRate;
        private decimal _profit;
        private string _profitCurrencySymbol;
        private decimal _profitCurrencyConversionRate;
        private int _purchaseID;
        private ItemType _itemType;

        public int ID
        {
            get => _id;
            set { _id = value; NotifyPropertyChanged(); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; NotifyPropertyChanged(); }
        }

        public int InventoryItemID
        {
            get => _inventoryItemID;
            set { _inventoryItemID = value; NotifyPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; NotifyPropertyChanged(); }
        }

        public string Type
        {
            get => _type;
            set { _type = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// This is cost per item~
        /// </summary>
        public decimal Cost
        {
            get => _cost;
            set { _cost = value; NotifyPropertyChanged(); }
        }

        public string CostCurrencySymbol
        {
            get => _costCurrencySymbol;
            set { _costCurrencySymbol = value; NotifyPropertyChanged(); }
        }

        public decimal CostCurrencyConversionRate
        {
            get => _costCurrencyConversionRate;
            set { _costCurrencyConversionRate = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// This is total profit
        /// </summary>
        public decimal Profit
        {
            get => _profit;
            set { _profit = value; NotifyPropertyChanged(); }
        }

        public string ProfitCurrencySymbol
        {
            get => _profitCurrencySymbol;
            set { _profitCurrencySymbol = value; NotifyPropertyChanged(); }
        }

        public decimal ProfitCurrencyConversionRate
        {
            get => _profitCurrencyConversionRate;
            set { _profitCurrencyConversionRate = value; NotifyPropertyChanged(); }
        }

        public int PurchaseID
        {
            get => _purchaseID;
            set { _purchaseID = value; NotifyPropertyChanged(); }
        }

        public ItemType ItemType
        {
            get => _itemType;
            set { _itemType = value; NotifyPropertyChanged(); }
        }

        public DateTime? DateTimePurchased { get; set; }

        public string SoldByUserName { get; set; }

        public string FriendlyDateTime => DateTimePurchased?.ToString(Utilities.DateTimeToFriendlyFullDateTimeStringFormat()) ?? "";

        public int QuantitySold => Quantity;

        public string TotalCostWithCurrency
        {
            get
            {
                return (Cost * Quantity).ToString() + " (" + CostCurrencySymbol + ")";
            }
        }

        public string TotalProfitWithCurrency
        {
            get
            {
                return (Profit * Quantity).ToString() + " (" + ProfitCurrencySymbol + ")";
            }
        }

        public static List<PurchasedItem> LoadPurchasedItems(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var purchasedItems = new List<PurchasedItem>();
            string query = "" +
                "SELECT PurchasedItems.ID AS PurchasedItemID, PurchasedItems.Quantity, " +
                "PurchasedItems.InventoryItemID, PurchasedItems.Name, Type, PurchasedItems.Cost, " +
                "CostCurrencySymbol, CostCurrencyConversionRate, PurchasedItems.Profit, " +
                    "ProfitCurrencySymbol, ProfitCurrencyConversionRate, PurchaseID, " +
                    "it.ID AS ItemTypeID, it.Name AS ItemTypeName, it.Description AS ItemTypeDescription," +
                    "it.IsDefault AS ItemTypeIsDefault " +
                "FROM PurchasedItems " +
                "   JOIN InventoryItems i ON PurchasedItems.InventoryItemID = i.ID" +
                "   LEFT JOIN ItemTypes it ON i.ItemTypeID = it.ID " +
                (string.IsNullOrEmpty(whereClause) ? "" : whereClause) + " " +
                "ORDER BY PurchasedItems.Name DESC, PurchasedItems.Quantity ASC";
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
                            var purchasedItem = new PurchasedItem();
                            purchasedItem.ID = dbHelper.ReadInt(reader, "PurchasedItemID");
                            purchasedItem.Quantity = dbHelper.ReadInt(reader, "Quantity");
                            purchasedItem.InventoryItemID = dbHelper.ReadInt(reader, "InventoryItemID");
                            purchasedItem.ItemType = new ItemType(
                                dbHelper.ReadInt(reader, "ItemTypeID"),
                                dbHelper.ReadString(reader, "ItemTypeName"),
                                dbHelper.ReadString(reader, "ItemTypeDescription"),
                                dbHelper.ReadBool(reader, "ItemTypeIsDefault"));
                            purchasedItem.Name = dbHelper.ReadString(reader, "Name");
                            purchasedItem.Type = dbHelper.ReadString(reader, "Type");
                            purchasedItem.Cost = dbHelper.ReadDecimal(reader, "Cost");
                            purchasedItem.CostCurrencySymbol = dbHelper.ReadString(reader, "CostCurrencySymbol");
                            purchasedItem.CostCurrencyConversionRate = dbHelper.ReadDecimal(reader, "CostCurrencyConversionRate");
                            purchasedItem.Profit = dbHelper.ReadDecimal(reader, "Profit");
                            purchasedItem.ProfitCurrencySymbol = dbHelper.ReadString(reader, "ProfitCurrencySymbol");
                            purchasedItem.ProfitCurrencyConversionRate = dbHelper.ReadDecimal(reader, "ProfitCurrencyConversionRate");
                            purchasedItem.PurchaseID = dbHelper.ReadInt(reader, "PurchaseID");
                            purchasedItems.Add(purchasedItem);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return purchasedItems;
        }

        public static List<PurchasedItem> LoadPurchasedItemsForPurchaseIDs(List<int> purchaseIDs)
        {
            if (purchaseIDs.Count == 0)
            {
                return new List<PurchasedItem>();
            }
            string whereClause = string.Format(" WHERE PurchaseID IN ({0}) ", string.Join(",", purchaseIDs));
            return LoadPurchasedItems(whereClause);
        }

        public void Create()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string insert = "" +
                        "INSERT INTO PurchasedItems " +
                        "(Quantity, InventoryItemID, Name, Type, Cost, CostCurrencySymbol, CostCurrencyConversionRate, Profit, " +
                            "ProfitCurrencySymbol, ProfitCurrencyConversionRate, PurchaseID) " +
                        "VALUES (@quantity, @itemID, @name, @type, @cost, @costSymbol, @costConversion," +
                            "@profit, @profitSymbol, @profitConversion, @purchaseID)";
                    command.CommandText = insert;
                    command.Parameters.AddWithValue("@quantity", Quantity);
                    command.Parameters.AddWithValue("@itemID", InventoryItemID);
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@type", Type);
                    command.Parameters.AddWithValue("@cost", Cost);
                    command.Parameters.AddWithValue("@costSymbol", CostCurrencySymbol);
                    command.Parameters.AddWithValue("@costConversion", CostCurrencyConversionRate);
                    command.Parameters.AddWithValue("@profit", Profit);
                    command.Parameters.AddWithValue("@profitSymbol", ProfitCurrencySymbol);
                    command.Parameters.AddWithValue("@profitConversion", ProfitCurrencyConversionRate);
                    command.Parameters.AddWithValue("@purchaseID", PurchaseID);
                    command.ExecuteNonQuery();
                    ID = (int)conn.LastInsertRowId;
                }
                conn.Close();
            }
        }

        public void Delete()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "DELETE FROM PurchasedItems WHERE ID = @id";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@id", ID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
    }
}
