using Anonymous.Cryptography;
using Anonymous.Database;

namespace Anonymous.Security
{
    public class ApplicationAuth
    {
        private static string? masterPassword;
        private static AccountDataManager.DefinedAccount? account;
        private static AESManager.Keyring? keyring;

        public static bool IsMasterPasswordValid()
        {
            if (masterPassword == null)
            {
                return false;
            }
            AccountDataManager.EncryptedAccount? account = AccountDataManager.GetEncryptedAccount();
            if (account == null)
            {
                return false;
            }
            if (!AccountDataManager.IsMasterPasswordCorrect(account, masterPassword))
            {
                return false;
            }
            return true;
        }

        public static void SetMasterPassword(string newMasterPassword)
        {
            masterPassword = newMasterPassword;
            if (!IsMasterPasswordValid())
            {
                masterPassword = null;
            }
        }

        public static AccountDataManager.DefinedAccount? GetAccount()
        {
            if (account == null) {
                if (masterPassword == null)
                {
                    return null;
                }
                account = AccountDataManager.GetAccountWithoutKeyring(masterPassword);
                return account;
            } else
            {
                return account;
            }
        }

        public static AESManager.Keyring? GetDatabaseKeyring()
        {
            if(keyring == null)
            {
                if (
                    masterPassword == null || 
                    account == null) {
                    return null;
                }
                keyring = AESManager.GetKeyringByMasterPassword(masterPassword, account.DatabaseIV);
                return keyring;
            } else
            {
                return keyring;
            } 
        }
    }
}
