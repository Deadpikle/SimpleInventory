using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

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

        SQLiteConnection GetDatabaseConnection()
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
        SQLiteCommand GetSQLiteCommand(SQLiteConnection conn)
        {
            var command = new SQLiteCommand(conn);
            conn.Open();
            command.CommandText = "PRAGMA foreign_keys = 1";
            command.ExecuteNonQuery();
            return command;
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
                        "WasDeleted INTEGER," +
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
                    conn.Close();
                }
            }
        }
    }
}
