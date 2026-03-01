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

public class CategoryListResponseDto
{
    public int CurrentCount { get; set; }
    public int MaxAllowed { get; set; }
    
    public List<CategoryDto> Categories { get; set; } = new();
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
