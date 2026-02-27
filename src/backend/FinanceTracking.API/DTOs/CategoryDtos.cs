using System;

namespace FinanceTracking.API.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public int? GroupId { get; set; }
    public string Name { get; set; }
    public string ColorHex { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; }
    public string ColorHex { get; set; }
}

public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? ColorHex { get; set; }
}
