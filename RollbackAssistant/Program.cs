using System.Diagnostics;

namespace RollbackAssistant
{
    internal class Program
    {
        private const string DB_FILE_NAME = "output_db.db";
        private const string SPENDING_FILE_NAME = "spending.csv";
        private const string SAVINGS_FILE_NAME = "savings.csv";

        static void Main(string[] args)
        {
            if (args.Length != 6)
            {
                Console.WriteLine("Expects 6 parameters:");
                Console.WriteLine("1) Enter the rollback end date (DD/MM/YYYY)");     
                Console.WriteLine("2) Path to the current report folder");
                Console.WriteLine("3) Path to the history report folder");
                Console.WriteLine("4) Path to the config folder");
                Console.WriteLine("5) Path to the backup folder");
                Console.WriteLine("6) Path to the tracker exe");
                return;
            }

            var rollbackDate = DateOnly.Parse(args[0]);
            var currentMonthFolderPath = args[1];
            var historyFolderPath = args[2];
            var configFolderPath = args[3];
            var backupFolderPath = args[4];
            var trackerExeFilePath = args[5];

            Console.WriteLine("Zipping old history");

            var zipFilePath = FileUtils.ZipHistory(historyFolderPath, backupFolderPath);

            Console.WriteLine($"Saved previous history as {zipFilePath}");

            var dates = FileUtils.ConstructDatesFromHistory(historyFolderPath, rollbackDate);

            Console.WriteLine("Resetting db to restore point");

            var dbFilePath = Path.Combine(currentMonthFolderPath, DB_FILE_NAME);

            FileUtils.DeleteIfExists(dbFilePath);

            var dateHistoryFolderPath = FileUtils.BuildDatePath(historyFolderPath, rollbackDate);
            var oldDbFilePath = Path.Combine(dateHistoryFolderPath, DB_FILE_NAME);

            File.Copy(oldDbFilePath, dbFilePath);

            Console.WriteLine($"Identified {dates.Count} reporting dates to reapply");

            foreach (var dateToApply in dates)
            {
                Console.WriteLine($"Reapplying reporting date: {dateToApply:dd/MM/yyyy}");

                ExecuteReportingPeriod(trackerExeFilePath, currentMonthFolderPath, historyFolderPath, configFolderPath, dateToApply);
            }
        }

        private static void ExecuteReportingPeriod(
            string trackerExePath,
            string currentMonthFolderPath,
            string historyFolderPath,
            string configFolderPath,
            DateOnly dateToExecute)
        {
            var dateHistoryFolderPath = FileUtils.BuildDatePath(historyFolderPath, dateToExecute);

            var spendingHistoryFilePath = Path.Combine(dateHistoryFolderPath, SPENDING_FILE_NAME);
            var savingsHistoryFilePath = Path.Combine(dateHistoryFolderPath, SAVINGS_FILE_NAME);

            var newSpendingFilePath = Path.Combine(currentMonthFolderPath, SPENDING_FILE_NAME);
            var newSavingsFilePath = Path.Combine(currentMonthFolderPath, SAVINGS_FILE_NAME);

            FileUtils.DeleteIfExists(newSpendingFilePath);
            FileUtils.DeleteIfExists(newSavingsFilePath);

            File.Copy(spendingHistoryFilePath, newSpendingFilePath);
            File.Copy(savingsHistoryFilePath, newSavingsFilePath);

            var startInfo = new ProcessStartInfo
            {
                FileName = trackerExePath
            };

            startInfo.ArgumentList.Add(dateToExecute.ToString("dd/MM/yyyy"));
            startInfo.ArgumentList.Add(configFolderPath);
            startInfo.ArgumentList.Add(currentMonthFolderPath);
            startInfo.ArgumentList.Add(historyFolderPath);

            using var exeProcess = Process.Start(startInfo)!;
            exeProcess.WaitForExit();
        } 
    }
}
