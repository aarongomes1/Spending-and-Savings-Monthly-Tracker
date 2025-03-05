
using CommonClasses.Structure;

namespace CommonClasses
{
    public class Creator
    {
        private readonly SpendingSavingsTracker _tracker;

        public Creator(SpendingSavingsTracker tracker)
        {
            _tracker = tracker;
        }

        public ReportingPeriod GetOrCreateReportingPeriod(DateTime startDate, DateTime endDate)
        {
            var matchingReportingPeriod = _tracker.ReportingPeriods.SingleOrDefault(x => x.StartDate.Equals(startDate) && x.EndDate.Equals(endDate));

            if (matchingReportingPeriod is not null)
            {
                return matchingReportingPeriod;
            }

            var newReportingPeriod = new ReportingPeriod()
            {
                StartDate = startDate,
                EndDate = endDate,
            };

            _tracker.ReportingPeriods.Add(newReportingPeriod);

            return newReportingPeriod;
        }

        public SpendingCategory GetOrCreateSpendingCategory(string categoryName)
        {
            var matchingSpendingCategory = _tracker.SpendingCategories.SingleOrDefault(x => x.SpendingCategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (matchingSpendingCategory is not null)
            {
                return matchingSpendingCategory;
            }

            var newSpendingCategory = new SpendingCategory()
            {
                SpendingCategoryName = categoryName,
            };

            _tracker.SpendingCategories.Add(newSpendingCategory);

            return newSpendingCategory;
        }

        public SpendingPlace GetOrCreateSpendingPlace(string spendingPlace, SpendingCategory spendingCategory)
        {
            var matchingSpendingPlace = _tracker.SpendingCategories.SelectMany(x => x.Transactions)
                .Select(x => x.SpendingPlace)
                .Where(x => x.SpendingPlaceName.Equals(spendingPlace, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (matchingSpendingPlace is not null)
            {
                return matchingSpendingPlace;
            }

            var newSpendingPlace = new SpendingPlace()
            {
                SpendingPlaceName = spendingPlace,
                SpendingCategory = spendingCategory
            };

            return newSpendingPlace;
        }

        public SavingsAccount GetOrCreateSavingsAccount(string savingsAccountName, bool isIsa)
        {
            var matchingSavingsAccount = _tracker.SavingsAccounts.SingleOrDefault(x => x.SavingsAccountName.Equals(savingsAccountName, StringComparison.OrdinalIgnoreCase));

            if (matchingSavingsAccount is not null)
            {
                return matchingSavingsAccount;
            }

            var newSavingsAccount = new SavingsAccount()
            {
                SavingsAccountName = savingsAccountName,
                Balance = 0,
                IsISA = isIsa,
            };

            _tracker.SavingsAccounts.Add(newSavingsAccount);

            return newSavingsAccount;
        }

        public SavingsTransaction GetOrCreateSavingsTransaction(SavingsAccount savingsAccount,
            ReportingPeriod reportingPeriod,
            decimal change,
            DateTime transactionDate,
            bool? countsToIsaLimit)
        {
            var existingTransaction = savingsAccount.Transactions.SingleOrDefault(x => x.ReportingPeriod.Equals(reportingPeriod));

            if (existingTransaction is not null)
            {
                return existingTransaction;
            }

            var newTransaction = new SavingsTransaction()
            {
                ReportingPeriod = reportingPeriod,
                SavingsAccount = savingsAccount,
                Change = change,
                TransactionDate = transactionDate,
                CountsToISALimit = countsToIsaLimit,
            };

            savingsAccount.Balance += change;

            savingsAccount.Transactions.Add(newTransaction);
            reportingPeriod.SavingsTransactionsThisPeriod.Add(newTransaction);

            return newTransaction;
        }

        public Spending GetOrCreateSpendingTransaction(SpendingPlace spendingPlace, ReportingPeriod reportingPeriod, decimal amount)
        {
            var existingTransaction = spendingPlace.SpendingCategory.Transactions.SingleOrDefault(x => x.SpendingPlace.Equals(spendingPlace) && x.ReportingPeriod.Equals(reportingPeriod));

            if (existingTransaction is not null)
            {
                existingTransaction.NumberOfTransactions++;
                existingTransaction.Amount += amount;

                return existingTransaction;
            }

            var newTransaction = new Spending()
            {
                ReportingPeriod = reportingPeriod,
                SpendingPlace = spendingPlace,
                Amount = amount,
                NumberOfTransactions = 1,
            };

            spendingPlace.SpendingCategory.Transactions.Add(newTransaction);
            reportingPeriod.SpendingTransactionsThisPeriod.Add(newTransaction);

            return newTransaction;
        }
    }
}
