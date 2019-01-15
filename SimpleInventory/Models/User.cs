using SimpleInventory.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInventory.Models
{
    class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }

        public UserPermissions Permissions { get; set; }

        public static string HashPassword(string password)
        {
            var hasher = new SHA256Managed();
            var unhashed = System.Text.Encoding.Unicode.GetBytes(password);
            var hashed = hasher.ComputeHash(unhashed);
            return Convert.ToBase64String(hashed);
        }

        public static User LoadUser(string username, string password)
        {
            User user = null;
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "" +
                        "SELECT u.ID AS UserID, u.Name, u.Username, up.ID AS PermissionID, CanAddEditItems, CanAdjustItemQuantity, " +
                                "CanViewDetailedItemQuantityAdjustments, CanScanItems, CanGenerateBarcodes, CanViewReports," +
                                "CanViewDetailedItemSoldInfo, CanSaveReportsToPDF, CanDeleteItemsFromInventory, CanManageItemCategories " +
                        "FROM Users u JOIN UserPermissions up ON u.ID = up.UserID " +
                        "WHERE u.Username = @username AND u.PasswordHash = @passwordHash";
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@passwordHash", HashPassword(password));
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            user = new User();
                            user.ID = dbHelper.ReadInt(reader, "UserID");
                            user.Name = dbHelper.ReadString(reader, "UserID");
                            user.Username = dbHelper.ReadString(reader, "Username");
                            // ALL the permissions!
                            user.Permissions = new UserPermissions();
                            user.Permissions.ID = dbHelper.ReadInt(reader, "PermissionID");
                            user.Permissions.UserID = user.ID;
                            user.Permissions.CanAddEditItems = dbHelper.ReadBool(reader, "CanAddEditItems");
                            user.Permissions.CanAdjustItemQuantity = dbHelper.ReadBool(reader, "CanAdjustItemQuantity");
                            user.Permissions.CanViewDetailedItemQuantityAdjustments = dbHelper.ReadBool(reader, "CanViewDetailedItemQuantityAdjustments");
                            user.Permissions.CanScanItems = dbHelper.ReadBool(reader, "CanScanItems");
                            user.Permissions.CanGenerateBarcodes = dbHelper.ReadBool(reader, "CanGenerateBarcodes");
                            user.Permissions.CanViewReports = dbHelper.ReadBool(reader, "CanViewReports");
                            user.Permissions.CanViewDetailedItemSoldInfo = dbHelper.ReadBool(reader, "CanViewDetailedItemSoldInfo");
                            user.Permissions.CanSaveReportsToPDF = dbHelper.ReadBool(reader, "CanSaveReportsToPDF");
                            user.Permissions.CanDeleteItemsFromInventory = dbHelper.ReadBool(reader, "CanDeleteItemsFromInventory");
                            user.Permissions.CanManageItemCategories = dbHelper.ReadBool(reader, "CanManageItemCategories");
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return user;
        }
    }
}
