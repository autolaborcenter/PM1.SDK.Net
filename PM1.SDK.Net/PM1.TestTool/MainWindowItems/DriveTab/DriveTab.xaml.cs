using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Autolabor.PM1.TestTool.MainWindowItems.DriveTab {
    /// <summary>
    /// DriveTab.xaml 的交互逻辑
    /// </summary>
    public partial class DriveTab : UserControl, ITabControl {
        private MainWindowContext _windowContext;
        private TabContext _tabContext;

        public DriveTab() => InitializeComponent();

        public void OnEnter() { }

        public void OnLeave() { }

        private void Tab_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => _windowContext = e.NewValue as MainWindowContext;

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => _tabContext = e.NewValue as TabContext;

        private void Drag_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (e.MouseDevice.Captured == null)
                    e.MouseDevice.Capture(Origin);
                var position = e.GetPosition(Origin);
                _tabContext.X = position.X;
                _tabContext.Y = position.Y;
            } else {
                e.MouseDevice.Capture(null);
                _tabContext.X =
                _tabContext.Y = TabContext.Size / 2;
            }
        }
    }
}
