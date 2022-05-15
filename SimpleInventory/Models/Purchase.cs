using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;

namespace SimpleInventory.Models
{
    public class Purchase : ChangeNotifier
    {
        private int _id;
        private DateTime _dateTimePurchased;
        private decimal _totalCost;
        private string _costCurrencySymbol;
        private decimal _costCurrencyConversionRate;
        private string _changeCurrencySymbol;
        private decimal _changeCurrencyConversionRate;
        // customer info should really be in another table, but that
        // can be an improvement left for another time since we are
        // crunched for time
        private string _customerName;
        private string _customerPhone;
        private string _customerEmail;
        private int _userID; // user who sold the item / made the sale
        private string _soldByUserName;

        private ObservableCollection<PurchasedItem> _items;

        public Purchase()
        {
            Items = new ObservableCollection<PurchasedItem>();
        }

        public int ID
        {
            get => _id;
            set { _id = value; NotifyPropertyChanged(); }
        }

        public DateTime DateTimePurchased
        {
            get => _dateTimePurchased;
            set { _dateTimePurchased = value; NotifyPropertyChanged(); }
        }

        public decimal TotalCost
        {
            get => _totalCost;
            set { _totalCost = value; NotifyPropertyChanged(); }
        }

        public string TotalCostWithCurrency
        {
            get
            {
                return TotalCost.ToString("0.00") + " (" + CostCurrencySymbol + ")";
            }
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

        public string ChangeCurrencySymbol
        {
            get => _changeCurrencySymbol;
            set { _changeCurrencySymbol = value; NotifyPropertyChanged(); }
        }

        public decimal ChangeCurrencyConversionRate
        {
            get => _changeCurrencyConversionRate;
            set { _changeCurrencyConversionRate = value; NotifyPropertyChanged(); }
        }

        public string CustomerName
        {
            get => _customerName;
            set { _customerName = value; NotifyPropertyChanged(); }
        }

        public string CustomerPhone
        {
            get => _customerPhone;
            set { _customerPhone = value; NotifyPropertyChanged(); }
        }

        public string CustomerEmail
        {
            get => _customerEmail;
            set { _customerEmail = value; NotifyPropertyChanged(); }
        }

        public int UserID
        {
            get => _userID;
            set { _userID = value; NotifyPropertyChanged(); }
        }

        public string SoldByUserName
        {
            get => _soldByUserName;
            set { _soldByUserName = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<PurchasedItem> Items
        {
            get => _items;
            set { _items = value; NotifyPropertyChanged(); }
        }

        public bool HasSingleItem
        {
            get => Items.Count == 1 && Items[0] != null;
        }

        public PurchasedItem FirstItem
        {
            get => Items[0];
        }

        public int TotalNumberOfItemsSold
        {
            get
            {
                var count = 0;
                foreach (var item in Items)
                {
                    count += item.Quantity;
                }
                return count;
            }
        }

        public static List<Purchase> LoadPurchases(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var purchases = new List<Purchase>();
            string query = "" +
                "SELECT ID, DateTimePurchased, TotalCost, Name, Phone, Email, UserID, " +
                    "CostCurrencySymbol, CostCurrencyConversionRate, ChangeCurrencySymbol, ChangeCurrencyConversionRate " +
                "FROM Purchases " +
                (string.IsNullOrEmpty(whereClause) ? "" : whereClause) + " " +
                "ORDER BY DateTimePurchased DESC";
            var purchaseIDs = new List<int>();
            var userIDs = new List<int>();
            var purchaseIDToPurchase = new Dictionary<int, Purchase>();
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
                            var purchase = new Purchase();
                            purchase.ID = dbHelper.ReadInt(reader, "ID");
                            purchaseIDs.Add(purchase.ID);
                            var dateTimePurchased = dbHelper.ReadString(reader, "DateTimePurchased");
                            purchase.DateTimePurchased = Convert.ToDateTime(dateTimePurchased);
                            purchase.TotalCost = dbHelper.ReadDecimal(reader, "TotalCost");
                            purchase.CostCurrencySymbol = dbHelper.ReadString(reader, "CostCurrencySymbol");
                            purchase.CostCurrencyConversionRate = dbHelper.ReadDecimal(reader, "CostCurrencyConversionRate");
                            purchase.CustomerName = dbHelper.ReadString(reader, "Name");
                            purchase.CustomerPhone = dbHelper.ReadString(reader, "Phone");
                            purchase.CustomerEmail = dbHelper.ReadString(reader, "Email");
                            purchase.UserID = dbHelper.ReadInt(reader, "UserID");
                            purchases.Add(purchase);
                            userIDs.Add(purchase.UserID);
                            purchaseIDToPurchase.Add(purchase.ID, purchase);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            var users = User.LoadUsersWithIDs(userIDs);
            var userIDtoUsers = new Dictionary<int, User>();
            foreach (var user in users)
            {
                userIDtoUsers.Add(user.ID, user);
            }
            var purchasedItems = PurchasedItem.LoadPurchasedItemsForPurchaseIDs(purchaseIDs);
            foreach (var purchasedItem in purchasedItems)
            {
                if (purchaseIDToPurchase.ContainsKey(purchasedItem.PurchaseID))
                {
                    purchaseIDToPurchase[purchasedItem.PurchaseID].Items.Add(purchasedItem);
                    purchasedItem.DateTimePurchased = purchaseIDToPurchase[purchasedItem.PurchaseID].DateTimePurchased;
                    if (userIDtoUsers.ContainsKey(purchaseIDToPurchase[purchasedItem.PurchaseID].UserID))
                    {
                        var name = userIDtoUsers[purchaseIDToPurchase[purchasedItem.PurchaseID].UserID].Name;
                        purchaseIDToPurchase[purchasedItem.PurchaseID].SoldByUserName = name;
                        purchasedItem.SoldByUserName = name;
                    }
                }
            }
            return purchases;
        }
        public static Purchase LoadPurchaseByID(int ID)
        {
            var data = LoadPurchases(" WHERE Purchases.ID = @id ",
                new List<Tuple<string, string>>() { new Tuple<string, string>("@id", ID.ToString())});
            return data.Count > 0 ? data[0] : null;
        }


        public static List<Purchase> LoadInfoForDate(DateTime date, int userID = -1)
        {
            string whereClause = "WHERE DateTimePurchased LIKE '" + date.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + "%' ";
            if (userID != -1)
            {
                whereClause += " AND Purchases.UserID = " + userID + " ";
            }
            return LoadPurchases(whereClause);
        }

        public static List<Purchase> LoadInfoForDateAndItem(DateTime date, int inventoryItemID, int userID = -1)
        {
            string whereClause = "WHERE DateTimePurchased LIKE '" + date.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + "%' ";
            if (userID != -1)
            {
                whereClause += " AND Purchases.UserID = " + userID + " ";
            }
            var purchases = LoadPurchases(whereClause, new List<Tuple<string, string>>() { });
            var purchaseList = new List<Purchase>();
            foreach (var purchase in purchases)
            {
                foreach (var item in purchase.Items)
                {
                    if (item.InventoryItemID == inventoryItemID)
                    {
                        purchaseList.Add(purchase);
                        break;
                    }
                }
            }
            return purchaseList;
        }

        public static List<Purchase> LoadInfoForDateAndItemUntilDate(DateTime startDate, DateTime endDate, int inventoryItemID, int userID = -1)
        {
            if (endDate != null && startDate.Date != endDate.Date && endDate > startDate)
            {
                string whereClause = "WHERE DateTimePurchased BETWEEN '" + startDate.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + " 00:00:00' AND '" +
                        endDate.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + " 00:00:00' ";
                if (userID != -1)
                {
                    whereClause += " AND Purchases.UserID = " + userID + " ";
                }

                var purchases = LoadPurchases(whereClause, new List<Tuple<string, string>>() { });
                var purchaseList = new List<Purchase>();
                foreach (var purchase in purchases)
                {
                    foreach (var item in purchase.Items)
                    {
                        if (item.InventoryItemID == inventoryItemID)
                        {
                            purchaseList.Add(purchase);
                            break;
                        }
                    }
                }
                return purchaseList;
            }
            return LoadInfoForDateAndItem(startDate, inventoryItemID, userID);
        }

        public static List<int> LoadItemIDsSoldBetweenDateAndItemUntilDate(DateTime startDate, DateTime endDate, bool ignoreTime = false)
        {
            List<Purchase> purchaseList = null;
            if (endDate != null && startDate.Date != endDate.Date && endDate > startDate)
            {
                if (!ignoreTime)
                {
                    string whereClause = "WHERE DateTimePurchased BETWEEN '" + startDate.ToString(Utilities.DateTimeToStringFormat()) + "' AND '" +
                            endDate.ToString(Utilities.DateTimeToStringFormat()) + "'";
                    purchaseList = LoadPurchases(whereClause, new List<Tuple<string, string>>() { });
                }
                else
                {
                    string whereClause = "WHERE DateTimePurchased BETWEEN '" + startDate.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + " 00:00:00' AND '" +
                            endDate.ToString(Utilities.DateTimeToDateOnlyStringFormat()) + " 00:00:00'";
                    purchaseList = LoadPurchases(whereClause, new List<Tuple<string, string>>() { });
                }
            }
            else
            {
                purchaseList = LoadInfoForDate(startDate);
            }
            var output = new List<int>();
            foreach (Purchase purchase in purchaseList)
            {
                foreach (PurchasedItem item in purchase.Items)
                {
                    output.Add(item.InventoryItemID);
                }
            }
            return output;
        }

        public void Create()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string insert = "" +
                        "INSERT INTO Purchases " +
                        "(DateTimePurchased, TotalCost, Name, Phone, Email, UserID," +
                            "CostCurrencySymbol, CostCurrencyConversionRate, ChangeCurrencySymbol, ChangeCurrencyConversionRate) " +
                        "VALUES (@dateTime, @totalCost, @name, @phone, @email, @userID, @costSymbol, @costConversion," +
                        "@changeSymbol, @changeConversion)";
                    command.CommandText = insert;
                    command.Parameters.AddWithValue("@dateTime", DateTimePurchased);
                    command.Parameters.AddWithValue("@totalCost", TotalCost);
                    command.Parameters.AddWithValue("@costSymbol", CostCurrencySymbol);
                    command.Parameters.AddWithValue("@costConversion", CostCurrencyConversionRate);
                    command.Parameters.AddWithValue("@changeSymbol", ChangeCurrencySymbol);
                    command.Parameters.AddWithValue("@changeConversion", ChangeCurrencyConversionRate);
                    command.Parameters.AddWithValue("@name", CustomerName);
                    command.Parameters.AddWithValue("@phone", CustomerPhone);
                    command.Parameters.AddWithValue("@email", CustomerEmail);
                    command.Parameters.AddWithValue("@userID", UserID);
                    command.ExecuteNonQuery();
                    ID = (int)conn.LastInsertRowId;
                }
                conn.Close();
            }
        }

        public void Delete()
        {
            foreach (var item in Items)
            {
                item.Delete();
            }
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "DELETE FROM Purchases WHERE ID = @id";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@id", ID);
                    command.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }
    }
}
