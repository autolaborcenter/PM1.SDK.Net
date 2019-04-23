using System.Globalization;
using System.Windows.Controls;

namespace Autolabor.PM1.TestTool.MainWindowItems.CalibrationTab {
    /// <summary>
    ///     范围检查
    /// </summary>
    internal class ParseValidationRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            => double.TryParse(value as string, out var _value)
               ? ValidationResult.ValidResult
               : new ValidationResult(false, "无法解析");
    }
}
