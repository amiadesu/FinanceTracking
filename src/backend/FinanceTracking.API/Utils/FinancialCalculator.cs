using FinanceTracking.API.Models;

namespace FinanceTracking.API.Utils;

public static class FinancialCalculator
{
    public static decimal CalculateEntryTotal(decimal price, decimal quantity)
    {
        return RoundUpToTwoDecimalPlaces(price * quantity);
    }

    public static decimal CalculateReceiptTotal(IEnumerable<ProductEntry> entries)
    {
        return RoundUpToTwoDecimalPlaces(
            entries.Sum(pe => CalculateEntryTotal(pe.Price, pe.Quantity))
        );
    }

    public static decimal RoundUpToTwoDecimalPlaces(decimal value)
    {
        return Math.Ceiling(value * 100) / 100;
    }
}