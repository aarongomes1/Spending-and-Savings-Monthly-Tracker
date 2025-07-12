
namespace TransactionsToSpendingSavings.Models
{
    internal class SavingsMapping
    {
        public required string TransactionName { get; set; }
        public required string SearchType { get; set; }
        public required string MappedName { get; set; }
        public bool? BalanceCountsToISA { get; set; }
        public bool? IsISA { get; set; }
    }
}
