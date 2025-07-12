using TransactionsToSpendingSavings.Mappers;
using TransactionsToSpendingSavings.Models;

namespace TransactionsToSpendingSavings
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 6)
            {
                Console.WriteLine("Expected 6 parameters:");
                Console.WriteLine("1) Path to the barclays csv");
                Console.WriteLine("2) Path to the barclaycard csv");
                Console.WriteLine("3) Path to the spending mapping file");
                Console.WriteLine("4) Path to the savings mapping file");
                Console.WriteLine("5) Path to the exclusion file");
                Console.WriteLine("6) Path to the output folder");
                return;
            }    

            var barlaysInputFilePath = args[0];
            var barlaycardInputFilePath = args[1];
            var spendingMappingFilePath = args[2];
            var savingsMappingFilePath = args[3];
            var exclusionFilePath = args[4];
            var outputFolder = args[5];

            AddHeadersToBarclyacardFileIfNotExists(barlaycardInputFilePath);

            var barclaysRecords = IO.ReadRecords<Barclays>(barlaysInputFilePath);
            var barclaycardRecords = IO.ReadRecords<Barclaycard>(barlaycardInputFilePath);
            var spendingMappingRecords = IO.ReadRecords<SpendingMapping>(spendingMappingFilePath);
            var savingsMappingRecords = IO.ReadRecords<SavingsMapping>(savingsMappingFilePath);
            var exclusionRecords = IO.ReadRecords<Exclusion>(exclusionFilePath);

            var barclaycardRecordsWithoutExcluded = barclaycardRecords.Where(x => !Excluder.IsExcluded(x.Name, exclusionRecords)).ToList();
            var barclaysRecordsWithoutExcluded = barclaysRecords.Where(x => !Excluder.IsExcluded(x.Memo, exclusionRecords)).ToList();

            (var mappedSpending, var unmappedSpending) = SpendingMapper.TranslateSpending(barclaycardRecordsWithoutExcluded, 
                    barclaysRecordsWithoutExcluded, spendingMappingRecords);
            (var mappedSavings, var unmappedSavings) = SavingsMapper.TranslateSavings(barclaysRecordsWithoutExcluded, savingsMappingRecords);

            // Only the records that weren't in both savings and spending need to be output as unmapped
            var unmappedRecords = unmappedSpending.Intersect(unmappedSavings).ToList();

            var mappedSavingsFilePath = Path.Combine(outputFolder, "savings.csv");
            var mappedSpendingFilePath = Path.Combine(outputFolder, "spending.csv");
            var unmappedTransactionsFilePath = Path.Combine(outputFolder, "unmappedTransactions.csv");

            IO.WriteRecords(mappedSavingsFilePath, mappedSavings);
            IO.WriteRecords(mappedSpendingFilePath, mappedSpending);
            IO.WriteRecords(unmappedTransactionsFilePath, unmappedRecords);
        }

        private static void AddHeadersToBarclyacardFileIfNotExists(string barclaycardFilePath)
        {
            var barclaycardRecords = File.ReadAllLines(barclaycardFilePath).ToList();
            var firstRecordOrHeader = barclaycardRecords.First();

            var headerRow = "Date,Name,Type,Account,Category,Debit,Credit";
            if (!firstRecordOrHeader.Equals(headerRow, StringComparison.OrdinalIgnoreCase))
            {
                var recordsIncludingHeader = new List<string> { headerRow };
                recordsIncludingHeader.AddRange(barclaycardRecords);

                File.WriteAllLines(barclaycardFilePath, recordsIncludingHeader);
            }
        }
    }
}
