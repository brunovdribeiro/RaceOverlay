using System.Globalization;
using System.Windows.Controls;

namespace RaceOverlay.App.Validation;

public class DoubleRangeValidationRule : ValidationRule
{
    public double Min { get; set; } = double.MinValue;
    public double Max { get; set; } = double.MaxValue;

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var input = value as string;
        if (string.IsNullOrWhiteSpace(input))
            return new ValidationResult(false, "Value is required.");

        if (!double.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, cultureInfo, out double result))
            return new ValidationResult(false, "Must be a number.");

        if (result < Min || result > Max)
            return new ValidationResult(false, $"Must be between {Min} and {Max}.");

        return ValidationResult.ValidResult;
    }
}
