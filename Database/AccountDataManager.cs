using System.Security.Cryptography;
using Anonymous.Cryptography;
using SQLite;
namespace Anonymous.Database
{
    public class AccountDataManager
    {
        public class Account
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public string? UID { get; set; } // ENCRYPTED
            public string? MasterKeyHash { get; set; }
            public string? DatabaseIV { get; set; }
        }

        public class EncryptedAccount
        {
            public required int Id { get; set; }
            public required string UID { get; set; } // ENCRYPTED
            public required string MasterKeyHash { get; set; }
            public required string DatabaseIV { get; set; }
        }

        public class DefinedAccount
        {
            public required int Id { get; set; }
            public required string UID { get; set; }
            public required string MasterKeyHash { get; set; }
            public required string DatabaseIV { get; set; }
        }

        private static string GetNewUID()
        {
            byte[] randomNumbers = new byte[16];
            RandomNumberGenerator.Fill(randomNumbers);

            Guid guid = Guid.NewGuid();

            string UID = "A-" + guid.ToString() + "-" + randomNumbers.ToString();
            return UID;
        }

        public static void MakeNewAccount(string masterPassword)
        {
            if(GetEncryptedAccount() != null)
            {
                return;
            }
            string userID = GetNewUID();
            string masterKeyHash = Argon2Manager.Hash(masterPassword);
            AESManager.Keyring dummyKeyring = AESManager.GenerateKeyring();
            string databaseIV = Convert.ToBase64String(dummyKeyring.IvBytes);
            AESManager.Keyring keyring = AESManager.GetKeyringByMasterPassword(masterPassword, databaseIV);
            string encryptedUID = AESManager.Encrypt(userID, keyring);
            Account account = new()
            {
                UID = encryptedUID,
                MasterKeyHash = masterKeyHash,
                DatabaseIV = databaseIV
            };
            SQLiteConnection connection = DatabaseManager.GetConnection();
            connection.Insert(account);
        }

        public static EncryptedAccount? GetEncryptedAccount()
        {
            SQLiteConnection connection = DatabaseManager.GetConnection();
            List<Account> accounts = [.. connection.Table<Account>()];
            if (accounts.Count < 1)
            {
                return null;
            }
            Account account = accounts[0];
            if (account.UID == null || account.MasterKeyHash == null || account.DatabaseIV == null)
            {
                return null;
            }
            return new EncryptedAccount()
            {
                Id = account.Id,
                UID = account.UID,
                MasterKeyHash = account.MasterKeyHash,
                DatabaseIV = account.DatabaseIV
            };
        }

        public static bool IsMasterPasswordCorrect(EncryptedAccount account, string masterPassword)
        {
            return Argon2Manager.VerifyHash(masterPassword, account.MasterKeyHash);
        }

        public static DefinedAccount? GetAccountWithoutKeyring(string masterPassword)
        {
            EncryptedAccount? account = GetEncryptedAccount();
            if(account == null)
            {
                return null;
            }
            AESManager.Keyring keyring = AESManager.GetKeyringByMasterPassword(masterPassword, account.DatabaseIV);
            string decryptedUID = AESManager.Decrypt(account.UID, keyring);
            return new DefinedAccount()
            {
                Id = account.Id,
                UID = decryptedUID,
                MasterKeyHash = account.MasterKeyHash,
                DatabaseIV = account.DatabaseIV
            };
        }
    }
}
