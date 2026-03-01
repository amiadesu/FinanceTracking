using System;
using FinanceTracking.API.Models;

namespace FinanceTracking.API.DTOs;

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsFull { get; set; }
    public bool IsPersonal { get; set; }
    public Guid? OwnerId { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateGroupDto
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateGroupDto
{
    public string Name { get; set; } = string.Empty;
}

public class ResetGroupDto
{
    public bool ResetMembers { get; set; }
    public bool ResetBudgetGoals { get; set; }
    public bool ResetCategories { get; set; }
    public bool ResetReceiptsProductsAndSellers { get; set; }
}