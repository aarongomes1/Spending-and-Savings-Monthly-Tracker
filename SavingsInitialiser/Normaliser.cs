using CommonClasses.Structure;
using SavingsInitialiser.Models;

namespace SavingsInitialiser
{
    internal class Normaliser
    {
        public static void NormaliseSavings(SpendingSavingsTracker tracker, List<SavingsInput> savingsRecords, DateOnly startOfFinancialYear)
        {
            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startOfFinancialYear, startOfFinancialYear);

            foreach (var record in savingsRecords)
            {
                var isIsa = record.IsISA is not null && (bool)record.IsISA;

                var savingsAccount = tracker.Creator.GetOrCreateSavingsAccount(record.AccountName, isIsa);

                var balance = record.BalanceFromPreviousYears;

                // If we have some ISA usage for the year, we need subtract this off the previous year balance
                if (record.ISAUsageUsed is not null)
                {
                    balance -= (decimal)record.ISAUsageUsed;
                }

                // Create a transaction at the start of the financial year with the amount from previous years
                tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, balance, startOfFinancialYear, false);

                // Create an extra transaction with the ISA usage for the year
                if (record.ISAUsageUsed is not null)
                {
                    tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, (decimal)record.ISAUsageUsed, startOfFinancialYear, true);
                }
            }
        }
    }
}
