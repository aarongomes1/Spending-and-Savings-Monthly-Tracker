using CommonClasses.Persistence.Models;
using Dapper;
using System.Data.SQLite;
using System.Globalization;

namespace CommonClasses.Persistence
{
    internal class Loader
    {
        public static Structure.SpendingSavingsTracker Load(string filePath)
        {
            using var sqlConnection = new SQLiteConnection($"Data Source={filePath}; Version = 3; New = False; Compress = True;");
            sqlConnection.Open();

            var reportingPeriods = sqlConnection.Query<ReportingPeriod>(FormQueryString("ReportingPeriod")).Select(x => new Structure.ReportingPeriod()
            {
                ReportingPeriodKey = Guid.Parse(x.ReportingPeriodKey),
                EndDate = DateTime.ParseExact(x.EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                StartDate = DateTime.ParseExact(x.StartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
            }).ToList();

            var savingsAccounts = sqlConnection.Query<SavingsAccount>(FormQueryString("SavingsAccount")).Select(x => new Structure.SavingsAccount()
            {
                SavingsAccountKey = Guid.Parse(x.SavingsAccountKey),
                SavingsAccountName = x.SavingsAccountName,
                Balance = x.Balance,
                IsISA = x.IsISA == 1
            }).ToList();

            var spendingCategories = sqlConnection.Query<SpendingCategory>(FormQueryString("SpendingCategory")).Select(x => new Structure.SpendingCategory()
            {
                SpendingCategoryKey = Guid.Parse(x.SpendingCategoryKey),
                SpendingCategoryName = x.SpendingCategoryName,
            }).ToList();

            var spendingPlaces = sqlConnection.Query<SpendingPlace>(FormQueryString("SpendingPlace")).ToList();
            var spending = sqlConnection.Query<Spending>(FormQueryString("Spending")).ToList();
            var savingAccountTransactions = sqlConnection.Query<SavingsAccountTransactions>(FormQueryString("SavingsAccountTransactions")).ToList();

            var spendPlaces = CreateSpendingPlacesTransactionsRelationship(spendingCategories, spendingPlaces);
            CreateSpendingRelationship(spendingCategories, spendPlaces, reportingPeriods, spending);
            CreateSavingsAccountTransactionsRelationship(savingsAccounts, reportingPeriods, savingAccountTransactions);

            var spendingSavingsTracker = new Structure.SpendingSavingsTracker(savingsAccounts, reportingPeriods, spendingCategories);

            return spendingSavingsTracker;
        }

        private static void CreateSavingsAccountTransactionsRelationship(List<Structure.SavingsAccount> savingsAccounts, List<Structure.ReportingPeriod> reportingPeriods, List<SavingsAccountTransactions> savingsAccountTransactions)
        {
            foreach (var transactionToLoad in savingsAccountTransactions)
            {
                var reportingPeriodKey = Guid.Parse(transactionToLoad.ReportingPeriodKey);
                var savingsAccountKey = Guid.Parse(transactionToLoad.SavingsAccountKey);

                var reportingPeriod = reportingPeriods.Single(x => x.ReportingPeriodKey == reportingPeriodKey);
                var savingsAccount = savingsAccounts.Single(x => x.SavingsAccountKey == savingsAccountKey);

                var transaction = new Structure.SavingsTransaction()
                {
                    ReportingPeriod = reportingPeriod,
                    SavingsAccount = savingsAccount,
                    Change = transactionToLoad.Change,
                    TransactionDate = DateTime.ParseExact(transactionToLoad.TransactionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    CountsToISALimit = transactionToLoad.CountsToISALimit == 1,
                    BalanceAfterTransaction = transactionToLoad.BalanceAfterTransaction,
                };

                savingsAccount.Transactions.Add(transaction);
                reportingPeriod.SavingsTransactionsThisPeriod.Add(transaction);
            }
        }

        private static void CreateSpendingRelationship(List<Structure.SpendingCategory> spendingCategories, List<Structure.SpendingPlace> spendingPlaces, List<Structure.ReportingPeriod> reportingPeriods, List<Spending> spendingsToSave)
        {
            foreach (var spendingToSave in spendingsToSave)
            {
                var reportingPeriodKey = Guid.Parse(spendingToSave.ReportingPeriodKey);
                var spendingPlaceKey = Guid.Parse(spendingToSave.SpendingPlaceKey);

                var reportingPeriod = reportingPeriods.Single(x => x.ReportingPeriodKey == reportingPeriodKey);
                var spendingPlace = spendingPlaces.Single(x => x.SpendingPlaceKey == spendingPlaceKey);
                var spendingCategory = spendingCategories.Single(x => x.SpendingCategoryKey == spendingPlace.SpendingCategory.SpendingCategoryKey);

                var spending = new Structure.Spending()
                {
                    ReportingPeriod = reportingPeriod,
                    SpendingPlace = spendingPlace,
                    Amount = spendingToSave.Amount,
                    NumberOfTransactions = spendingToSave.NumberOfTransactions,
                };

                spendingCategory.Transactions.Add(spending);
                reportingPeriod.SpendingTransactionsThisPeriod.Add(spending);
            }
        }

        private static List<Structure.SpendingPlace> CreateSpendingPlacesTransactionsRelationship(List<Structure.SpendingCategory> spendingCategories, List<SpendingPlace> spendingPlacesToSave)
        {
            var spendingPlaces = new List<Structure.SpendingPlace>();

            foreach (var spendingPlaceToSave in spendingPlacesToSave)
            {
                var spendingCategoryKey = Guid.Parse(spendingPlaceToSave.SpendingCategoryKey);

                var spendingCategory = spendingCategories.Single(x => x.SpendingCategoryKey == spendingCategoryKey);

                var spendingPlace = new Structure.SpendingPlace()
                {
                    SpendingPlaceKey = Guid.Parse(spendingPlaceToSave.SpendingPlaceKey),
                    SpendingCategory = spendingCategory,
                    SpendingPlaceName = spendingPlaceToSave.SpendingPlaceName
                };

                spendingPlaces.Add(spendingPlace);
            }

            return spendingPlaces;
        }

        private static string FormQueryString(string tableName)
        {
            var query = $"SELECT * FROM {tableName}";
            return query;
        }
    }
}
