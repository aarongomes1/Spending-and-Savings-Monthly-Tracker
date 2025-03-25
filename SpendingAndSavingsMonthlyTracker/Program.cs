using CommonClasses.Structure;
using SpendingAndSavingsMonthlyTracker.Models;
using System.Globalization;

namespace SpendingAndSavingsMonthlyTracker
{
    internal class Program
    {
        private const string LICENCE_FILE = "license.txt";
        private const string REPORT_FILE = "report.xlsx";
        private const string DB_FILE = "output_db.db";
        private const string TEMPLATE_FILE = "template.xlsx";

        static void Main(string[] args)
        {
            if (args.Length != 7)
            {
                Console.WriteLine("Expected 7 parameters");
                Console.WriteLine("1) Path to the spending CSV file");
                Console.WriteLine("2) Path to the saving CSV file");
                Console.WriteLine("3) Reporting start date (DD/MM/YYYY)");
                Console.WriteLine("4) Reporting end date (DD/MM/YYYY)");
                Console.WriteLine("5) Path to the config folder");
                Console.WriteLine("6) Path to the current report folder");
                Console.WriteLine("7) Path to the history report folder");
                return;
            }

            var spendingFilePath = args[0];
            var savingsFilePath = args[1];
            var startDate = DateTime.ParseExact(args[2], "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(args[3], "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var configFolderPath = args[4];
            var currentReportFolderPath = args[5];
            var historyFolderPath = args[6];

            var syncfusionLicenseFilePath = Path.Combine(configFolderPath, LICENCE_FILE);
            var license = File.ReadAllLines(syncfusionLicenseFilePath).Single();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(license);

            var spendingRecords = IO.ReadRecords<SpendingInput>(spendingFilePath);
            var savingsRecords = IO.ReadRecords<SavingsInput>(savingsFilePath);

            var previousReportDbFilePath = Path.Combine(currentReportFolderPath, DB_FILE);

            SpendingSavingsTracker tracker;
            
            // If we have a previous month's run then load it in, if not then just create an empty models object
            if (File.Exists(previousReportDbFilePath))
            {
                tracker = SpendingSavingsTracker.Load(previousReportDbFilePath);
            }
            else
            {
                tracker = SpendingSavingsTracker.InitialiseEmpty();
            }

            var matchingReportingPeriod = tracker.ReportingPeriods.SingleOrDefault(x => x.StartDate == startDate && x.EndDate == endDate);
            if (matchingReportingPeriod is not null)
            {
                throw new ArgumentException("Reporting month already processed");
            }

            Normaliser.Normalise(tracker, savingsRecords, spendingRecords, startDate, endDate);

            var dbFilePath = Path.Combine(currentReportFolderPath, DB_FILE);
            var xlsxFilePath = Path.Combine(currentReportFolderPath, REPORT_FILE);

            // Save the completed models object to the db 
            tracker.Save(dbFilePath);

            // Calculate the various stats
            var spendingOverTime = StatsExtractor.GetSpendingOverTime(tracker);
            var spendingThisPeriod = StatsExtractor.GetSpendingThisPeriod(tracker, matchingReportingPeriod!);
            var savingsOverTime = StatsExtractor.GetSavingsOverTime(tracker);
            var isaUsage = StatsExtractor.GetISAUsageOverTime(tracker);

            var templateFilePath = Path.Combine(configFolderPath, TEMPLATE_FILE);

            XlsxDataInsertion.PopulateTemplate(templateFilePath, savingsOverTime, spendingOverTime, spendingThisPeriod, isaUsage, xlsxFilePath);

            CopyFileToHistory(spendingFilePath, historyFolderPath, endDate);
            CopyFileToHistory(savingsFilePath, historyFolderPath, endDate);
            CopyFileToHistory(dbFilePath, historyFolderPath, endDate);
            CopyFileToHistory(xlsxFilePath, historyFolderPath, endDate);
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
