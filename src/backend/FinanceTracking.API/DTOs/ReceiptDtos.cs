using System;
using System.Collections.Generic;

namespace FinanceTracking.API.DTOs;

public class ReceiptDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public int? SellerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public List<ReceiptProductDto> Products { get; set; }
}

public class ReceiptProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<string> Categories { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
}

public class CreateReceiptDto
{
    public int? SellerId { get; set; }
    public DateTime PaymentDate { get; set; }
    public List<CreateReceiptProductDto> Products { get; set; }
}

public class CreateReceiptProductDto
{
    public string Name { get; set; }
    public List<string> Categories { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
}

public class UpdateReceiptDto
{
    public int? SellerId { get; set; }
    public DateTime? PaymentDate { get; set; }
    public List<UpdateReceiptProductDto>? Products { get; set; }
}

public class UpdateReceiptProductDto
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public List<string> Categories { get; set; }
    public decimal? Price { get; set; }
    public decimal? Quantity { get; set; }
}