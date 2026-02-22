using System;
using System.Collections.Generic;

namespace FinanceTracking.API.Models;

public class Group
{
    public int Id { get; set; }
    public Guid? OwnerId { get; set; }
    public string Name { get; set; }
    public bool IsPersonal { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public AppUser Owner { get; set; }
    public ICollection<GroupMember> Members { get; set; }
    public ICollection<Category> Categories { get; set; }
    public ICollection<Seller> Sellers { get; set; }
    public ICollection<ProductData> Products { get; set; }
    public ICollection<Receipt> Receipts { get; set; }
}