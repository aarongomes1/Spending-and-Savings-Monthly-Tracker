using CommonClasses.Structure;
using SpendingAndSavingsMonthlyTracker.Models.ProcessedModels;

namespace SpendingAndSavingsMonthlyTracker
{
    public class StatsExtractor
    {
        private static readonly int NUMBER_OF_REPORTING_PERIODS = 13;
        private static readonly string TOTAL_NAME = "Total";

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
            var reportingPeriods = tracker.ReportingPeriods.OrderByDescending(x => x.EndDate).Take(NUMBER_OF_REPORTING_PERIODS).OrderBy(x => x.EndDate);
            
            var spendingOverTime = new List<SpendingOverTime>();

            foreach(var reportingPeriod in reportingPeriods)
            {
                // Remove the initial reporting period from spending over time
                if (reportingPeriod.StartDate.Day == 6 && reportingPeriod.StartDate.Month == 4
                    && reportingPeriod.EndDate.Day == 6 && reportingPeriod.EndDate.Month == 4)
                {
                    continue;
                }

                var transactionsThisPeriod = reportingPeriod.SpendingTransactionsThisPeriod
                    .GroupBy(x => x.SpendingPlace.SpendingCategory.SpendingCategoryName, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new SpendingOverTime() {
                        ReportingPeriod = reportingPeriod.ToString(),
                        SpendingCategory = x.Key,
                        Amount = x.Select(x => x.Amount).Sum(),
                    });

                spendingOverTime.AddRange(transactionsThisPeriod);

                // Add the total of all spendind for the reporting period
                var totalAmount = reportingPeriod.SpendingTransactionsThisPeriod.Select(x => x.Amount).Sum();
                var totalSpendingPerReportingPeriod = new SpendingOverTime()
                {
                    SpendingCategory = TOTAL_NAME,
                    Amount = totalAmount,
                    ReportingPeriod = reportingPeriod.ToString(),
                };
                spendingOverTime.Add(totalSpendingPerReportingPeriod);
            }


            return spendingOverTime;
        }

        public static List<SavingsOverTime> GetSavingsOverTime(SpendingSavingsTracker tracker)
        {
            var reportingPeriods = tracker.ReportingPeriods.OrderByDescending(x => x.EndDate).Take(NUMBER_OF_REPORTING_PERIODS).OrderBy(x => x.EndDate);

            var spendingOverTime = new List<SavingsOverTime>();

            foreach (var reportingPeriod in reportingPeriods)
            {
                var transactionsThisPeriod = reportingPeriod.SavingsTransactionsThisPeriod
                    .GroupBy(x => x.SavingsAccount.SavingsAccountName, StringComparer.OrdinalIgnoreCase)
                    .Select(x => {
                        var newestTransaction = x.OrderByDescending(x => x.TransactionDate).First();

                        return new SavingsOverTime()
                        {
                            ReportingPeriod = reportingPeriod.ToString(),
                            Account = x.Key,
                            AmountAdded = newestTransaction.BalanceAfterTransaction,
                        };
                    });

                spendingOverTime.AddRange(transactionsThisPeriod);

                var total = transactionsThisPeriod.Select(x => x.AmountAdded).Sum();

                var totalSavingsPerReportingPeriod = new SavingsOverTime()
                {
                    Account = TOTAL_NAME,
                    AmountAdded = total,
                    ReportingPeriod = reportingPeriod.ToString(),
                };

                spendingOverTime.Add(totalSavingsPerReportingPeriod);
            }


            return spendingOverTime;
        }

        public static List<ISAUsage> GetISAUsageOverTime(SpendingSavingsTracker tracker)
        {
            var mostRecentPeriod = tracker.ReportingPeriods.OrderByDescending(x => x.StartDate).First();

            var startOfFinancialYear = DateTime.Parse($"06/04/{mostRecentPeriod.StartDate.Year}");
            var endOfFinancialYear = DateTime.Parse($"05/04/{mostRecentPeriod.StartDate.Year + 1}");

            if (startOfFinancialYear >= mostRecentPeriod.StartDate)
            {
                startOfFinancialYear = DateTime.Parse($"06/04/{mostRecentPeriod.StartDate.Year - 1}");
                endOfFinancialYear = DateTime.Parse($"05/04/{mostRecentPeriod.StartDate.Year}");
            }

            var reportingPeriodsWithinFinancialYear = tracker.ReportingPeriods.Where(x => x.EndDate >= startOfFinancialYear && x.StartDate <= endOfFinancialYear)
                .OrderBy(x => x.StartDate).ToList();

            var isaRecords = new List<ISAUsage>();

            decimal totalIsaUsage = 0m;

            foreach(var reportingPeriod in reportingPeriodsWithinFinancialYear)
            {
                var transactionsForReportingPeriod = reportingPeriod.SavingsTransactionsThisPeriod.Where(x => x.CountsToISALimit is not null && (bool) x.CountsToISALimit).ToList();
                var totalForTheMonth = transactionsForReportingPeriod.Select(x => Math.Abs(x.Change)).Sum();

                totalIsaUsage += totalForTheMonth;

                var isaUsage = new ISAUsage()
                {
                    Month = reportingPeriod.ToString(),
                    TotalLimitUsed = totalIsaUsage
                };

                isaRecords.Add(isaUsage);
            }

            return isaRecords;
        }
    }
}
