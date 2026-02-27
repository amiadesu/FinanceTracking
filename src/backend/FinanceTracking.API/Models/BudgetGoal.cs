using System;

namespace FinanceTracking.API.Models;

public class BudgetGoal
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public decimal TargetAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public Group Group { get; set; } = null!;
}