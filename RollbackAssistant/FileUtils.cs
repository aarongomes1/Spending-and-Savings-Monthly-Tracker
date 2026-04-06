using System.IO.Compression;

namespace RollbackAssistant
{
    internal class FileUtils
    {
        public static void DeleteIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static string BuildDatePath(string baseFolderPath, DateOnly date)
        {
            var day = $"{date.Year}{date.Month:D2}{date.Day:D2}";
            var month = $"{date.Year:D2}{date.Month:D2}";
            var year = date.Year.ToString();

            var dateFolderPath = Path.Combine(baseFolderPath, year, month, day);

            return dateFolderPath;
        }

        public static List<DateOnly> ConstructDatesFromHistory(string historyFolderPath, DateOnly date)
        {
            var dates = new List<DateOnly>();

            var yearDirectories = Directory.GetDirectories(historyFolderPath).Where(x => int.Parse(Path.GetFileName(x)) >= date.Year).ToList();

            foreach (var yearDirectory in yearDirectories)
            {
                var monthDirectorys = Directory.GetDirectories(yearDirectory);

                foreach (var monthDirectory in monthDirectorys)
                {
                    var dateFolderPaths = Directory.GetDirectories(monthDirectory);
                    var currentDates = dateFolderPaths.Select(x => DateOnly.ParseExact(Path.GetFileName(x), "yyyyMMdd")).ToList();

                    var currentDatesWithinRange = currentDates.Where(x => x > date).Order().ToList();
                    dates.AddRange(currentDatesWithinRange);
                }
            }

            return dates;
        }

        public static string ZipHistory(string folderPath, string backupFolderPath)
        {
            var dateTimeNow = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");

            var zipFilePath = Path.Combine(backupFolderPath, $"HistoryBackUp_{dateTimeNow}.zip");

            ZipFile.CreateFromDirectory(folderPath, zipFilePath);

            return zipFilePath;
        }
    }
}
