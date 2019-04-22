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

        public string GetDatabaseFilePath()
        {
            return GetFilePath();
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

        public bool ReadBool(SQLiteDataReader reader, int columnNumber)
        {
            return reader.IsDBNull(columnNumber) ? false : reader.GetBoolean(columnNumber);
        }

        public int ReadInt(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
        }

        public int ReadInt(SQLiteDataReader reader, int columnNumber)
        {
            return reader.IsDBNull(columnNumber) ? 0 : reader.GetInt32(columnNumber);
        }

        public long ReadLong(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt64(ordinal);
        }

        public long ReadLong(SQLiteDataReader reader, int columnNumber)
        {
            return reader.IsDBNull(columnNumber) ? 0 : reader.GetInt64(columnNumber);
        }

        public string ReadString(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? "" : reader.GetString(ordinal);
        }

        public string ReadString(SQLiteDataReader reader, int columnNumber)
        {
            return reader.IsDBNull(columnNumber) ? "" : reader.GetString(columnNumber);
        }

        public decimal ReadDecimal(SQLiteDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
        }

        public decimal ReadDecimal(SQLiteDataReader reader, int columnNumber)
        {
            return reader.IsDBNull(columnNumber) ? 0m : reader.GetDecimal(columnNumber);
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
                            goto case 2; // weeee
                        case 2:
                            // add IsDrink column
                            string addIsDrinkColumn = "" +
                                "ALTER TABLE InventoryItems " +
                                "ADD COLUMN IsDrink INTEGER DEFAULT 0;";
                            command.CommandText = addIsDrinkColumn;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 2;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            goto case 3;
                        case 3:
                            // add ItemTypes table
                            command.CommandText = "PRAGMA foreign_keys = 0";
                            command.ExecuteNonQuery();
                            command.CommandText = "BEGIN TRANSACTION;";
                            command.ExecuteNonQuery();
                            string addItemTypesTable = "" +
                                "CREATE TABLE ItemTypes (" +
                                "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                                "Name TEXT," +
                                "Description TEXT)";
                            command.CommandText = addItemTypesTable;
                            command.ExecuteNonQuery();
                            string addInitialItemTypes = "" +
                                "INSERT INTO ItemTypes (Name, Description) VALUES (\"School supplies\", \"Pencils, pens, etc.\")";
                            command.CommandText = addInitialItemTypes;
                            command.ExecuteNonQuery();
                            addInitialItemTypes = "" +
                                "INSERT INTO ItemTypes (Name, Description) VALUES (\"Drinks\", \"Water, milk, etc.\")";
                            command.CommandText = addInitialItemTypes;
                            command.ExecuteNonQuery();
                            addInitialItemTypes = "" +
                                "INSERT INTO ItemTypes (Name, Description) VALUES (\"Meal tickets\", \"Tickets for student meals\")";
                            command.CommandText = addInitialItemTypes;
                            command.ExecuteNonQuery();
                            // Ugh, to change IsDrink to ItemTypeID with a FK, we have to recreate
                            // the entire table. :( :( :( 
                            // to do so, we create a new table, copy over the data, drop the old table,
                            // and rename the new table to the new table
                            string recreateInventoryItemTable = "CREATE TABLE New_InventoryItems (" +
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
                                "CreatedByUserID INTEGER REFERENCES Users(ID)," +
                                "ItemTypeID INTEGER REFERENCES ItemTypes(ID))";
                            command.CommandText = recreateInventoryItemTable;
                            command.ExecuteNonQuery();
                            string moveInventoryData = "" +
                                "INSERT INTO New_InventoryItems (Name, Description, PicturePath, " +
                                "Cost, CostCurrencyID, ProfitPerItem, ProfitPerItemCurrencyID," +
                                "Quantity, BarcodeNumber, WasDeleted, CreatedByUserID, ItemTypeID) " +
                                "SELECT Name, Description, PicturePath, " +
                                "Cost, CostCurrencyID, ProfitPerItem, ProfitPerItemCurrencyID," +
                                "Quantity, BarcodeNumber, WasDeleted, CreatedByUserID, 1 " +
                                "FROM InventoryItems " +
                                "ORDER BY ID;";
                            command.CommandText = moveInventoryData;
                            command.ExecuteNonQuery();
                            string removeOldTable = "" +
                                "DROP TABLE InventoryItems;";
                            command.CommandText = removeOldTable;
                            command.ExecuteNonQuery();
                            string renameNewTable = "" +
                                "ALTER TABLE New_InventoryItems RENAME TO InventoryItems;";
                            command.CommandText = renameNewTable;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 3;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            command.CommandText = "COMMIT TRANSACTION;";
                            command.ExecuteNonQuery();
                            command.CommandText = "PRAGMA foreign_keys = 1";
                            command.ExecuteNonQuery();
                            // cleanup
                            command.CommandText = "VACUUM;";
                            command.ExecuteNonQuery();
                            goto case 4;
                        case 4:
                            // add IsDefault column to ItemTypes
                            string addIsDefaultColumn = "" +
                                "ALTER TABLE ItemTypes " +
                                "ADD COLUMN IsDefault INTEGER DEFAULT 0;";
                            command.CommandText = addIsDefaultColumn;
                            command.ExecuteNonQuery();
                            // set default item type to school supplies
                            command.CommandText = " UPDATE ItemTypes SET IsDefault = 1 WHERE ID = 1";
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 4;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            goto case 5;
                        case 5:
                            command.CommandText = "PRAGMA foreign_keys = 0";
                            command.ExecuteNonQuery();
                            // oh bother, AmountChanged is the wrong column type. Should be int, is text. -_-
                            // Gotta recreate THAT table too...
                            string recreateQuantityAdjustmentsTable = "CREATE TABLE New_QuantityAdjustments (" +
                                "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                                "AmountChanged INTEGER," +
                                "DateTimeChanged TEXT," +
                                "InventoryItemID INTEGER REFERENCES InventoryItems(ID)," +
                                "AdjustedByUserID INTEGER REFERENCES Users(ID))";
                            command.CommandText = recreateQuantityAdjustmentsTable;
                            command.ExecuteNonQuery();
                            string moveQuantityAdjustmentData = "" +
                                "INSERT INTO New_QuantityAdjustments (AmountChanged, DateTimeChanged, InventoryItemID, AdjustedByUserID) " +
                                "SELECT AmountChanged, DateTimeChanged, InventoryItemID, AdjustedByUserID " +
                                "FROM QuantityAdjustments " +
                                "ORDER BY ID;";
                            command.CommandText = moveQuantityAdjustmentData;
                            command.ExecuteNonQuery();
                            string removeOldQuantityAdjustmentTable = "" +
                                "DROP TABLE QuantityAdjustments;";
                            command.CommandText = removeOldQuantityAdjustmentTable;
                            command.ExecuteNonQuery();
                            string renameNewQuantityAdjustmentsTable = "" +
                                "ALTER TABLE New_QuantityAdjustments RENAME TO QuantityAdjustments;";
                            command.CommandText = renameNewQuantityAdjustmentsTable;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 5;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            command.CommandText = "PRAGMA foreign_keys = 1";
                            command.ExecuteNonQuery();
                            goto case 6;
                        case 6:
                            // add user permissions table
                            string addUserPermissionsTable = "" +
                                "CREATE TABLE UserPermissions (" +
                                    "ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                                    "CanAddEditItems INTEGER," +
                                    "CanAdjustItemQuantity INTEGER," +
                                    "CanViewDetailedItemQuantityAdjustments INTEGER," +
                                    "CanScanItems INTEGER," +
                                    "CanGenerateBarcodes INTEGER," +
                                    "CanViewReports INTEGER," +
                                    "CanViewDetailedItemSoldInfo INTEGER," +
                                    "CanSaveReportsToPDF INTEGER," +
                                    "CanDeleteItemsFromInventory INTEGER," +
                                    "CanManageItemCategories INTEGER," +
                                    "UserID INTEGER REFERENCES Users(ID))";
                            command.CommandText = addUserPermissionsTable;
                            command.ExecuteNonQuery();
                            // add default user permission for default user
                            string addDefaultPermissions = "" +
                                "INSERT INTO UserPermissions (CanAddEditItems, CanAdjustItemQuantity, " +
                                "CanViewDetailedItemQuantityAdjustments, CanScanItems, CanGenerateBarcodes, CanViewReports," +
                                "CanViewDetailedItemSoldInfo, CanSaveReportsToPDF, CanDeleteItemsFromInventory, CanManageItemCategories," +
                                "UserID) " +
                                "VALUES (1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1)";
                            command.CommandText = addDefaultPermissions;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 6;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            goto case 7;
                        case 7:
                            // add CanManageUsers column to UserPermissions
                            string addCanManageUsersColumn = "" +
                                "ALTER TABLE UserPermissions " +
                                "ADD COLUMN CanManageUsers INTEGER DEFAULT 1;";
                            command.CommandText = addCanManageUsersColumn;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 7;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            goto case 8;
                        case 8:
                            // add WasDeleted column to Users
                            string addWasDeletedUsersColumn = "" +
                                "ALTER TABLE Users " +
                                "ADD COLUMN WasDeleted INTEGER DEFAULT 0;";
                            command.CommandText = addWasDeletedUsersColumn;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 8;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            goto case 9;
                        case 9:
                            // add CanDeleteItemsSold column to UserPermissions
                            string addCanDeleteItemsSold = "" +
                                "ALTER TABLE UserPermissions " +
                                "ADD COLUMN CanDeleteItemsSold INTEGER DEFAULT 1;";
                            command.CommandText = addCanDeleteItemsSold;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 9;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            goto case 10;
                        case 10:
                            // add Explanation column to QuantityAdjustments
                            string addExplanationColumn = "" +
                                "ALTER TABLE QuantityAdjustments " +
                                "ADD COLUMN Explanation TEXT DEFAULT '';";
                            command.CommandText = addExplanationColumn;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 10;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            goto case 11;
                        case 11:
                            // add CanManageUsers column to UserPermissions
                            string addCanViewManageInventoryQuantityColumn = "" +
                                "ALTER TABLE UserPermissions " +
                                "ADD COLUMN CanViewManageInventoryQuantity INTEGER DEFAULT 1;";
                            command.CommandText = addCanViewManageInventoryQuantityColumn;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 11;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            goto case 12;
                        case 12:
                            // add WasAdjustedForStockPurchase column to QuantityAdjustments
                            string addWasAdjustedForStockPurchase = "" +
                                "ALTER TABLE QuantityAdjustments " +
                                "ADD COLUMN WasAdjustedForStockPurchase INTEGER DEFAULT 0;";
                            command.CommandText = addWasAdjustedForStockPurchase;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 12;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            goto case 13;
                        case 13:
                            // add WasAdjustedForStockPurchase column to QuantityAdjustments
                            string addPurchaseCostAndItemsPerPurchase = "" +
                                "ALTER TABLE InventoryItems " +
                                "ADD COLUMN ItemPurchaseCost TEXT DEFAULT '0'; " +
                                "ALTER TABLE InventoryItems " +
                                "ADD COLUMN ItemPurchaseCostCurrencyID INTEGER DEFAULT 0;" +
                                "ALTER TABLE InventoryItems " +
                                "ADD COLUMN ItemsPerPurchase INTEGER DEFAULT 0";
                            command.CommandText = addPurchaseCostAndItemsPerPurchase;
                            command.ExecuteNonQuery();
                            // bump user_version
                            command.CommandText = "PRAGMA user_version = 13;";
                            command.ExecuteNonQuery();
                            command.Parameters.Clear();
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
                    command.Parameters.AddWithValue("@name", "US Dollar");
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
