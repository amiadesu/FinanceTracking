namespace FinanceTracking.API.Constants;

public static class ServiceConstants
{
    // Groups
    public const int MaxGroupsPerUser = 10;
    public const int MaxMembersPerGroup = 1;
    public const int MaxCategoriesPerGroup = 50;
    public const int MaxReceiptsPerGroup = int.MaxValue; // No limit
    public const int MaxSellersPerGroup = int.MaxValue; // No limit
    public const int MaxBudgetGoalsPerGroup = int.MaxValue; // No limit

    // Receipts
    public const int MaxProductsPerReceipt = 50;
    public const int MaxCategoriesPerProduct = 5;

    // Categories
    public const string DefaultCategoryColor = "#000000";
}