namespace CommonClasses.Persistence.Models
{
    internal class SpendingPlace
    {
        public required string SpendingPlaceKey { get; init; }
        public required string SpendingPlaceName { get; init; }
        public required string SpendingCategoryKey { get; init; }
    }
}
