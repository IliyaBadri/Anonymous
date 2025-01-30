using System.Security.Cryptography;
using System.Text;
using Anonymous.Cryptography;
using Anonymous.State;
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
            byte[] randomNumbers = new byte[4];
            RandomNumberGenerator.Fill(randomNumbers);

            Guid guid = Guid.NewGuid();

            string UID = "A-" + guid.ToString() + "-" + ((uint)BitConverter.ToInt32(randomNumbers, 0)).ToString();

            return UID;
        }

        public static DefinedAccount MakeNewAccount(string masterPassword)
        {
            ApplicationState.ProcessState processState = new("Hashing password", 3);
            ApplicationState.CallUpdate();
            string userID = GetNewUID();
            string masterKeyHash = Argon2Manager.Hash(masterPassword);
            processState.Increment();
            processState.title = "Encrypting account";
            ApplicationState.CallUpdate();
            AESManager.Keyring dummyKeyring = AESManager.GenerateKeyring();
            string databaseIV = Convert.ToBase64String(dummyKeyring.IvBytes);
            AESManager.Keyring keyring = AESManager.GetKeyringByMasterPassword(masterPassword, databaseIV);
            string encryptedUID = AESManager.Encrypt(userID, keyring);
            processState.Increment();
            processState.title = "Updating database";
            ApplicationState.CallUpdate();
            Account account = new()
            {
                UID = encryptedUID,
                MasterKeyHash = masterKeyHash,
                DatabaseIV = databaseIV
            };
            SQLiteConnection connection = DatabaseManager.GetConnection();
            connection.Insert(account);
            processState.Destroy();
            ApplicationState.CallUpdate();
            DefinedAccount definedAccount = new() {
                Id = 0,
                UID = userID,
                MasterKeyHash = masterKeyHash,
                DatabaseIV = databaseIV
            };
            return definedAccount;
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

        public static DefinedAccount? GetAccountWithoutKeyring(EncryptedAccount encryptedAccount, string masterPassword)
        {
            AESManager.Keyring keyring = AESManager.GetKeyringByMasterPassword(masterPassword, encryptedAccount.DatabaseIV);
            string decryptedUID = AESManager.Decrypt(encryptedAccount.UID, keyring);
            return new DefinedAccount()
            {
                Id = encryptedAccount.Id,
                UID = decryptedUID,
                MasterKeyHash = encryptedAccount.MasterKeyHash,
                DatabaseIV = encryptedAccount.DatabaseIV
            };
        }
    }
}
