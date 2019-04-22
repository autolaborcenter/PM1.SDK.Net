using System.Globalization;
using System.Windows.Controls;

namespace Autolabor.PM1.TestTool.MainWindowItems.SettingsWindow {
    /// <summary>
    ///     范围检查
    /// </summary>
    internal class RangeValidationRule : ValidationRule {
        public double Min { get; set; } = double.NegativeInfinity;

        public double Max { get; set; } = double.PositiveInfinity;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            => double.TryParse(value as string, out var _value)
               ? _value < Min || _value > Max
                   ? new ValidationResult(false, string.Format(cultureInfo, "超出范围：({0}, {1})", Min, Max))
                   : ValidationResult.ValidResult
               : new ValidationResult(false, "无法解析");
    }
}
