using System;
using System.Globalization;
using System.Windows.Data;

namespace Autolabor.PM1.TestTool.MainWindowItems.CalibrationTab {
    [ValueConversion(typeof(double), typeof(string))]
    internal class ValueFormatter : IValueConverter {
        public string Format { get; set; } = "0.###";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ((double)value).ToString(Format, culture);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
