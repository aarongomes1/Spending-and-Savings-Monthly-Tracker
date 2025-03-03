using CommonClasses.Structure;
using SpendingAndSavingsMonthlyTracker.Models;
using System.Globalization;

namespace SpendingAndSavingsMonthlyTracker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 6)
            {
                Console.WriteLine("Expected 6 parameters");
                Console.WriteLine("1) Path to the spending CSV file");
                Console.WriteLine("2) Path to the saving CSV file");
                Console.WriteLine("3) Reporting start date (DD/MM/YYYY)");
                Console.WriteLine("4) Reporting end date (DD/MM/YYYY)");
                Console.WriteLine("5) Previous report month file path or blank if none");
                Console.WriteLine("6) Path to the output folder path");
                return;
            }

            var spendingFilePath = args[0];
            var savingsFilePath = args[1];
            var startDate = DateTime.ParseExact(args[2], "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(args[3], "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var previousReportMonthFilePath = args[4];
            var outputFolderPath = args[5];

            var spendingRecords = IO.ReadRecords<SpendingInput>(spendingFilePath);
            var savingsRecords = IO.ReadRecords<SavingsInput>(savingsFilePath);

            SpendingSavingsTracker tracker;

            if (!string.IsNullOrWhiteSpace(previousReportMonthFilePath))
            {
                tracker = SpendingSavingsTracker.Load(previousReportMonthFilePath);
            }
            else
            {
                tracker = new SpendingSavingsTracker([], [], []);
            }


            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startDate, endDate);

            foreach(var savingsRecord in savingsRecords)
            {
                var savingsAccount = tracker.Creator.GetOrCreateSavingsAccount(savingsRecord.AccountName);
                tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, savingsRecord.Deposit);
            }

            foreach(var spendingRecord in spendingRecords)
            {
                var amountChanged = spendingRecord.Debit ?? spendingRecord.Refund ?? throw new Exception("Spending record has blank amount");

                var spendingCategory = tracker.Creator.GetOrCreateSpendingCategory(spendingRecord.Category);
                var spendingPlace = tracker.Creator.GetOrCreateSpendingPlace(spendingRecord.Name, spendingCategory);

                tracker.Creator.GetOrCreateSpendingTransaction(spendingPlace, reportingPeriod, amountChanged);
            }

            var dbFilePath = Path.Combine(outputFolderPath, "output.db");

            tracker.Save(dbFilePath);
        }
    }
}
