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
            var data = LoadUsers(" WHERE u.Username = @username AND u.PasswordHash = @passwordHash ",
                new List<Tuple<string, string>>() { new Tuple<string, string>("@username", username),
                                                    new Tuple<string, string>("@passwordHash", HashPassword(password))});
            return data.Count > 0 ? data[0] : null;
        }

        public static List<User> LoadUsers(string whereClause = "", List<Tuple<string, string>> whereParams = null)
        {
            var users = new List<User>();
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string query = "" +
                        "SELECT u.ID AS UserID, u.Name, u.Username, up.ID AS PermissionID, CanAddEditItems, CanAdjustItemQuantity, " +
                                "CanViewDetailedItemQuantityAdjustments, CanScanItems, CanGenerateBarcodes, CanViewReports," +
                                "CanViewDetailedItemSoldInfo, CanSaveReportsToPDF, CanDeleteItemsFromInventory, CanManageItemCategories, CanManageUsers " +
                        "FROM Users u JOIN UserPermissions up ON u.ID = up.UserID " +
                            (string.IsNullOrEmpty(whereClause) ? " " : whereClause) + " " +
                        "ORDER BY u.Username, u.Name";
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
                            var user = new User();
                            user.ID = dbHelper.ReadInt(reader, "UserID");
                            user.Name = dbHelper.ReadString(reader, "Name");
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
                            user.Permissions.CanManageUsers = dbHelper.ReadBool(reader, "CanManageUsers");
                            users.Add(user);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return users;
        }
    }
}
