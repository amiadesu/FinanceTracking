using System.Xml.Linq;
using System.Text.RegularExpressions;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.Utils;
using System.Xml;
using System.Linq;

namespace FinanceTracking.API.Parsers;

public class ReceiptXmlParser
{
    private class ReceiptItem
    {
        public int OperationNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal OriginalSum { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal DiscountSum { get; set; }
        public decimal MarkupSum { get; set; }
    }

    public async Task<CreateReceiptDto?> ParseAsync(Stream xmlStream)
    {
        var settings = new XmlReaderSettings 
        { 
            Async = true,
            IgnoreWhitespace = true 
        };

        using var xmlReader = XmlReader.Create(xmlStream, settings);
        var xDoc = XDocument.Load(xmlReader);

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

        var productsDict = new Dictionary<int, ReceiptItem>();

        // Parse all purchased products (<P>)
        foreach (var pNode in cNode.Elements("P"))
        {
            var nAttr = pNode.Attribute("N")?.Value;
            if (!int.TryParse(nAttr, out int n)) continue;

            var name = pNode.Attribute("NM")?.Value ?? "Unknown Product";

            // Quantity is multiplied by 1000. If missing, it implies 1.0 (Q="1000").
            var qStr = pNode.Attribute("Q")?.Value;
            var quantity = string.IsNullOrEmpty(qStr) ? 1.0m : decimal.Parse(qStr) / 1000m;

            // Price and Sum are multiplied by 100.
            var prcStr = pNode.Attribute("PRC")?.Value;
            var smStr = pNode.Attribute("SM")?.Value;

            decimal sum = 0;
            decimal? originalPrice = null;

            // Capture exact price if PRC is present
            if (!string.IsNullOrEmpty(prcStr) && decimal.TryParse(prcStr, out decimal parsedPrc))
            {
                originalPrice = parsedPrc / 100m;
            }

            if (!string.IsNullOrEmpty(smStr) && decimal.TryParse(smStr, out decimal parsedSm))
            {
                sum = parsedSm / 100m;
            }
            else if (originalPrice.HasValue)
            {
                sum = originalPrice.Value * quantity;
            }

            if (sum < 0) sum = 0;

            productsDict[n] = new ReceiptItem
            {
                OperationNumber = n,
                Name = name,
                Quantity = quantity,
                OriginalSum = sum,
                OriginalPrice = originalPrice,
                DiscountSum = 0,
                MarkupSum = 0
            };
        }

        // Parse all Discounts (<D>) and Markups (<S>)
        var modifiers = cNode.Elements().Where(e => e.Name == "D" || e.Name == "S");
        foreach (var modNode in modifiers)
        {
            var isDiscount = modNode.Name == "D";
            
            var nAttr = modNode.Attribute("N")?.Value;
            int.TryParse(nAttr, out int currentN);

            var trAttr = modNode.Attribute("TR")?.Value ?? "0";
            var tyAttr = modNode.Attribute("TY")?.Value ?? "0";

            var targetNIs = new List<int>();
            
            // Determine explicit targets
            var niAttr = modNode.Attribute("NI")?.Value;
            if (!string.IsNullOrEmpty(niAttr) && int.TryParse(niAttr, out int explicitNi))
            {
                targetNIs.Add(explicitNi);
            }
            else
            {
                var niElements = modNode.Elements("NI");
                foreach (var niElement in niElements)
                {
                    var subNiAttr = niElement.Attribute("NI")?.Value;
                    if (int.TryParse(subNiAttr, out int subNi)) 
                    {
                        targetNIs.Add(subNi);
                    }
                }
            }

            // Determine implicit targets
            if (!targetNIs.Any())
            {
                if (trAttr == "0") 
                {
                    var previousProductN = productsDict.Keys.Where(k => k < currentN).DefaultIfEmpty(-1).Max();
                    if (previousProductN != -1) targetNIs.Add(previousProductN);
                }
                else if (trAttr == "1")
                {
                    targetNIs.AddRange(productsDict.Keys.Where(k => k < currentN));
                }
            }

            var validTargets = targetNIs.Where(productsDict.ContainsKey).ToList();
            if (!validTargets.Any()) continue;

            // Apply the modifier based on its Type (TY)
            if (tyAttr == "1")
            {
                // Percentage-based modifier (TY="1")
                var prStr = modNode.Attribute("PR")?.Value;
                if (!string.IsNullOrEmpty(prStr) && decimal.TryParse(prStr, out decimal percentage))
                {
                    decimal multiplier = percentage / 100m;
                    foreach (var ni in validTargets)
                    {
                        var item = productsDict[ni];
                        decimal amount = item.OriginalSum * multiplier;
                        
                        if (isDiscount) item.DiscountSum += amount;
                        else item.MarkupSum += amount;
                    }
                }
            }
            else
            {
                // Flat sum modifier (TY="0")
                var smStr = modNode.Attribute("SM")?.Value;
                if (!string.IsNullOrEmpty(smStr) && decimal.TryParse(smStr, out decimal totalModSum))
                {
                    totalModSum /= 100m;
                    decimal totalTargetSum = validTargets.Sum(ni => productsDict[ni].OriginalSum);

                    if (totalTargetSum > 0)
                    {
                        foreach (var ni in validTargets)
                        {
                            var item = productsDict[ni];
                            decimal proportionalModSum = totalModSum * (item.OriginalSum / totalTargetSum);
                            
                            if (isDiscount) item.DiscountSum += proportionalModSum;
                            else item.MarkupSum += proportionalModSum;
                        }
                    }
                }
            }
        }

        // Build the final DTO mapping
        var products = new List<CreateReceiptProductDto>();
        foreach (var item in productsDict.Values.OrderBy(p => p.OperationNumber))
        {
            decimal effectivePrice;

            // If there are no modifiers (discounts/markups) and we have an exact original price, use it.
            if (item.DiscountSum == 0 && item.MarkupSum == 0 && item.OriginalPrice.HasValue)
            {
                effectivePrice = item.OriginalPrice.Value;
            }
            else
            {
                // Fallback: Calculate effective price if modifiers were applied or PRC was missing
                decimal finalSum = Math.Max(0, item.OriginalSum - item.DiscountSum + item.MarkupSum);
                effectivePrice = item.Quantity > 0 ? finalSum / item.Quantity : 0;
            }

            products.Add(new CreateReceiptProductDto
            {
                Name = item.Name,
                Price = FinancialCalculator.RoundUpToTwoDecimalPlaces(effectivePrice),
                Quantity = FinancialCalculator.RoundUpToThreeDecimalPlaces(item.Quantity),
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