using System;
using System.Collections.Generic;

namespace FinanceTracking.API.DTOs;

public class ProductDataDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Categories { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class UpdateProductDataDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<int>? CategoryIds { get; set; }
}