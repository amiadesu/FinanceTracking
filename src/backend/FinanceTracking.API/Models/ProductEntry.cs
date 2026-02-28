using System;

namespace FinanceTracking.API.Models;

public class ProductEntry
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int ReceiptId { get; set; }
    public int ProductDataId { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public Group Group { get; set; }
    public Receipt Receipt { get; set; }
    public ProductData ProductData { get; set; }
}