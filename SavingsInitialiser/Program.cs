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

            // Create an empty models class
            var tracker = SpendingSavingsTracker.InitialiseEmpty();

            // We'll assign any previous years contributions and ISA usage to the start of the financial year
            var startOfFinancialYear = DateTime.ParseExact($"06/04/{year}", "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var reportingPeriod = tracker.Creator.GetOrCreateReportingPeriod(startOfFinancialYear, startOfFinancialYear);

            foreach(var record in savingsRecords)
            {
                var isIsa = record.IsISA is not null && (bool) record.IsISA;

                var savingsAccount = tracker.Creator.GetOrCreateSavingsAccount(record.AccountName, isIsa);

                var balance = record.BalanceFromPreviousYears;

                // If we have some ISA usage for the year, we need subtract this off the previous year balance
                if (record.ISAUsageUsed is not null)
                {
                    balance -= (decimal) record.ISAUsageUsed;
                }

                // Create a transaction at the start of the financial year with the amount from previous years
                tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, balance, startOfFinancialYear, false);

                // Create an extra transaction with the ISA usage for the year
                if (record.ISAUsageUsed is not null)
                {
                    tracker.Creator.GetOrCreateSavingsTransaction(savingsAccount, reportingPeriod, (decimal) record.ISAUsageUsed, startOfFinancialYear, true);
                }
            }

            tracker.Save(outputDbFilePath);
        }
    }
}
