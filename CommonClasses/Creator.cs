
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

        public ReportingPeriod GetOrCreateReportingPeriod(DateOnly startDate, DateOnly endDate)
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

        public static SavingsTransaction GetOrCreateSavingsTransaction(SavingsAccount savingsAccount,
            ReportingPeriod reportingPeriod,
            decimal change,
            DateOnly transactionDate,
            bool? countsToIsaLimit)
        {
            var existingTransaction = savingsAccount.Transactions.SingleOrDefault(x => x.ReportingPeriod.Equals(reportingPeriod)
            && x.Change == change
            && x.CountsToISALimit == countsToIsaLimit
            && x.TransactionDate == transactionDate);

            if (existingTransaction is not null)
            {
                return existingTransaction;
            }

            var balanceAfterTransaction = savingsAccount.Balance + change;
            savingsAccount.Balance = balanceAfterTransaction;

            var newTransaction = new SavingsTransaction()
            {
                ReportingPeriod = reportingPeriod,
                SavingsAccount = savingsAccount,
                Change = change,
                TransactionDate = transactionDate,
                CountsToISALimit = countsToIsaLimit,
                BalanceAfterTransaction = balanceAfterTransaction
            };

            savingsAccount.Transactions.Add(newTransaction);
            reportingPeriod.SavingsTransactionsThisPeriod.Add(newTransaction);

            return newTransaction;
        }

        public static Spending GetOrCreateSpendingTransaction(
            SpendingPlace spendingPlace,
            ReportingPeriod reportingPeriod,
            decimal amount,
            bool isRefund
            )
        {
            if (isRefund)
            {
                // If we have a refund we want to subtract from the total spend rather than add to it.
                // We will still count a refund as a 'visit' or a transaction because we may not know which reporting period applies to.
                // If the refund is within the last reporting period then we may try and incorrectly cancel a transaction in this one
                // which will lead to inaccurate figures.
                amount = 0 - Math.Abs(amount);
            }
            else if (amount < 0)
            {
                throw new ArgumentException("Debit amounts can't be < 0");
            }

            var existingTransaction = spendingPlace.SpendingCategory.Transactions.SingleOrDefault(x => x.SpendingPlace.Equals(spendingPlace) && x.ReportingPeriod.Equals(reportingPeriod));

            if (existingTransaction is not null)
            {
                existingTransaction.NumberOfTransactions++; 
                existingTransaction.Amount += amount;

                return existingTransaction;
            }

            var newTransaction = new Spending
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
