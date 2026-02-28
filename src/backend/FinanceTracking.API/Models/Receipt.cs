using System;

namespace FinanceTracking.API.Models;

public class Receipt
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public int? SellerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public Group Group { get; set; }
    public AppUser CreatedByUser { get; set; }
    public Seller Seller { get; set; }
    public ICollection<ProductEntry> ProductEntries { get; set; }
}