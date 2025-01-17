using System.IO;
using SQLite;

namespace Anonymous.Database
{
    public class DatabaseManager
    {
        private static SQLiteConnection? globalConnection;

        public static SQLiteConnection GetConnection()
        {
            if (globalConnection == null)
            {
                string databasePath = Path.Combine(FileSystem.AppDataDirectory, "database.db");
                globalConnection = new SQLiteConnection(databasePath);
                return globalConnection;
            }
            else
            {
                return globalConnection;
            }
        }

        public static void InitializeTables()
        {
            SQLiteConnection connection = GetConnection();

            Type[] tables = [
                typeof(AccountDataManager.Account)
                ];

            connection.CreateTables(CreateFlags.None ,tables);
        }
    }
}
