using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace RaceOverlay.App.Validation;

public partial class HexColorValidationRule : ValidationRule
{
    [GeneratedRegex(@"^#?[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColorRegex();

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = value as string;
        if (string.IsNullOrWhiteSpace(input))
            return new ValidationResult(false, "Value is required.");

        if (!HexColorRegex().IsMatch(input))
            return new ValidationResult(false, "Must be a hex color (e.g. #FF0000).");

        return ValidationResult.ValidResult;
    }
}
