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
            Buffer.BlockCopy(passwordBytes, 0, passwordAndSalt, 0, passwordBytes.Length);
            Buffer.BlockCopy(iv, 0, passwordAndSalt, passwordBytes.Length, iv.Length);
            byte[] key = SHA256.HashData(passwordAndSalt);
            return new Keyring()
            {
                KeyBytes = key,
                IvBytes = iv
            };
        }
        
        public static Keyring GenerateKeyring()
        {
            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.GenerateKey();
            aes.GenerateIV();
            return new Keyring()
            {
                KeyBytes = aes.Key,
                IvBytes = aes.IV
            };
        }

        public static string Encrypt(string data, Keyring key)
        {
            using Aes aes = Aes.Create();
            aes.Key = key.KeyBytes;
            aes.IV = key.IvBytes;
            using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream cipherStream = new ();
            using CryptoStream cryptoStream = new (cipherStream, encryptor, CryptoStreamMode.Write);
            using StreamWriter streamWriter = new (cryptoStream);
            streamWriter.Write(data);
            return Convert.ToBase64String(cipherStream.ToArray());
        }

        public static string Decrypt(string cipher, Keyring key)
        {
            using Aes aes = Aes.Create();
            aes.Key = key.KeyBytes;
            aes.IV = key.IvBytes;
            using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream dataStream = new (Convert.FromBase64String(cipher));
            using CryptoStream cryptoStream = new (dataStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new (cryptoStream);
            return streamReader.ReadToEnd();
        }
    }
}
