using System;
using System.Globalization;
using System.Windows.Data;

namespace Autolabor.PM1.TestTool.MainWindowItems.CalibrationTab {
    [ValueConversion(typeof(TabContext.StateEnum), typeof(bool))]
    internal class EnabledConverter : IValueConverter{
        public bool What { get; set; } = true;

        public TabContext.StateEnum Which { get; set; } = TabContext.StateEnum.Normal;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (TabContext.StateEnum)value == Which ? What : !What;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
