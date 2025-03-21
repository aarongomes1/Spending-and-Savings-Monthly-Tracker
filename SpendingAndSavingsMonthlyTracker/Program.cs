using CommonClasses.Structure;
using SpendingAndSavingsMonthlyTracker.Models;
using System.Globalization;

namespace SpendingAndSavingsMonthlyTracker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 8)
            {
                Console.WriteLine("Expected 8 parameters");
                Console.WriteLine("1) Path to the spending CSV file");
                Console.WriteLine("2) Path to the saving CSV file");
                Console.WriteLine("3) Reporting start date (DD/MM/YYYY)");
                Console.WriteLine("4) Reporting end date (DD/MM/YYYY)");
                Console.WriteLine("5) Path to the template file");
                Console.WriteLine("6) Previous report month file path or blank if none");
                Console.WriteLine("7) Path to the Syncfusion license key file");
                Console.WriteLine("8) Path to the output folder path");
                return;
            }

            var spendingFilePath = args[0];
            var savingsFilePath = args[1];
            var startDate = DateTime.ParseExact(args[2], "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(args[3], "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var templateFilePath = args[4];
            var previousReportMonthFilePath = args[5];
            var syncfusionLicenseFilePath = args[6];
            var outputFolderPath = args[7];

            var license = File.ReadAllLines(syncfusionLicenseFilePath).Single();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(license);

            var spendingRecords = IO.ReadRecords<SpendingInput>(spendingFilePath);
            var savingsRecords = IO.ReadRecords<SavingsInput>(savingsFilePath);

            SpendingSavingsTracker tracker;

            if (!string.IsNullOrWhiteSpace(previousReportMonthFilePath))
            {
                tracker = SpendingSavingsTracker.Load(previousReportMonthFilePath);
            }
            else
            {
                tracker = SpendingSavingsTracker.InitialiseEmpty();
            }


            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startDate, endDate);

            var savingsAccountsUsed = new List<SavingsAccount>();

            savingsRecords = savingsRecords.OrderBy(x => DateTime.ParseExact(x.TransactionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToList();

            foreach (var savingsRecord in savingsRecords)
            {
                var isIsa = !string.IsNullOrWhiteSpace(savingsRecord.IsISA) && savingsRecord.IsISA.Equals("true", StringComparison.OrdinalIgnoreCase);

                var savingsAccount = tracker.Creator.GetOrCreateSavingsAccount(savingsRecord.AccountName, isIsa);
                savingsAccountsUsed.Add(savingsAccount);

                var countsToIsaLimit = !string.IsNullOrWhiteSpace(savingsRecord.BalanceCountsToISALimit) && savingsRecord.BalanceCountsToISALimit.Equals("true", StringComparison.OrdinalIgnoreCase);

                var transactionDate = DateTime.ParseExact(savingsRecord.TransactionDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, savingsRecord.Deposit, transactionDate, countsToIsaLimit);
            }

            // For the savings accounts that weren't added to set the transaction to 0
            foreach(var savingsAccount in tracker.SavingsAccounts)
            {
                if (!savingsAccountsUsed.Contains(savingsAccount))
                {
                    tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, 0, reportingPeriod.StartDate, savingsAccount.IsISA);
                }
            }

            foreach(var spendingRecord in spendingRecords)
            {
                var amountChanged = spendingRecord.Debit ?? spendingRecord.Refund ?? throw new Exception("Spending record has blank amount");

                var spendingCategory = tracker.Creator.GetOrCreateSpendingCategory(spendingRecord.Category);
                var spendingPlace = tracker.Creator.GetOrCreateSpendingPlace(spendingRecord.Name, spendingCategory);

                tracker.Creator.GetOrCreateSpendingTransaction(spendingPlace, reportingPeriod, amountChanged);
            }

            var dbFilePath = Path.Combine(outputFolderPath, "output.db");
            var xlsxFilePath = Path.Combine(outputFolderPath, "report.xlsx");

            tracker.Save(dbFilePath);

            var spendingOverTime = StatsExtractor.GetSpendingOverTime(tracker);
            var spendingThisPeriod = StatsExtractor.GetSpendingThisPeriod(tracker);
            var savingsOverTime = StatsExtractor.GetSavingsOverTime(tracker);
            var isaUsage = StatsExtractor.GetISAUsageOverTime(tracker);

            XlsxDataInsertion.PopulateTemplate(templateFilePath, savingsOverTime, spendingOverTime, spendingThisPeriod, isaUsage, xlsxFilePath);
        }
    }
}
