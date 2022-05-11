using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    public class Currency
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Symbol { get; set; }
        public decimal ConversionRateToUSD { get; set; } // One X to 1 USD = formula
        public bool IsDefaultCurrency { get; set; }

        public string NameWithSymbol { get { return Name + " (" + Symbol + ")"; } }

        public static List<Currency> LoadCurrencies(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var items = new List<Currency>();
            string query = "" +
                "SELECT ID, Name, Abbreviation, Symbol, ConversionRateToUSD, IsDefaultCurrency " +
                "FROM Currencies " +
                (string.IsNullOrEmpty(whereClause) ? "" : whereClause) + " " +
                "ORDER BY lower(Name), Abbreviation, ConversionRateToUSD";

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
                            var item = new Currency();
                            item.ID = dbHelper.ReadInt(reader, "ID");
                            item.Name = dbHelper.ReadString(reader, "Name");
                            item.Abbreviation = dbHelper.ReadString(reader, "Abbreviation");
                            item.Symbol = dbHelper.ReadString(reader, "Symbol");
                            item.ConversionRateToUSD = dbHelper.ReadDecimal(reader, "ConversionRateToUSD");
                            item.IsDefaultCurrency = dbHelper.ReadBool(reader, "IsDefaultCurrency");
                            items.Add(item);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return items;
        }

        public static Dictionary<int, Currency> GetKeyValueCurrencyList(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var currencies = LoadCurrencies(whereClause, whereParams);
            return ConvertListToKeyValueList(currencies);
        }

        public static Dictionary<int, Currency> ConvertListToKeyValueList(List<Currency> currencies)
        {
            var dictionary = new Dictionary<int, Currency>();
            foreach (Currency currency in currencies)
            {
                dictionary.Add(currency.ID, currency);
            }
            return dictionary;
        }

        public static Currency LoadDefaultCurrency()
        {
            var data = LoadCurrencies(" WHERE IsDefaultCurrency = @isDefault",
                new List<Tuple<string, string>>() { new Tuple<string, string>("@isDefault", "1") });
            return data.Count > 0 ? data[0] : null;
        }

        public static Currency LoadUSDCurrency()
        {
            var data = LoadCurrencies(" WHERE Abbreviation = @abbr",
                new List<Tuple<string, string>>() { new Tuple<string, string>("@abbr", "USD") });
            return data.Count > 0 ? data[0] : null;
        }

        public void Create()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    if (IsDefaultCurrency)
                    {
                        // need to remove the current default
                        string removeDefault = "UPDATE Currencies SET IsDefaultCurrency = 0";
                        command.CommandText = removeDefault;
                        command.ExecuteNonQuery();
                    }
                    string insert = "INSERT INTO Currencies (Name, Abbreviation, Symbol, ConversionRateToUSD, IsDefaultCurrency) " +
                        "VALUES (@name, @abbreviation, @symbol, @conversionRate, @isDefault)";
                    command.CommandText = insert;
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@abbreviation", Abbreviation);
                    command.Parameters.AddWithValue("@symbol", Symbol);
                    command.Parameters.AddWithValue("@conversionRate", ConversionRateToUSD);
                    command.Parameters.AddWithValue("@isDefault", IsDefaultCurrency);
                    command.ExecuteNonQuery();
                    ID = (int)conn.LastInsertRowId;
                }
                conn.Close();
            }
        }

        public void Save()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    if (IsDefaultCurrency)
                    {
                        // need to remove the current default
                        string removeDefault = "UPDATE Currencies SET IsDefaultCurrency = 0";
                        command.CommandText = removeDefault;
                        command.ExecuteNonQuery();
                    }
                    string insert = "" +
                        "UPDATE Currencies SET Name = @name, Abbreviation = @abbr, Symbol = @symbol, " +
                        "ConversionRateToUSD = @conversion, IsDefaultCurrency = @isDefault " +
                        "WHERE ID = @currencyID";
                    command.CommandText = insert;
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@abbr", Abbreviation);
                    command.Parameters.AddWithValue("@symbol", Symbol);
                    command.Parameters.AddWithValue("@conversion", ConversionRateToUSD);
                    command.Parameters.AddWithValue("@isDefault", IsDefaultCurrency);
                    command.Parameters.AddWithValue("@currencyID", ID);
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        /// <summary>
        /// You cannot delete the default Currency.
        /// </summary>
        public void Delete()
        {
            if (!IsDefaultCurrency)
            {
                var dbHelper = new DatabaseHelper();
                using (var conn = dbHelper.GetDatabaseConnection())
                {
                    using (var command = dbHelper.GetSQLiteCommand(conn))
                    {
                        /// set currency to null for items as applicable
                        string updateCommand = "UPDATE InventoryItems SET ItemPurchaseCostCurrencyID = NULL " +
                            "WHERE ItemPurchaseCostCurrencyID = @removingID";
                        command.CommandText = updateCommand;
                        command.Parameters.AddWithValue("@removingID", ID);
                        command.ExecuteNonQuery();
                        updateCommand = "UPDATE InventoryItems SET ProfitPerItemCurrencyID = NULL " +
                            "WHERE ProfitPerItemCurrencyID = @removingID";
                        command.CommandText = updateCommand;
                        command.Parameters.AddWithValue("@removingID", ID);
                        command.ExecuteNonQuery();
                        updateCommand = "UPDATE InventoryItems SET CostCurrencyID = NULL " +
                            "WHERE CostCurrencyID = @removingID";
                        command.CommandText = updateCommand;
                        command.Parameters.AddWithValue("@removingID", ID);
                        command.ExecuteNonQuery();
                        // ok, now delete this category
                        string deleteCommand = "DELETE FROM Currencies WHERE ID = @currencyID";
                        command.CommandText = deleteCommand;
                        command.Parameters.AddWithValue("@currencyID", ID);
                        command.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }
    }
}
