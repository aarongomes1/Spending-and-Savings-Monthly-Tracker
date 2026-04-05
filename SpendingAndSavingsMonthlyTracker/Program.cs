using CommonClasses.Structure;
using SpendingAndSavingsMonthlyTracker.Models;
using System.Globalization;

namespace SpendingAndSavingsMonthlyTracker
{
    internal class Program
    {
        private const string LICENCE_FILE_NAME = "license.txt";
        private const string REPORT_FILE_NAME = "report.xlsx";
        private const string DB_FILE_NAME = "output_db.db";
        private const string TEMPLATE_FILE_NAME = "template.xlsx";
        private const string SPENDING_FILE_NAME = "spending.csv";
        private const string SAVINGS_FILE_NAME = "savings.csv";

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Expected 4 parameters");
                Console.WriteLine("1) Reporting end date (DD/MM/YYYY)");
                Console.WriteLine("2) Path to the config folder");
                Console.WriteLine("3) Path to the current report folder");
                Console.WriteLine("4) Path to the history report folder");
                return;
            }

            var endDate = DateOnly.ParseExact(args[0], "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var configFolderPath = args[1];
            var currentReportFolderPath = args[2];
            var historyFolderPath = args[3];

            var syncfusionLicenseFilePath = Path.Combine(configFolderPath, LICENCE_FILE_NAME);
            var license = File.ReadAllLines(syncfusionLicenseFilePath).Single();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(license);

            var spendingFilePath = Path.Combine(currentReportFolderPath, SPENDING_FILE_NAME);
            var savingsFilePath = Path.Combine(currentReportFolderPath, SAVINGS_FILE_NAME);

            var spendingRecords = IO.ReadRecords<SpendingInput>(spendingFilePath);
            var savingsRecords = IO.ReadRecords<SavingsInput>(savingsFilePath);

            var previousReportDbFilePath = Path.Combine(currentReportFolderPath, DB_FILE_NAME);

            // If we have a previous month's run then load it in, if not then just create an empty models object
            var tracker = File.Exists(previousReportDbFilePath) ?
                SpendingSavingsTracker.Load(previousReportDbFilePath) : 
                SpendingSavingsTracker.InitialiseEmpty();

            var previousReportingPeriod = tracker.ReportingPeriods.OrderBy(x => x.EndDate).Last();

            if (previousReportingPeriod.EndDate >= endDate)
            {
                throw new ArgumentException("Reporting month already processed");
            }

            var startDate = previousReportingPeriod.EndDate.AddDays(1);

            // To prevent accidently using old db file, we'll restrict the reporting period length to 40 days or less
            if (previousReportingPeriod.EndDate.AddDays(40) < startDate)
            {
                throw new ArgumentException("End date is more than 40 day from the previous end date");
            }

            var newReportingPeriod = Normaliser.Normalise(tracker, savingsRecords, spendingRecords, startDate, endDate);

            var dbFilePath = Path.Combine(currentReportFolderPath, DB_FILE_NAME);
            var xlsxFilePath = Path.Combine(currentReportFolderPath, REPORT_FILE_NAME);

            // Save the completed models object to the db 
            tracker.Save(dbFilePath);

            // Calculate the various stats
            var spendingOverTime = StatsExtractor.GetSpendingOverTime(tracker);
            var spendingThisPeriod = StatsExtractor.GetSpendingThisPeriod(tracker, newReportingPeriod);
            var savingsOverTime = StatsExtractor.GetSavingsOverTime(tracker);
            var isaUsage = StatsExtractor.GetISAUsageOverTime(tracker);

            var templateFilePath = Path.Combine(configFolderPath, TEMPLATE_FILE_NAME);

            XlsxDataInsertion.PopulateTemplate(templateFilePath, savingsOverTime, spendingOverTime, spendingThisPeriod, isaUsage, xlsxFilePath);

            CopyFileToHistory(spendingFilePath, historyFolderPath, endDate);
            CopyFileToHistory(savingsFilePath, historyFolderPath, endDate);
            CopyFileToHistory(dbFilePath, historyFolderPath, endDate);
            CopyFileToHistory(xlsxFilePath, historyFolderPath, endDate);
        }

        private static void CopyFileToHistory(string filePath, string historyFolderPath, DateOnly date)
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
