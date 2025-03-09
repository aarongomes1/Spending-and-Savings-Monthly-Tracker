using CommonClasses.Persistence.Models;
using Microsoft.Data.Sqlite;
using RepoDb;

namespace CommonClasses.Persistence
{
    public class Saver
    {
        public static void Save(string filePath, Structure.SpendingSavingsTracker tracker)
        {
            GlobalConfiguration.Setup().UseSqlite();

            ResetDb(filePath);
            CreateDatabase(filePath);

            using var sqlConnection = new SqliteConnection($"Data Source={filePath};");
            sqlConnection.Open();

            var savingsAccounts = tracker.SavingsAccounts
                .Select(x => new SavingsAccount() {
                    SavingsAccountKey = x.SavingsAccountKey.ToString(),
                    SavingsAccountName = x.SavingsAccountName,
                    Balance = x.Balance,
                    IsISA = x.IsISA ? 1 : 0
                }).ToList();

            var spendingCategories = tracker.SpendingCategories
                .Select(x => new SpendingCategory() {
                    SpendingCategoryKey = x.SpendingCategoryKey.ToString(),
                    SpendingCategoryName = x.SpendingCategoryName
                }).ToList();

            var reportingPeriods = tracker.ReportingPeriods
                .Select(x => new ReportingPeriod() {
                    ReportingPeriodKey = x.ReportingPeriodKey.ToString(),
                    EndDate = x.EndDate.ToString("dd/MM/yyyy"),
                    StartDate = x.StartDate.ToString("dd/MM/yyyy")
                }).ToList();

            var savingCountTransactions = tracker.SavingsAccounts.SelectMany(x => x.Transactions)
                .Select(x => new SavingsAccountTransactions() {
                    SavingsAccountKey = x.SavingsAccount.SavingsAccountKey.ToString(),
                    ReportingPeriodKey = x.ReportingPeriod.ReportingPeriodKey.ToString(),
                    Change = x.Change,
                    TransactionDate = x.TransactionDate.ToString("dd/MM/yyyy"),
                    CountsToISALimit = x.CountsToISALimit == true ? 1 : 0,
                    BalanceAfterTransaction = x.BalanceAfterTransaction
                }).ToList();

            var spending = tracker.SpendingCategories.SelectMany(x => x.Transactions)
                .Select(x => new Spending() {
                    SpendingPlaceKey = x.SpendingPlace.SpendingPlaceKey.ToString(),
                    ReportingPeriodKey = x.ReportingPeriod.ReportingPeriodKey.ToString(),
                    Amount = x.Amount,
                    NumberOfTransactions = x.NumberOfTransactions
                }).ToList();

            var spendingPlaces = tracker.SpendingCategories.SelectMany(x => x.Transactions)
                .Select(x => new SpendingPlace() {
                    SpendingPlaceKey = x.SpendingPlace.SpendingPlaceKey.ToString(),
                    SpendingPlaceName = x.SpendingPlace.SpendingPlaceName,
                    SpendingCategoryKey = x.SpendingPlace.SpendingCategory.SpendingCategoryKey.ToString()
                }).DistinctBy(x => x.SpendingPlaceKey).ToList();

            sqlConnection.InsertAll("SavingsAccount", savingsAccounts);
            sqlConnection.InsertAll("SpendingCategory", spendingCategories);
            sqlConnection.InsertAll("ReportingPeriod", reportingPeriods);
            sqlConnection.InsertAll("SpendingPlace", spendingPlaces);
            sqlConnection.InsertAll("SavingsAccountTransactions", savingCountTransactions);
            sqlConnection.InsertAll("Spending", spending);   
        }

        // Reset the db so we can recreate it
        // This allows us to 'start afresh' and not take any baggage any previous runs might have left
        private static void ResetDb(string filePath)
        {
            using var connection = new SqliteConnection($"Data Source={filePath};");
            connection.Open();

            var tableNames = connection.ExecuteQuery<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';"
            ).ToList();

            connection.ExecuteNonQuery("PRAGMA foreign_keys = OFF;");

            foreach (var table in tableNames)
            {
                connection.ExecuteNonQuery($"DELETE FROM [{table}];");
            }

            connection.ExecuteNonQuery("PRAGMA foreign_keys = ON;");
        }

        public static void CreateDatabase(string filePath)
        {
            using var newDb = new SqliteConnection($"Data Source={filePath};");

            newDb.Open();

            var reportingPeriodTable = @"
            CREATE TABLE IF NOT EXISTS ""ReportingPeriod"" (
	            ""ReportingPeriodKey""	TEXT NOT NULL UNIQUE,
	            ""StartDate""	TEXT NOT NULL,
	            ""EndDate""	TEXT,
	            PRIMARY KEY(""ReportingPeriodKey"")
            );";

            var spendingCategoryTable = @"
            CREATE TABLE IF NOT EXISTS ""SpendingCategory"" (
	            ""SpendingCategoryKey""	TEXT NOT NULL UNIQUE,
	            ""SpendingCategoryName""	TEXT NOT NULL,
	            PRIMARY KEY(""SpendingCategoryKey"")
            );";

            var savingsAccountTable = @"
            CREATE TABLE IF NOT EXISTS ""SavingsAccount"" (
	            ""SavingsAccountKey""	TEXT NOT NULL UNIQUE,
	            ""SavingsAccountName""	TEXT NOT NULL UNIQUE,
	            ""Balance""	REAL NOT NULL,
	            ""IsISA""	INTEGER NOT NULL,
	            PRIMARY KEY(""SavingsAccountKey"")
            );";

            var spendingPlaceTable = @"
            CREATE TABLE IF NOT EXISTS ""SpendingPlace"" (
	            ""SpendingPlaceKey""	TEXT NOT NULL UNIQUE,
	            ""SpendingPlaceName""	TEXT NOT NULL,
	            ""SpendingCategoryKey""	TEXT NOT NULL,
	            PRIMARY KEY(""SpendingPlaceKey""),
	            FOREIGN KEY(""SpendingCategoryKey"") REFERENCES ""SpendingCategory""(""SpendingCategoryKey"")
            );";

            var spendingTable = @"
            CREATE TABLE IF NOT EXISTS ""Spending"" (
	            ""SpendingPlaceKey""	TEXT NOT NULL,
	            ""ReportingPeriodKey""	TEXT NOT NULL,
	            ""Amount""	REAL NOT NULL,
	            ""NumberOfTransactions""	INTEGER NOT NULL,
	            PRIMARY KEY(""SpendingPlaceKey"",""ReportingPeriodKey""),
	            FOREIGN KEY(""ReportingPeriodKey"") REFERENCES ""ReportingPeriod""(""ReportingPeriodKey""),
	            FOREIGN KEY(""SpendingPlaceKey"") REFERENCES ""SpendingPlace""(""SpendingPlaceKey"")
            );";

            var savingsAccountTransactionTable = @"
            CREATE TABLE IF NOT EXISTS ""SavingsAccountTransactions"" (
	            ""SavingsAccountKey""	TEXT NOT NULL,
	            ""ReportingPeriodKey""	TEXT NOT NULL,
	            ""Change""	REAL NOT NULL,
	            ""TransactionDate""	TEXT NOT NULL,
                ""CountsToISALimit"" INT NULL,
                ""BalanceAfterTransaction""	REAL NOT NULL,
	            PRIMARY KEY(""SavingsAccountKey"",""ReportingPeriodKey"", ""CountsToISALimit"", ""TransactionDate""),
	            FOREIGN KEY(""ReportingPeriodKey"") REFERENCES ""ReportingPeriod""(""ReportingPeriodKey""),
	            FOREIGN KEY(""SavingsAccountKey"") REFERENCES ""SavingsAccount""(""SavingsAccountKey"")
)           ;";

            newDb.ExecuteNonQuery(reportingPeriodTable);
            newDb.ExecuteNonQuery(spendingCategoryTable);
            newDb.ExecuteNonQuery(savingsAccountTable);
            newDb.ExecuteNonQuery(spendingPlaceTable);
            newDb.ExecuteNonQuery(spendingTable);
            newDb.ExecuteNonQuery(savingsAccountTransactionTable);
        }

        public static void CreateDatabase(string filePath)
        {
            SQLiteConnection.CreateFile(filePath);

            using var newDb = new SQLiteConnection($"Data Source={filePath}; Version = 3; New = True; Compress = True;");

            newDb.Open();

            var reportingPeriodTable = @"
            CREATE TABLE ""ReportingPeriod"" (
	            ""ReportingPeriodKey""	TEXT NOT NULL UNIQUE,
	            ""StartDate""	TEXT NOT NULL,
	            ""EndDate""	TEXT,
	            PRIMARY KEY(""ReportingPeriodKey"")
            );";

            var spendingCategoryTable = @"
            CREATE TABLE ""SpendingCategory"" (
	            ""SpendingCategoryKey""	TEXT NOT NULL UNIQUE,
	            ""SpendingCategoryName""	TEXT NOT NULL,
	            PRIMARY KEY(""SpendingCategoryKey"")
            );";

            var savingsAccountTable = @"
            CREATE TABLE ""SavingsAccount"" (
	            ""SavingsAccountKey""	TEXT NOT NULL UNIQUE,
	            ""SavingsAccountName""	TEXT NOT NULL UNIQUE,
	            ""Balance""	REAL NOT NULL,
	            ""IsISA""	INTEGER NOT NULL,
	            PRIMARY KEY(""SavingsAccountKey"")
            );";

            var spendingPlaceTable = @"
            CREATE TABLE ""SpendingPlace"" (
	            ""SpendingPlaceKey""	TEXT NOT NULL UNIQUE,
	            ""SpendingPlaceName""	TEXT NOT NULL,
	            ""SpendingCategoryKey""	TEXT NOT NULL,
	            PRIMARY KEY(""SpendingPlaceKey""),
	            FOREIGN KEY(""SpendingCategoryKey"") REFERENCES ""SpendingCategory""(""SpendingCategoryKey"")
            );";

            var spendingTable = @"
            CREATE TABLE ""Spending"" (
	            ""SpendingPlaceKey""	TEXT NOT NULL,
	            ""ReportingPeriodKey""	TEXT NOT NULL,
	            ""Amount""	REAL NOT NULL,
	            ""NumberOfTransactions""	INTEGER NOT NULL,
	            PRIMARY KEY(""SpendingPlaceKey"",""ReportingPeriodKey""),
	            FOREIGN KEY(""ReportingPeriodKey"") REFERENCES ""ReportingPeriod""(""ReportingPeriodKey""),
	            FOREIGN KEY(""SpendingPlaceKey"") REFERENCES ""SpendingPlace""(""SpendingPlaceKey"")
            );";

            var savingsAccountTransactionTable = @"
            CREATE TABLE ""SavingsAccountTransactions"" (
	            ""SavingsAccountKey""	TEXT NOT NULL,
	            ""ReportingPeriodKey""	TEXT NOT NULL,
	            ""Change""	REAL NOT NULL,
	            ""TransactionDate""	TEXT NOT NULL,
                ""CountsToISALimit"" INT NULL,
                ""BalanceAfterTransaction""	REAL NOT NULL,
	            PRIMARY KEY(""SavingsAccountKey"",""ReportingPeriodKey"", ""CountsToISALimit""),
	            FOREIGN KEY(""ReportingPeriodKey"") REFERENCES ""ReportingPeriod""(""ReportingPeriodKey""),
	            FOREIGN KEY(""SavingsAccountKey"") REFERENCES ""SavingsAccount""(""SavingsAccountKey"")
)           ;";

            newDb.Execute(reportingPeriodTable);
            newDb.Execute(spendingCategoryTable);
            newDb.Execute(savingsAccountTable);
            newDb.Execute(spendingPlaceTable);
            newDb.Execute(spendingTable);
            newDb.Execute(savingsAccountTransactionTable);

            newDb.Shutdown();
        }
    }
}
