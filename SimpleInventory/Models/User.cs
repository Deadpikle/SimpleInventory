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
        public bool WasDeleted { get; set; }

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
            var data = LoadUsers(" WHERE u.Username = @username AND u.PasswordHash = @passwordHash AND WasDeleted = 0 ",
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
                                "CanViewDetailedItemSoldInfo, CanSaveReportsToPDF, CanDeleteItemsFromInventory, CanManageItemCategories, CanManageUsers," +
                                "CanDeleteItemsSold, CanViewManageInventoryQuantity " +
                        "FROM Users u JOIN UserPermissions up ON u.ID = up.UserID " +
                            (string.IsNullOrEmpty(whereClause) ? " WHERE WasDeleted = 0 " : whereClause) + " " +
                        "ORDER BY lower(u.Username), lower(u.Name)";
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
                            user.WasDeleted = dbHelper.ReadBool(reader, "WasDeleted");
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
                            user.Permissions.CanDeleteItemsSold = dbHelper.ReadBool(reader, "CanDeleteItemsSold");
                            user.Permissions.CanViewManageInventoryQuantity = dbHelper.ReadBool(reader, "CanViewManageInventoryQuantity");
                            users.Add(user);
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return users;
        }

        public void Create(string password)
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string insert = "INSERT INTO Users (Name, Username, PasswordHash, WasDeleted) " +
                        "VALUES (@name, @username, @password, 0)";
                    command.CommandText = insert;
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@username", Username);
                    command.Parameters.AddWithValue("@password", HashPassword(password));
                    command.ExecuteNonQuery();
                    ID = (int)conn.LastInsertRowId;
                    // now insert into user permissions!
                    insert = "INSERT INTO UserPermissions (CanAddEditItems, CanAdjustItemQuantity, " +
                                "CanViewDetailedItemQuantityAdjustments, CanScanItems, CanGenerateBarcodes, CanViewReports," +
                                "CanViewDetailedItemSoldInfo, CanSaveReportsToPDF, CanDeleteItemsFromInventory, CanManageItemCategories, " +
                                "CanManageUsers, CanDeleteItemsSold, CanViewManageInventoryQuantity, UserID) " +
                        "VALUES (@canEditItems, @canAdjustQuantity, @canViewDetailedItemQuantityAdjustments, @canScan, @canGenerate, @canViewReports," +
                                "@canViewDetailedItemSoldInfo, @canSaveReports, @canDeleteItems, @canManageCategories, @canManageUsers, " +
                                "@canDeleteItemsSold, @canViewManageInventoryQuantity, @userID)";
                    command.CommandText = insert;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@canEditItems", Permissions.CanAddEditItems);
                    command.Parameters.AddWithValue("@canAdjustQuantity", Permissions.CanAdjustItemQuantity);
                    command.Parameters.AddWithValue("@canViewDetailedItemQuantityAdjustments", Permissions.CanViewDetailedItemQuantityAdjustments);
                    command.Parameters.AddWithValue("@canScan", Permissions.CanScanItems);
                    command.Parameters.AddWithValue("@canGenerate", Permissions.CanGenerateBarcodes);
                    command.Parameters.AddWithValue("@canViewReports", Permissions.CanViewReports);
                    command.Parameters.AddWithValue("@canViewDetailedItemSoldInfo", Permissions.CanViewDetailedItemSoldInfo);
                    command.Parameters.AddWithValue("@canSaveReports", Permissions.CanSaveReportsToPDF);
                    command.Parameters.AddWithValue("@canDeleteItems", Permissions.CanDeleteItemsFromInventory);
                    command.Parameters.AddWithValue("@canManageCategories", Permissions.CanManageItemCategories);
                    command.Parameters.AddWithValue("@canManageUsers", Permissions.CanManageUsers);
                    command.Parameters.AddWithValue("@canDeleteItemsSold", Permissions.CanDeleteItemsSold);
                    command.Parameters.AddWithValue("@canViewManageInventoryQuantity", Permissions.CanViewManageInventoryQuantity);
                    command.Parameters.AddWithValue("@userID", ID);
                    command.ExecuteNonQuery();
                    Permissions.ID = (int)conn.LastInsertRowId;
                }
                conn.Close();
            }
        }

        public void Save(string password = null)
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string update = "UPDATE Users SET Name = @name, Username = @username " +
                        (password == null ? " " : ", PasswordHash = @password ") +
                        "WHERE ID = @userID";
                    command.CommandText = update;
                    command.Parameters.AddWithValue("@name", Name);
                    command.Parameters.AddWithValue("@username", Username);
                    if (password != null)
                    {
                        command.Parameters.AddWithValue("@password", HashPassword(password));
                    }
                    command.Parameters.AddWithValue("@userID", ID);
                    command.ExecuteNonQuery();
                    // now update user permissions!
                    update = "UPDATE UserPermissions SET CanAddEditItems = @canEditItems, CanAdjustItemQuantity = @canAdjustQuantity, " +
                                "CanViewDetailedItemQuantityAdjustments = @canViewDetailedItemQuantityAdjustments, " +
                                "CanScanItems = @canScan, CanGenerateBarcodes = @canGenerate, CanViewReports = @canViewReports," +
                                "CanViewDetailedItemSoldInfo = @canViewDetailedItemSoldInfo, " +
                                "CanSaveReportsToPDF = @canSaveReports, CanDeleteItemsFromInventory = @canDeleteItems, " +
                                "CanManageItemCategories = @canManageCategories, CanManageUsers = @canManageUsers," +
                                "CanDeleteItemsSold = @canDeleteItemsSold, CanViewManageInventoryQuantity = @canViewManageInventoryQuantity " +
                                "WHERE ID = @permissionID";
                    command.CommandText = update;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@canEditItems", Permissions.CanAddEditItems);
                    command.Parameters.AddWithValue("@canAdjustQuantity", Permissions.CanAdjustItemQuantity);
                    command.Parameters.AddWithValue("@canViewDetailedItemQuantityAdjustments", Permissions.CanViewDetailedItemQuantityAdjustments);
                    command.Parameters.AddWithValue("@canScan", Permissions.CanScanItems);
                    command.Parameters.AddWithValue("@canGenerate", Permissions.CanGenerateBarcodes);
                    command.Parameters.AddWithValue("@canViewReports", Permissions.CanViewReports);
                    command.Parameters.AddWithValue("@canViewDetailedItemSoldInfo", Permissions.CanViewDetailedItemSoldInfo);
                    command.Parameters.AddWithValue("@canSaveReports", Permissions.CanSaveReportsToPDF);
                    command.Parameters.AddWithValue("@canDeleteItems", Permissions.CanDeleteItemsFromInventory);
                    command.Parameters.AddWithValue("@canManageCategories", Permissions.CanManageItemCategories);
                    command.Parameters.AddWithValue("@canManageUsers", Permissions.CanManageUsers);
                    command.Parameters.AddWithValue("@canDeleteItemsSold", Permissions.CanDeleteItemsSold);
                    command.Parameters.AddWithValue("@canViewManageInventoryQuantity", Permissions.CanViewManageInventoryQuantity);
                    command.Parameters.AddWithValue("@permissionID", Permissions.ID);
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public void UpdatePassword(string password)
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    string update = "UPDATE Users SET PasswordHash = @password WHERE ID = @userID";
                    command.CommandText = update;
                    command.Parameters.AddWithValue("@password", HashPassword(password));
                    command.Parameters.AddWithValue("@userID", ID);
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public void Delete()
        {
            var dbHelper = new DatabaseHelper();
            using (var conn = dbHelper.GetDatabaseConnection())
            {
                using (var command = dbHelper.GetSQLiteCommand(conn))
                {
                    // ok, now delete this category
                    string deleteCommand = "UPDATE Users SET WasDeleted = 1 WHERE ID = @userID";
                    command.CommandText = deleteCommand;
                    command.Parameters.AddWithValue("@userID", ID);
                    command.ExecuteNonQuery();
                    WasDeleted = true;
                }
                conn.Close();
            }
        }
    }
}
