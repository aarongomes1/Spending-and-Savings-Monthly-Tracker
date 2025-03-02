using CsvHelper;
using System.Globalization;
using System.Text;

namespace SpendingAndSavingsMonthlyTracker
{
    public class IO
    {
        public static List<T> ReadRecords<T>(string filePath)
        {
            using var streamReader = new StreamReader(filePath, encoding: Encoding.UTF8);
            using var csvReader = new CsvReader(streamReader, culture: CultureInfo.InvariantCulture);

            return csvReader.GetRecords<T>().ToList();
        }

        public static void WriteRecords<T>(string filePath, List<T> records)
        {
            var directory = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            using var streamWriter = new StreamWriter(filePath);
            using var csvWriter = new CsvWriter(streamWriter, culture: CultureInfo.InvariantCulture);

            csvWriter.WriteRecords(records);
        }
    }
}