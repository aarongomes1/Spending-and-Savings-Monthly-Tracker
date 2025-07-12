
namespace TransactionsToSpendingSavings.Models
{
    internal class Barclaycard
    {
        public required string Name { get; set; }
        public decimal? Credit { get; set; }
        public decimal? Debit { get; set; }
    }
}
