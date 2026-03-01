using System.Collections.Generic;

namespace FinanceTracking.API.DTOs;

public class SystemConfigDto
{
    public GroupConfigDto GroupLimits { get; set; } = new();
    public ReceiptConfigDto ReceiptLimits { get; set; } = new();
    public CategoryConfigDto CategoryRules { get; set; } = new();
    
    public Dictionary<string, int> GroupRoles { get; set; } = new(); 
}

public class GroupConfigDto
{
    public int MaxGroupsPerUser { get; set; }
    public int MaxMembersPerGroup { get; set; }
    public int MaxCategoriesPerGroup { get; set; }
    public int MaxReceiptsPerGroup { get; set; }
    public int MaxSellersPerGroup { get; set; }
    public int MaxBudgetGoalsPerGroup { get; set; }
}

public class ReceiptConfigDto
{
    public int MaxProductsPerReceipt { get; set; }
    public int MaxCategoriesPerProduct { get; set; }
}

public class CategoryConfigDto
{
    public string DefaultCategoryColor { get; set; } = string.Empty;
}