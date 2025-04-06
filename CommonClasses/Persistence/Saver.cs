using CommonClasses.Persistence.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using Z.Dapper.Plus;

namespace CommonClasses.Persistence
{
    public class Saver
    {
        public static void Save(string filePath, Structure.SpendingSavingsTracker tracker)
        {
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
                }).ToList();

            DapperPlusManager.Entity<SavingsAccount>("SavingsAccount").Table("SavingsAccount").KeepIdentity(true).InsertIfNotExists(true).Identity(x => x.SavingsAccountKey);
            DapperPlusManager.Entity<SpendingCategory>("SpendingCategory").Table("SpendingCategory").KeepIdentity(true).InsertIfNotExists(true).Identity(x => x.SpendingCategoryKey);
            DapperPlusManager.Entity<ReportingPeriod>("ReportingPeriod").Table("ReportingPeriod").KeepIdentity(true).InsertIfNotExists(true).Identity(x => x.ReportingPeriodKey);
            DapperPlusManager.Entity<SavingsAccountTransactions>("SavingsAccountTransactions").Table("SavingsAccountTransactions").KeepIdentity(true).InsertIfNotExists(true);
            DapperPlusManager.Entity<Spending>("Spending").Table("Spending").KeepIdentity(true).InsertIfNotExists(true);
            DapperPlusManager.Entity<SpendingPlace>("SpendingPlace").Table("SpendingPlace").KeepIdentity(true).InsertIfNotExists(true).Identity(x => x.SpendingPlaceKey);

            sqlConnection.BulkInsert("SavingsAccount", savingsAccounts);
            sqlConnection.BulkInsert("SpendingCategory", spendingCategories);
            sqlConnection.BulkInsert("ReportingPeriod", reportingPeriods);
            sqlConnection.BulkInsert("SpendingPlace", spendingPlaces);
            sqlConnection.BulkInsert("SavingsAccountTransactions", savingCountTransactions);
            sqlConnection.BulkInsert("Spending", spending);
            
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
        }
    }
}
