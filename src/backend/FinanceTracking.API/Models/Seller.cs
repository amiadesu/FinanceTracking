using System;

namespace FinanceTracking.API.Models;

public class Seller
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public Group Group { get; set; }
    public ICollection<Receipt> Receipts { get; set; }
}