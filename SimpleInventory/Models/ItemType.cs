using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    public class ItemType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }

        public ItemType()
        {
            ID = 0;
            Name = "";
            Description = "";
            IsDefault = false;
        }

        public ItemType(int id, string name, string description, bool isDefault)
        {
            ID = id;
            Name = name;
            Description = description;
            IsDefault = isDefault;
        }

        public static List<ItemType> LoadItemTypes(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var items = new List<ItemType>();
            string query = "" +
                "SELECT ID, Name, Description, IsDefault " +
                "FROM ItemTypes " +
                (string.IsNullOrEmpty(whereClause) ? "" : whereClause) + " " +
                "ORDER BY lower(Name), lower(Description)";
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
                            itemType.IsDefault = dbHelper.ReadBool(reader, "IsDefault");
                            items.Add(itemType);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return items;
        }

        public static ItemType LoadDefaultItemType()
        {
            var data = LoadItemTypes(" WHERE IsDefault = @isDefault",
                new List<Tuple<string, string>>() { new Tuple<string, string>("@isDefault", "1") });
            return data.Count > 0 ? data[0] : null;
        }

        public void Create()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    if (IsDefault)
                    {
                        // need to remove the current default
                        string removeDefault = "UPDATE ItemTypes SET IsDefault = 0";
                        command.CommandText = removeDefault;
                        command.ExecuteNonQuery();
                    }
                    string insert = "INSERT INTO ItemTypes (Name, Description, IsDefault) " +
                        "VALUES (@name, @description, @isDefault)";
                    command.CommandText = insert;
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@description", Description);
                    command.Parameters.AddWithValue("@isDefault", IsDefault);
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
                    if (IsDefault)
                    {
                        // need to remove the current default
                        string removeDefault = "UPDATE ItemTypes SET IsDefault = 0";
                        command.CommandText = removeDefault;
                        command.ExecuteNonQuery();
                    }
                    string insert = "UPDATE ItemTypes SET Name = @name, Description = @description, IsDefault = @isDefault " +
                        "WHERE ID = @itemTypeID";
                    command.CommandText = insert;
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@description", Description);
                    command.Parameters.AddWithValue("@isDefault", IsDefault);
                    command.Parameters.AddWithValue("@itemTypeID", ID);
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        /// <summary>
        /// You cannot delete the default ItemType.
        /// </summary>
        public void Delete()
        {
            if (!IsDefault)
            {
                var dbHelper = new DatabaseHelper();
                using (var conn = dbHelper.GetDatabaseConnection())
                {
                    using (var command = dbHelper.GetSQLiteCommand(conn))
                    {
                        // move items from this category to the default category
                        var defaultType = LoadDefaultItemType();
                        if (defaultType != null)
                        {
                            string updateCommand = "UPDATE InventoryItems SET ItemTypeID = @itemTypeID WHERE ItemTypeID = @removingItemID";
                            command.CommandText = updateCommand;
                            command.Parameters.AddWithValue("@itemTypeID", defaultType.ID);
                            command.Parameters.AddWithValue("@removingItemID", ID);
                            command.ExecuteNonQuery();
                        }
                        // ok, now delete this category
                        string deleteCommand = "DELETE FROM ItemTypes WHERE ID = @itemTypeID";
                        command.CommandText = deleteCommand;
                        command.Parameters.AddWithValue("@itemTypeID", ID);
                        command.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }
    }
}
