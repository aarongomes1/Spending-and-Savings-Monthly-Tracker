namespace CommonClasses.Structure
{
    public class SpendingCategory
    {
        public required Guid SpendingCategoryKey { get; init; } = Guid.NewGuid();
        public required string SpendingCategoryName { get; init; }

        public List<Spending> Transactions { get; } = [];
    }
}
