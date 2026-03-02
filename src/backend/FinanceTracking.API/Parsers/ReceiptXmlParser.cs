using System.Xml.Linq;
using System.Text.RegularExpressions;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Extensions;

namespace FinanceTracking.API.Parsers;

public class ReceiptXmlParser
{
    public async Task<CreateReceiptDto?> ParseAsync(Stream xmlStream)
    {
        using var reader = new StreamReader(xmlStream, detectEncodingFromByteOrderMarks: true);
        var xDoc = XDocument.Load(reader);

        var datNode = xDoc.Descendants("DAT").FirstOrDefault();
        if (datNode == null) 
            throw new FormatException("Invalid XML format: Missing DAT node.");

        var tnValue = datNode.Attribute("TN")?.Value ?? "";
        var numberMatch = Regex.Match(tnValue, @"\d+");
        if (!numberMatch.Success)
            throw new FormatException("Invalid TN format: No seller ID found.");

        var sellerId = numberMatch.Value.ToString();

        // Ensure this is a sales receipt (T="0")
        var cNode = datNode.Element("C");
        if (cNode == null || cNode.Attribute("T")?.Value != "0")
            throw new FormatException("Unsupported receipt type. Only sales receipts (T=0) are supported.");

        var products = new List<CreateReceiptProductDto>();

        foreach (var pNode in cNode.Elements("P"))
        {
            var name = pNode.Attribute("NM")?.Value ?? "Unknown Product";

            // Quantity is multiplied by 1000. If missing, it implies 1.0 (Q="1000").
            var qStr = pNode.Attribute("Q")?.Value;
            var quantity = string.IsNullOrEmpty(qStr) ? 1.0m : decimal.Parse(qStr) / 1000m;

            // Price and Sum are multiplied by 100.
            var prcStr = pNode.Attribute("PRC")?.Value;
            var smStr = pNode.Attribute("SM")?.Value;
            
            decimal price = 0;
            if (!string.IsNullOrEmpty(prcStr))
            {
                price = decimal.Parse(prcStr) / 100m;
            }
            else if (!string.IsNullOrEmpty(smStr))
            {
                // Fallback: Calculate price from sum and quantity
                price = (decimal.Parse(smStr) / 100m) / quantity;
            }

            products.Add(new CreateReceiptProductDto
            {
                Name = name,
                Price = price,
                Quantity = quantity,
                Categories = new List<string>()
            });
        }

        // Parse Payment Date (format: YYYYMMDDhhmmss)
        var eNode = cNode.Element("E");
        var tsStr = eNode?.Attribute("TS")?.Value ?? datNode.Element("TS")?.Value;
        
        DateTime paymentDate = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(tsStr) && tsStr.Length >= 14)
        {
            if (DateTime.TryParseExact(tsStr.Substring(0, 14), "yyyyMMddHHmmss", 
                null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                paymentDate = parsedDate;
            }
        }

        return new CreateReceiptDto
        {
            SellerId = sellerId,
            PaymentDate = paymentDate,
            Products = products
        };
    }
}