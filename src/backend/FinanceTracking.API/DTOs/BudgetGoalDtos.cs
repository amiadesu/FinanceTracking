using System;

namespace FinanceTracking.API.DTOs;

public class BudgetGoalDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public decimal TargetAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class BudgetGoalProgressDto
{
    public int GoalId { get; set; }
    public int GroupId { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CreateBudgetGoalDto
{
    public decimal TargetAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class UpdateBudgetGoalDto
{
    public decimal? TargetAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}