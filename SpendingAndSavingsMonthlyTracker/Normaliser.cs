
using CommonClasses.Structure;
using SpendingAndSavingsMonthlyTracker.Models;
using System.Globalization;

namespace SpendingAndSavingsMonthlyTracker
{
    internal class Normaliser
    {
        public static void Normalise(
            SpendingSavingsTracker tracker,
            List<SavingsInput> savingsRecords,
            List<SpendingInput> spendingRecords,
            DateOnly startDate,
            DateOnly endDate)
        {
            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startDate, endDate);

            NormaliseSavings(tracker, savingsRecords, reportingPeriod);
            NormaliseSpending(tracker, spendingRecords, reportingPeriod);
        }

        private static void NormaliseSavings(SpendingSavingsTracker tracker,
            List<SavingsInput> savingsRecords,
            ReportingPeriod reportingPeriod)
        {
            var savingsAccountsUsed = new List<SavingsAccount>();

            // Ordering by date allows us to calculate the balance of the savings account for each transaction
            savingsRecords = savingsRecords.OrderBy(x => DateOnly.ParseExact(x.TransactionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToList();

            foreach (var savingsRecord in savingsRecords)
            {
                var isIsa = savingsRecord.IsISA is not null && (bool)savingsRecord.IsISA;

                var savingsAccount = tracker.Creator.GetOrCreateSavingsAccount(savingsRecord.AccountName, isIsa);
                savingsAccountsUsed.Add(savingsAccount);

                // If the account we put money into is an ISA then we need to count this transaction towards the ISA limit
                var countsToIsaLimit = savingsRecord.BalanceCountsToISALimit is not null && (bool)savingsRecord.BalanceCountsToISALimit;

                var transactionDate = DateOnly.ParseExact(savingsRecord.TransactionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, savingsRecord.Deposit, transactionDate, countsToIsaLimit);
            }

            // For the savings accounts that weren't added to set the transaction to 0
            foreach (var savingsAccount in tracker.SavingsAccounts)
            {
                if (!savingsAccountsUsed.Contains(savingsAccount))
                {
                    tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, 0, reportingPeriod.StartDate, savingsAccount.IsISA);
                }
            }
        }

        private static void NormaliseSpending(SpendingSavingsTracker tracker,
            List<SpendingInput> spendingRecords,
            ReportingPeriod reportingPeriod)
        {
            // Zip through all the spending records and translate into the model
            foreach (var spendingRecord in spendingRecords)
            {
                var amountChanged = spendingRecord.Debit ?? spendingRecord.Refund ?? throw new Exception("Spending record has blank amount");

                var spendingCategory = tracker.Creator.GetOrCreateSpendingCategory(spendingRecord.Category);
                var spendingPlace = tracker.Creator.GetOrCreateSpendingPlace(spendingRecord.Name, spendingCategory);

                tracker.Creator.GetOrCreateSpendingTransaction(spendingPlace, reportingPeriod, amountChanged);
            }
        }
    }
}
