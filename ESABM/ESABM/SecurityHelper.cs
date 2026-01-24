using System;
using System.Security.Cryptography;
using System.Text;

namespace ESABM {
    public static class SecurityHelper {
        public static string HashPassword(string password) {
            using(SHA256 sha = SHA256.Create()) {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
