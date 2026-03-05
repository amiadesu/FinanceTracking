namespace FinanceTracking.API.Constants;

public static class ServiceConstants
{
    public const int InfiniteLimit = int.MaxValue;

    // Groups
    public const int MaxGroupsPerUser = 10;
    public const int MaxMembersPerGroup = 100;
    public const int MaxCategoriesPerGroup = 50;
    public const int MaxReceiptsPerGroup = InfiniteLimit;
    public const int MaxSellersPerGroup = InfiniteLimit;
    public const int MaxBudgetGoalsPerGroup = InfiniteLimit;

    // Group histories
    public const int MaxGroupHistoryEntriesPerPage = 50;
    public const int DefaultGroupHistoryEntriesPerPage = 20;

    // Receipts
    public const int MaxProductsPerReceipt = 50;
    public const int MaxCategoriesPerProduct = 5;

    // Categories
    public const string DefaultCategoryColor = "#000000";
}