using Anonymous.Cryptography;
using Anonymous.Database;

namespace Anonymous.Global
{
    public class ApplicationProperties
    {
        // Rule 1: Every operation on these variables must be invoked on the main thread.
        public static string? masterPassword;
        public static AccountDataManager.EncryptedAccount? encryptedAccount;
        public static AccountDataManager.DefinedAccount? account;
        public static AESManager.Keyring? databaseKeyring;
    }
}
