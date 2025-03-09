using System.Globalization;
using System.Text;
using CsvHelper;


namespace SavingsInitialiser
{
    public class IO
    {
        public static List<T> ReadRecords<T>(string filePath)
        {
            using var streamReader = new StreamReader(filePath, encoding: Encoding.UTF8);
            using var csvReader = new CsvReader(streamReader, culture: CultureInfo.InvariantCulture);

            return csvReader.GetRecords<T>().ToList();
        }
    }
}
