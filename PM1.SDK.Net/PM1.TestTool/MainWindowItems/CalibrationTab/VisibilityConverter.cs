using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Autolabor.PM1.TestTool.MainWindowItems.CalibrationTab {
    [ValueConversion(typeof(TabContext.StateEnum), typeof(Visibility))]
    internal class VisibilityConverter : IValueConverter {
        public bool Visible { get; set; } = true;

        public TabContext.StateEnum Which { get; set; } = TabContext.StateEnum.Normal;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (TabContext.StateEnum)value == Which
               ? Visible ? Visibility.Visible
                         : Visibility.Collapsed
               : Visible ? Visibility.Collapsed
                         : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
            => throw new NotImplementedException();
    }
}
