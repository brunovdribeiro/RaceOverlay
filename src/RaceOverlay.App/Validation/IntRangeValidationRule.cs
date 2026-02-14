using System.Globalization;
using System.Windows.Controls;

namespace RaceOverlay.App.Validation;

public class IntRangeValidationRule : ValidationRule
{
    public int Min { get; set; } = int.MinValue;
    public int Max { get; set; } = int.MaxValue;

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = value as string;
        if (string.IsNullOrWhiteSpace(input))
            return new ValidationResult(false, "Value is required.");

        if (!int.TryParse(input, NumberStyles.Integer, cultureInfo, out int result))
            return new ValidationResult(false, "Must be a whole number.");

        if (result < Min || result > Max)
            return new ValidationResult(false, $"Must be between {Min} and {Max}.");

        return ValidationResult.ValidResult;
    }
}
