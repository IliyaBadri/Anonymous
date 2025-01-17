using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Anonymous.Cryptography
{
    public class Argon2Manager
    {
        public static string Hash(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] salt = new byte[16];
            new Random().NextBytes(salt);
            using Argon2id argon2 = new (passwordBytes);
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 4;
            argon2.MemorySize = 65536;
            argon2.Iterations = 3;
            byte[] hash = argon2.GetBytes(32);
            byte[] resultBytes = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, resultBytes, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, resultBytes, salt.Length, hash.Length);
            return Convert.ToBase64String(resultBytes);
        }
        public static bool VerifyHash(string password, string hashString)
        {
            byte[] resultBytes = Convert.FromBase64String(hashString);
            byte[] salt = new byte[16];
            byte[] hash = new byte[32];
            Buffer.BlockCopy(resultBytes, 0, salt, 0, 16);
            Buffer.BlockCopy(resultBytes, 16, hash, 0, 32);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            using var argon2 = new Argon2id(passwordBytes);
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 4;
            argon2.MemorySize = 65536;
            argon2.Iterations = 3;
            byte[] testHash = argon2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(testHash, hash);
        }
    }
}
