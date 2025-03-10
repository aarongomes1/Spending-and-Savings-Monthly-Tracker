using CommonClasses.Structure;
using SavingsInitialiser.Models;
using System.Globalization;

namespace SavingsInitialiser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Expected 3 parameters");
                Console.WriteLine("1) Path to the saving CSV file");
                Console.WriteLine("2) Financial year start date year (eg 2024)");
                Console.WriteLine("3) Path to the output db path");
                return;
            }

            var savingsFilePath = args[0];
            var year = args[1];
            var outputDbFilePath = args[2];

            var savingsRecords = IO.ReadRecords<SavingsInput>(savingsFilePath);

            var tracker = SpendingSavingsTracker.InitialiseEmpty();

            var startOfFinancialYear = DateTime.ParseExact($"06/04/{year}", "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startOfFinancialYear, startOfFinancialYear);

            foreach(var record in savingsRecords)
            {
                var isIsa = !string.IsNullOrWhiteSpace(record.IsISA) && record.IsISA.Equals("true", StringComparison.OrdinalIgnoreCase);

                var savingsAccount = tracker.Creator.GetOrCreateSavingsAccount(record.AccountName, isIsa);

                var balance = record.BalanceFromPreviousYears;

                if (record.ISAUsageUsed is not null)
                {
                    balance -= (decimal) record.ISAUsageUsed;
                }

                tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, balance, startOfFinancialYear, false);

                if (record.ISAUsageUsed is not null)
                {
                    tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, (decimal) record.ISAUsageUsed, startOfFinancialYear, true);
                }
            }

            tracker.Save(outputDbFilePath);
        }
    }
}
