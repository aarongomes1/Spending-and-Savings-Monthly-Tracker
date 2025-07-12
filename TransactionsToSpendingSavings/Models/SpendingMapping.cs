
namespace TransactionsToSpendingSavings.Models
{
    internal class SpendingMapping
    {
        public required string TransactionName { get; set; }
        public required string SearchType { get; set; }
        public required string MappedName { get; set; }
        public required string Category { get; set; }
    }
}
