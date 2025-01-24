using System.Security.Cryptography;
using System.Text;
namespace Anonymous.Cryptography
{
    public class AESManager
    {
        public class Keyring
        {
            public required byte[] KeyBytes { get; set; }
            public required byte[] IvBytes { get; set; }
        }

        public static Keyring GetKeyringByString(string key, string iv)
        {
            return new Keyring()
            {
                KeyBytes = Convert.FromBase64String(key),
                IvBytes = Convert.FromBase64String(iv)
            };
        }

        public static Keyring GetKeyringByMasterPassword(string masterPassword, string ivString)
        {
            byte[] iv = Convert.FromBase64String(ivString);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(masterPassword);
            byte[] passwordAndSalt = new byte[passwordBytes.Length + iv.Length];
            Array.Copy(passwordBytes, 0, passwordAndSalt, 0, passwordBytes.Length);
            Array.Copy(iv, 0, passwordAndSalt, passwordBytes.Length, iv.Length);
            byte[] key = SHA256.HashData(passwordAndSalt);
            return new Keyring()
            {
                KeyBytes = key,
                IvBytes = iv
            };
        }
        
        public static Keyring GenerateKeyring()
        {
            byte[] key;
            byte[] iv;
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.GenerateKey();
                aes.GenerateIV();

                key = aes.Key;
                iv = aes.IV;
            }
            
            return new Keyring()
            {
                KeyBytes = key,
                IvBytes = iv
            };
        }

        public static string Encrypt(string data, Keyring key)
        {
            byte[] encryptedBytes;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key.KeyBytes;
                aes.IV = key.IvBytes;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream memoryStream = new();
                using (CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using StreamWriter streamWriter = new(cryptoStream);
                    streamWriter.Write(data);
                }
                encryptedBytes = memoryStream.ToArray();
            }

            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string cipher, Keyring key)
        {
            string decryptedText;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key.KeyBytes;
                aes.IV = key.IvBytes;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream memoryStream = new(Convert.FromBase64String(cipher));
                using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
                using StreamReader streamReader = new(cryptoStream);
                decryptedText = streamReader.ReadToEnd();
            }

            return decryptedText;
        }
    }
}
