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
    }
}
