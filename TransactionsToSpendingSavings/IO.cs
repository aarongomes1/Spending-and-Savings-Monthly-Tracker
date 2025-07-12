using CsvHelper;
using System.Globalization;
using System.Text;

namespace TransactionsToSpendingSavings
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
            using var streamWriter = new StreamWriter(filePath, append: false);
            using var csvReader = new CsvWriter(streamWriter, culture: CultureInfo.InvariantCulture);

            csvReader.WriteRecords(records);
        }
    }
}