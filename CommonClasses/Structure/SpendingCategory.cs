namespace CommonClasses.Structure
{
    public class SpendingCategory
    {
        public Guid SpendingCategoryKey { get; init; } = Guid.NewGuid();

        public required string SpendingCategoryName { get; init; }

        public List<Spending> Transactions { get; } = [];

        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != typeof(SpendingCategory))
            {
                return false;
            }

            var other = (obj as SpendingCategory)!;

            return other.SpendingCategoryName.Equals(SpendingCategoryName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return SpendingCategoryKey.GetHashCode();
        }
    }
}
