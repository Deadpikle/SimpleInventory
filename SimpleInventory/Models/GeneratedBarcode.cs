using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SimpleInventory.Models
{
    class GeneratedBarcode
    {
        public int ID { get; set; }
        public long Number { get; set; }

        public DateTime DateTimeGenerated { get; set; }
        public int GeneratedByUserID { get; set; }

        public static long GetLatestBarcodeNumber()
        {
            long output = 1000000;
            string query = "" +
               "SELECT Number " +
               "FROM GeneratedBarcodes " +
               "ORDER BY Number DESC " +
               "LIMIT 1";
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    command.CommandText = query;
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            output = dbHelper.ReadLong(reader, "Number");
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return output;
        }

        public static void AddGeneratedCodes(List<long> numbers, DateTime generatedTime, int userID)
        {
            string query = "INSERT INTO GeneratedBarcodes (Number, DateTimeGenerated, GeneratedByUserID)" +
                " VALUES (@number, @dateTime, @userID)";

            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    command.CommandText = query;
                    foreach (long number in numbers)
                    {
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue("@number", number);
                        command.Parameters.AddWithValue("@dateTime", generatedTime.ToString(Utilities.DateTimeToStringFormat()));
                        command.Parameters.AddWithValue("@userID", userID);
                        command.ExecuteNonQuery();
                        command.Reset();
                    }
                }
                conn.Close();
            }
        }
    }
}
