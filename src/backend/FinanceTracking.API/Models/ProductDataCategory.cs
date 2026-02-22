using System;

namespace FinanceTracking.API.Models;

public class ProductDataCategory
{
    public int ProductDataId { get; set; }
    public int CategoryId { get; set; }
    public int GroupId { get; set; }

    public ProductData ProductData { get; set; }
    public Category Category { get; set; }
    public Group Group { get; set; }
}