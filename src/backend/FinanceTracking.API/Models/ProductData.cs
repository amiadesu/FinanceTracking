using System;

namespace FinanceTracking.API.Models;

public class ProductData
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public Group Group { get; set; }
    public ICollection<ProductDataCategory> ProductDataCategories { get; set; }
    public ICollection<ProductEntry> ProductEntries { get; set; }
}