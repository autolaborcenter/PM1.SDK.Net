using System.Windows;
using System.Windows.Controls;

namespace Autolabor.PM1.TestTool.MainWindowItems.ActionTab {
    /// <summary>
    /// RudderTab.xaml 的交互逻辑
    /// </summary>
    public partial class RudderControlView : UserControl {
        public delegate void OnCompletedHandler(object sender, double value);
        public event OnCompletedHandler OnCompleted;

        public RudderControlView() => InitializeComponent();

        public void Reset() => Box.Dispatch(it => it.Text = "");

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            var box = (TextBox)sender;
            box.Foreground = double.TryParse(box.Text, out _) 
                ? ToolFunctions.NormalBrush
                : ToolFunctions.ErrorBrush;
        }

        private void Left_Click(object sender, RoutedEventArgs e) {
            if (!double.TryParse(Box.Text, out var value) || value == 0) return;
            OnCompleted?.Invoke(this, +value.ToRad());
        }

        private void Right_Click(object sender, RoutedEventArgs e) {
            if (!double.TryParse(Box.Text, out var value) || value == 0) return;
            OnCompleted?.Invoke(this, -value.ToRad());
        }
    }
}
