using System;

namespace FinanceTracking.API.DTOs;

public class SellerDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

public class CreateSellerDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class UpdateSellerDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
