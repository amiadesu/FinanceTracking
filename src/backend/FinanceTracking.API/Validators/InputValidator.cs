using System;
using System.Linq;

namespace FinanceTracking.API.Validators;

public static class InputValidator
{
    public static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        color = color.Trim();

        if (color.StartsWith("#"))
            color = color.Substring(1);

        if (color.Length != 6)
            return false;

        return color.All(c => "0123456789ABCDEFabcdef".Contains(c));
    }
}
