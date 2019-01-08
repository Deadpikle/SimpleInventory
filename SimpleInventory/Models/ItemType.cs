using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class ItemType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public static List<ItemType> LoadItemTypes(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var items = new List<ItemType>();
            string query = "" +
                "SELECT ID, Name, Description " +
                "FROM ItemTypes " +
                (string.IsNullOrEmpty(whereClause) ? "" : whereClause) + " " +
                "ORDER BY Name, Description";
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
                            var itemType = new ItemType();
                            itemType.ID = dbHelper.ReadInt(reader, "ID");
                            itemType.Name = dbHelper.ReadString(reader, "Name");
                            itemType.Description = dbHelper.ReadString(reader, "Description");
                            items.Add(itemType);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return items;
        }
    }
}
