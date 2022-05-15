using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
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
        // customer info should really be in another table, but that
        // can be an improvement left for another time since we are
        // crunched for time
        private string _customerName;
        private string _customerPhone;
        private string _customerEmail;
        private int _userID;

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

        public static List<Purchase> LoadPurchases(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var purchases = new List<Purchase>();
            string query = "" +
                "SELECT ID, DateTimePurchased, TotalCost, CustomerName, CustomerPhone, CustomerEmail, UserID, " +
                    "CostCurrencySymbol, CostCurrencyConversionRate " +
                "FROM Purchases " +
                (string.IsNullOrEmpty(whereClause) ? "" : whereClause) + " " +
                "ORDER BY DateTimePurchased DESC";
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
                            var dateTimePurchased = dbHelper.ReadString(reader, "DateTimePurchased");
                            purchase.DateTimePurchased = Convert.ToDateTime(dateTimePurchased);
                            purchase.TotalCost = dbHelper.ReadDecimal(reader, "TotalCost");
                            purchase.CostCurrencySymbol = dbHelper.ReadString(reader, "CostCurrencySymbol");
                            purchase.CostCurrencyConversionRate = dbHelper.ReadDecimal(reader, "CostCurrencyConversionRate");
                            purchase.CustomerName = dbHelper.ReadString(reader, "CustomerName");
                            purchase.CustomerPhone = dbHelper.ReadString(reader, "CustomerPhone");
                            purchase.CustomerEmail = dbHelper.ReadString(reader, "CustomerEmail");
                            purchases.Add(purchase);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return purchases;
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
                            "CostCurrencySymbol, CostCurrencyConversionRate) " +
                        "VALUES (@dateTime, @totalCost, @name, @phone, @email, @userID, @costSymbol, @costConversion)";
                    command.CommandText = insert;
                    command.Parameters.AddWithValue("@dateTime", DateTimePurchased);
                    command.Parameters.AddWithValue("@totalCost", TotalCost);
                    command.Parameters.AddWithValue("@costSymbol", CostCurrencySymbol);
                    command.Parameters.AddWithValue("@costConversion", CostCurrencyConversionRate);
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
    }
}
