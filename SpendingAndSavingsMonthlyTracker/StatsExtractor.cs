using CommonClasses.Structure;
using SpendingAndSavingsMonthlyTracker.Models.ProcessedModels;

namespace SpendingAndSavingsMonthlyTracker
{
    public class StatsExtractor
    {
        private static readonly int NUMBER_OF_REPORTING_PERIODS = 13;

        public static List<SpendingThisPeriod> GetSpendingThisPeriod(SpendingSavingsTracker tracker)
        {
            var currentReportingPeriod = tracker.ReportingPeriods.OrderByDescending(x => x.EndDate).First();

            var spendingThisPeriod = currentReportingPeriod.SpendingTransactionsThisPeriod.Select(x =>
                new SpendingThisPeriod()
                {
                    TotalAmountSpent = x.Amount,
                    SpendingPlace = x.SpendingPlace.SpendingPlaceName,
                    SpendingCategory = x.SpendingPlace.SpendingCategory.SpendingCategoryName,
                    NumberOfVisits = x.NumberOfTransactions,
                }).ToList();

            return spendingThisPeriod;
        }

        public static List<SpendingOverTime> GetSpendingOverTime(SpendingSavingsTracker tracker)
        {
            var reportingPeriods = tracker.ReportingPeriods.OrderByDescending(x => x.EndDate).Take(NUMBER_OF_REPORTING_PERIODS);
            
            var spendingOverTime = new List<SpendingOverTime>();

            foreach(var reportingPeriod in reportingPeriods)
            {
                var transactionsThisPeriod = reportingPeriod.SpendingTransactionsThisPeriod
                    .GroupBy(x => x.SpendingPlace.SpendingCategory.SpendingCategoryName, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new SpendingOverTime() {
                        ReportingPeriod = reportingPeriod.ToString(),
                        SpendingCategory = x.Key,
                        Amount = x.Select(x => x.Amount).Sum(),
                    });

                spendingOverTime.AddRange(transactionsThisPeriod);
            }


            return spendingOverTime;
        }

        public static List<SavingsOverTime> GetSavingsOverTime(SpendingSavingsTracker tracker)
        {
            var reportingPeriods = tracker.ReportingPeriods.OrderByDescending(x => x.EndDate).Take(NUMBER_OF_REPORTING_PERIODS);

            var spendingOverTime = new List<SavingsOverTime>();

            foreach (var reportingPeriod in reportingPeriods)
            {
                var transactionsThisPeriod = reportingPeriod.SavingsTransactionsThisPeriod
                    .GroupBy(x => x.SavingsAccount.SavingsAccountName, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new SavingsOverTime()
                    {
                        ReportingPeriod = reportingPeriod.ToString(),
                        Account = x.Key,
                        AmountAdded = x.Select(x => x.Change).Sum(),
                    });

                spendingOverTime.AddRange(transactionsThisPeriod);
            }


            return spendingOverTime;
        }
    }
}
