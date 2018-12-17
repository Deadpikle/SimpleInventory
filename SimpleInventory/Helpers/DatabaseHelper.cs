using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;
using System.Security.Cryptography;
using SimpleInventory.Models;

namespace SimpleInventory.Helpers
{
    class DatabaseHelper
    {
        private const string _directory = "data";
        private const string _fileName = "inventory.sidb";

        private string GetFilePath()
        {
            return _directory + "/" + _fileName;
        }

        private bool DoesDatabaseExist()
        {
            return File.Exists(GetFilePath());
        }

        public SQLiteConnection GetDatabaseConnection()
        {
            if (!DoesDatabaseExist())
            {
                CreateDatabase();
            }
            return new SQLiteConnection("data source=" + GetFilePath());
        }

        /// <summary>
        /// Returns a command with an open db connection and foreign keys turned on
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public SQLiteCommand GetSQLiteCommand(SQLiteConnection conn)
        {
            var command = new SQLiteCommand(conn);
            conn.Open();
            command.CommandText = "PRAGMA foreign_keys = 1";
            command.ExecuteNonQuery();
            return command;
        }

        public int ReadInt(SQLiteDataReader reader, string columnName)
        {
            return (int)reader[columnName];
        }

        public string ReadString(SQLiteDataReader reader, string columnName)
        {
            return (string)reader[columnName];
        }

        public decimal ReadDecimal(SQLiteDataReader reader, string columnName)
        {
            string value = ReadString(reader, columnName);
            decimal decValue = 0.0m;
            if (Decimal.TryParse(value, out decValue))
            {
                return decValue;
            }
            return 0.0m;
        }

        private void CreateDatabase()
        {
            // create directory (if needed) and sqlite file 
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
            SQLiteConnection.CreateFile(GetFilePath());
            // now open and create the database
            using (var conn = GetDatabaseConnection())
            {
                using (var command = GetSQLiteCommand(conn))
                {
                    string createUsersTable = "CREATE TABLE Users (" +
                        "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT," +
                        "Username TEXT," +
                        "PasswordHash TEXT)";
                    command.CommandText = createUsersTable;
                    command.ExecuteNonQuery();

                    string createInventoryItemTable = "CREATE TABLE InventoryItems (" +
                        "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT," +
                        "Description TEXT," +
                        "PicturePath TEXT," +
                        "CostDollars TEXT," +
                        "CostRiel INTEGER," +
                        "Quantity INTEGER," +
                        "BarcodeNumber TEXT," +
                        "WasDeleted INTEGER DEFAULT 0," +
                        "CreatedByUserID INTEGER REFERENCES Users(ID))";
                    command.CommandText = createInventoryItemTable;
                    command.ExecuteNonQuery();

                    string createItemSoldInfoTable = "CREATE TABLE ItemsSoldInfo (" +
                        "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "DateTimeSold TEXT," +
                        "QuantitySold INTEGER DEFAULT 1," +
                        "CostDollars TEXT," +
                        "CostRiel INTEGER," +
                        "PaidDollars TEXT," +
                        "PaidRiel INTEGER," +
                        "ChangeDollars TEXT," +
                        "ChangeRiel INTEGER," +
                        "InventoryItemID INTEGER REFERENCES InventoryItems(ID)," +
                        "SoldByUserID INTEGER REFERENCES Users(ID) )";
                    command.CommandText = createItemSoldInfoTable;
                    command.ExecuteNonQuery();

                    string createGeneratedBarcodesTable = "CREATE TABLE GeneratedBarcodes (" +
                        "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Number TEXT," +
                        "DateTimeGenerated TEXT," +
                        "GeneratedByUserID INTEGER REFERENCES Users(ID) )";
                    command.CommandText = createGeneratedBarcodesTable;
                    command.ExecuteNonQuery();

                    // add an initial user
                    string addInitialUser = "" +
                        "INSERT INTO Users (Name, Username, PasswordHash) VALUES (@name, @username, @passwordHash)";
                    command.CommandText = addInitialUser;
                    command.Parameters.AddWithValue("@name", "Administrator");
                    command.Parameters.AddWithValue("@username", "admin");
                    command.Parameters.AddWithValue("@passwordHash", User.HashPassword("changeme"));
                    command.ExecuteNonQuery();

                    // close the connection
                    conn.Close();
                }
            }
        }
    }
}
