using CommonClasses.Persistence.Models;
using System.Data.SQLite;
using Z.Dapper.Plus;

namespace CommonClasses.Persistence
{
    public class Saver
    {
        public static void Save(string filePath, Structure.SpendingSavingsTracker tracker)
        {
            using var sqlConnection = new SQLiteConnection($"Data Source={filePath}; Version = 3; New = False; Compress = True;");
            sqlConnection.Open();

            var savingsAccounts = tracker.SavingsAccounts.Select(x => new SavingsAccount() { SavingsAccountKey = x.SavingsAccountKey.ToString(), SavingsAccountName = x.SavingsAccountName, Balance = x.Balance });
            var spendingCategories = tracker.SpendingCategories.Select(x => new SpendingCategory() { SpendingCategoryKey = x.SpendingCategoryKey.ToString(), SpendingCategoryName = x.SpendingCategoryName });
            var reportingPeriods = tracker.ReportingPeriods.Select(x => new ReportingPeriod() { ReportingPeriodKey = x.ReportingPeriodKey, EndDate = x.EndDate.ToString("dd/MM/yyyy"), StartDate = x.StartDate.ToString("dd/MM/yyyy") });

            var savingCountTransactions = tracker.SavingsAccounts.SelectMany(x => x.Transactions).Select(x => new SavingsAccountTransactions() { SavingsAccountKey = x.SavingsAccount.SavingsAccountKey.ToString(), ReportingPeriodKey = x.ReportingPeriod.ReportingPeriodKey.ToString(), Change = x.Change });
            var spending = tracker.SpendingCategories.SelectMany(x => x.Transactions).Select(x => new Spending() { SpendingPlaceKey = x.SpendingPlace.SpendingPlaceKey.ToString(), ReportingPeriodKey = x.ReportingPeriod.ReportingPeriodKey.ToString(), Amount = x.Amount, NumberOfTransactions = x.NumberOfTransactions });
            var spendingPlaces = tracker.SpendingCategories.SelectMany(x => x.Transactions).Select(x => new SpendingPlace() { ReportingCategoryKey = x.ReportingPeriod.ReportingPeriodKey.ToString(), SpendingPlaceKey = x.SpendingPlace.SpendingPlaceKey.ToString(), SpendingPlaceName = x.SpendingPlace.SpendingPlaceName });

            DapperPlusManager.Entity<SavingsAccount>("SavingsAccount").Table("SavingsAccount").KeepIdentity(true).InsertIfNotExists(true).Identity(x => x.SavingsAccountKey);
            DapperPlusManager.Entity<SpendingCategory>("SpendingCategory").Table("SpendingCategory").KeepIdentity(true).InsertIfNotExists(true).Identity(x => x.SpendingCategoryKey);
            DapperPlusManager.Entity<ReportingPeriod>("ReportingPeriod").Table("ReportingPeriod").KeepIdentity(true).InsertIfNotExists(true).Identity(x => x.ReportingPeriodKey);
            DapperPlusManager.Entity<SavingsAccountTransactions>("SavingsAccountTransactions").Table("SavingsAccountTransactions").KeepIdentity(true).InsertIfNotExists(true);
            DapperPlusManager.Entity<Spending>("Spending").Table("Spending").KeepIdentity(true).InsertIfNotExists(true);
            DapperPlusManager.Entity<SpendingPlace>("SpendingPlace").Table("SpendingPlace").KeepIdentity(true).InsertIfNotExists(true).Identity(x => x.SpendingPlaceKey);

            sqlConnection.BulkInsert("SavingsAccount", savingsAccounts);
            sqlConnection.BulkInsert("SpendingCategory", spendingCategories);
            sqlConnection.BulkInsert("ReportingPeriod", reportingPeriods);
            sqlConnection.BulkInsert("SavingsAccountTransactions", savingCountTransactions);
            sqlConnection.BulkInsert("Spending", spending);
            sqlConnection.BulkInsert("SpendingPlace", spendingPlaces);
        }
    }
}
