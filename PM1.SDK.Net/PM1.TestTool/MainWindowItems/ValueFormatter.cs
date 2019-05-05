using System;
using System.Globalization;
using System.Windows.Data;

namespace Autolabor.PM1.TestTool.MainWindowItems {
    [ValueConversion(typeof(double), typeof(string))]
    internal class ValueFormatter : IValueConverter {
        public string Format { get; set; } = "0.###";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ((double)value).ToString(Format, culture);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var text = ((string)value).ToLower(culture);
            return double.TryParse(text, out var result)
                   ? result
                   : text == "inf"
                  || text == "+inf"
                  || text == "∞"
                  || text == "+∞"
                     ? double.PositiveInfinity
                     : (object)double.NaN;
        }
    }
}
