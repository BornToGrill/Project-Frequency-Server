using System;
using System.Linq;
using System.Security.Cryptography;

namespace LobbyController.Security {
    static class Cryptography {
        internal static bool CompareSaltHash(string password, string hash, string salt) {
            byte[] hashBytes = Convert.FromBase64String(hash);
            byte[] saltBytes = Convert.FromBase64String(salt);

            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, saltBytes)) {
                byte[] testKey = deriveBytes.GetBytes(20);
                return testKey.SequenceEqual(hashBytes);
            }
        }
    }
}
