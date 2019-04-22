using System;
using System.Globalization;
using System.Windows.Data;

namespace Autolabor.PM1.TestTool.MainWindowItems.SettingsWindow {
    [ValueConversion(typeof(double), typeof(string))]
    internal class AngleFormatter : IValueConverter {
        public string Format { get; set; } = "0.#";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ((double)value).ToDegree().ToString(Format, culture);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => double.TryParse((string)value, out var result) ? result.ToRad() : double.NaN;
    }
}
