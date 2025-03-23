using CommonClasses.Structure;
using SavingsInitialiser.Models;
using System.Globalization;

namespace SavingsInitialiser
{
    internal class Program
    {
        private const string DB_FILE = "output_db.db";

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Expected 4 parameters");
                Console.WriteLine("1) Path to the saving CSV file");
                Console.WriteLine("2) Financial year start date year (eg 2024)");
                Console.WriteLine("3) Path to the current month folder");
                Console.WriteLine("3) Path to the history folder");
                return;
            }

            var savingsFilePath = args[0];
            var year = args[1];
            var currentMonthFolderPath = args[2];
            var historyFolderPath = args[3];

            var savingsRecords = IO.ReadRecords<SavingsInput>(savingsFilePath);

            // Create an empty models class
            var tracker = SpendingSavingsTracker.InitialiseEmpty();

            // We'll assign any previous years contributions and ISA usage to the start of the financial year
            var startOfFinancialYear = DateTime.ParseExact($"06/04/{year}", "dd/MM/yyyy", CultureInfo.InvariantCulture);

            Normaliser.NormaliseSavings(tracker, savingsRecords, startOfFinancialYear);

            var outputDbFilePath = Path.Combine(currentMonthFolderPath, DB_FILE);
            tracker.Save(outputDbFilePath);

            CopyFileToHistory(outputDbFilePath, historyFolderPath, startOfFinancialYear);
        }

        private static void CopyFileToHistory(string filePath, string historyFolderPath, DateTime date)
        {
            var fileToCopy = Path.GetFileName(filePath);

            var year = date.ToString("yyyy");
            var yearMonth = date.ToString("yyyyMM");
            var formattedDate = date.ToString("yyyyMMdd");

            var outputFilePath = Path.Combine(historyFolderPath, year, yearMonth, formattedDate, fileToCopy);

            var directory = Path.GetDirectoryName(outputFilePath)!;
            if (!Path.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(filePath, outputFilePath, true);
        }
    }
}
