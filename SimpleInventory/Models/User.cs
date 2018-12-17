using System;
using System.Collections.Generic;
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
        public string PasswordHash { get; set; }

        public static string HashPassword(string password)
        {
            var hasher = new SHA256Managed();
            var unhashed = System.Text.Encoding.Unicode.GetBytes("changeme");
            var hashed = hasher.ComputeHash(unhashed);
            return Convert.ToBase64String(hashed);
        }
    }
}
