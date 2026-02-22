using System;

namespace FinanceTracking.API.Models;

public class Category
{
    public int Id { get; set; }
    public int? GroupId { get; set; }
    public string Name { get; set; }
    public string ColorHex { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public Group Group { get; set; }
    public ICollection<ProductDataCategory> ProductDataCategories { get; set; }
}