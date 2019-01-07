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

        private SQLiteConnection GetDatabaseConnectionWithoutMigrating()
        {
            var conn = new SQLiteConnection("data source=" + GetFilePath());
            conn.Open();
            return conn;
        }

        public SQLiteConnection GetDatabaseConnection()
        {
            if (!DoesDatabaseExist())
            {
                CreateDatabase();
            }
            var conn = GetDatabaseConnectionWithoutMigrating();
            using (var command = new SQLiteCommand(conn))
            {
                command.CommandText = "PRAGMA foreign_keys = 1";
                command.ExecuteNonQuery();
                PerformMigrationsAsNecessary(command);
            }
            return conn;
        }

        /// <summary>
        /// Returns a command with an open db connection and foreign keys turned on
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public SQLiteCommand GetSQLiteCommand(SQLiteConnection conn)
        {
            return new SQLiteCommand(conn);
        }

        public bool ReadBool(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? false : reader.GetBoolean(ordinal);
        }

        public int ReadInt(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
        }

        public long ReadLong(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt64(ordinal);
        }

        public string ReadString(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
        }

        public decimal ReadDecimal(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
        }

        private void PerformMigrationsAsNecessary(SQLiteCommand command)
        {
            // uncomment when you need some migrations
            command.CommandText = "PRAGMA user_version";
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var userVersion = reader.GetInt32(0); // initial version is 0
                    reader.Close(); // have to close it now otherwise we can't execute commands
                    switch (userVersion + 1)
                    {
                        case 1:
                            // create QuantityAdjustments table
                            string createQuantityAdjustmentsTable = "CREATE TABLE QuantityAdjustments (" +
                                "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                                "AmountChanged TEXT," +
                                "DateTimeChanged TEXT," +
                                "InventoryItemID INTEGER REFERENCES InventoryItems(ID)," +
                                "AdjustedByUserID INTEGER REFERENCES Users(ID))";
                            command.CommandText = createQuantityAdjustmentsTable;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 1;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            command.CommandText = "PRAGMA user_version";
                            using (var reader2 = command.ExecuteReader())
                            {
                                if (reader2.Read())
                                {
                                    var userVersion2 = reader2.GetInt32(0); // initial version is 0
                                    var a = userVersion;
                                }
                                reader2.Close();
                            }
                                    break;
                    }
                }
                else
                {
                    reader.Close();
                }
            }
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
            using (var conn = GetDatabaseConnectionWithoutMigrating())
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

                    string createCurrenciesTable = "CREATE TABLE Currencies (" +
                        "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT," +
                        "Abbreviation TEXT," +
                        "Symbol TEXT," +
                        "ConversionRateToUSD TEXT," +
                        "IsDefaultCurrency INTEGER DEFAULT 0)";
                    command.CommandText = createCurrenciesTable;
                    command.ExecuteNonQuery();

                    string createInventoryItemTable = "CREATE TABLE InventoryItems (" +
                        "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT," +
                        "Description TEXT," +
                        "PicturePath TEXT," +
                        "Cost TEXT," +
                        "CostCurrencyID INTEGER REFERENCES Currencies(ID)," +
                        "ProfitPerItem TEXT," +
                        "ProfitPerItemCurrencyID INTEGER REFERENCES Currencies(ID)," +
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
                        "Cost TEXT," +
                        "CostCurrencyID INTEGER REFERENCES Currencies(ID)," +
                        "Paid TEXT," +
                        "PaidCurrencyID INTEGER REFERENCES Currencies(ID)," +
                        "Change TEXT," +
                        "ChangeCurrencyID INTEGER REFERENCES Currencies(ID)," +
                        "ProfitPerItem TEXT," +
                        "ProfitPerItemCurrencyID INTEGER REFERENCES Currencies(ID)," +
                        "InventoryItemID INTEGER REFERENCES InventoryItems(ID)," +
                        "SoldByUserID INTEGER REFERENCES Users(ID) )";
                    command.CommandText = createItemSoldInfoTable;
                    command.ExecuteNonQuery();

                    string createGeneratedBarcodesTable = "CREATE TABLE GeneratedBarcodes (" +
                        "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Number INTEGER," +
                        "DateTimeGenerated TEXT," +
                        "GeneratedByUserID INTEGER REFERENCES Users(ID) )";
                    command.CommandText = createGeneratedBarcodesTable;
                    command.ExecuteNonQuery();

                    // add initial data
                    // add default user
                    string addInitialUser = "" +
                        "INSERT INTO Users (Name, Username, PasswordHash) VALUES (@name, @username, @passwordHash)";
                    command.CommandText = addInitialUser;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@name", "Administrator");
                    command.Parameters.AddWithValue("@username", "admin");
                    command.Parameters.AddWithValue("@passwordHash", User.HashPassword("changeme"));
                    command.ExecuteNonQuery();

                    // add default currencies
                    string addCurrency = "" +
                        "INSERT INTO Currencies (Name, Abbreviation, Symbol, ConversionRateToUSD, IsDefaultCurrency) " +
                        "VALUES (@name, @abbreviation, @symbol, @conversion, @isDefault)";
                    command.CommandText = addCurrency;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@name", "US Dollars");
                    command.Parameters.AddWithValue("@abbreviation", "USD");
                    command.Parameters.AddWithValue("@symbol", "$");
                    command.Parameters.AddWithValue("@conversion", "1.0");
                    command.Parameters.AddWithValue("@isDefault", false);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@name", "Cambodian Riel");
                    command.Parameters.AddWithValue("@abbreviation", "KHR");
                    command.Parameters.AddWithValue("@symbol", "៛");
                    command.Parameters.AddWithValue("@conversion", "4050");
                    command.Parameters.AddWithValue("@isDefault", true);
                    command.ExecuteNonQuery();

                    command.CommandText = "PRAGMA user_version = 0";
                    command.Parameters.Clear();
                    command.ExecuteNonQuery();

                    // close the connection
                    conn.Close();
                }
            }
        }
    }
}
