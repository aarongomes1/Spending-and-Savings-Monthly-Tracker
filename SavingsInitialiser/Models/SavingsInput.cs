
namespace SavingsInitialiser.Models
{
    internal class SavingsInput
    {
        public required string AccountName { get; init; }
        public required decimal BalanceFromPreviousYears { get; init; }
        public string? IsISA { get; init; }
        public decimal? ISAUsageUsed { get; init; }
    }
}
