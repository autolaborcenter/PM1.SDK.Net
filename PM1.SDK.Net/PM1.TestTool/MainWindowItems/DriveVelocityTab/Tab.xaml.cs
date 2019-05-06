using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Autolabor.PM1.TestTool.MainWindowItems.DriveVelocityTab {
    /// <summary>
    /// Tab.xaml 的交互逻辑
    /// </summary>
    public partial class Tab : UserControl, ITabControl {
        private MainWindowContext _windowContext;
        private TabContext _tabContext;

        public Tab() => InitializeComponent();

        private volatile bool flag;
        private Task _task;

        private MouseDevice mouseDevice;

        private void Tab_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => _windowContext = e.NewValue as MainWindowContext;

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => _tabContext = e.NewValue as TabContext;

        public void OnEnter() {
            _task = Task.Run(async () => {
                flag = true;
                while (flag) {
                    await Task.Delay(50).ConfigureAwait(false);
                    try {
                        if (_windowContext?.State == MainWindowContext.ConnectionState.Connected)
                            Methods.VelocityTarget = (_tabContext.V, _tabContext.W);
                    } catch (Exception exception) {
                        _windowContext.ErrorInfo = exception.Message;
                    }
                }
                _task = null;
            });
        }

        public void OnLeave() {
            flag = false;
            _task?.Wait();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e) {
            var origin = (IInputElement)sender;

            if (e.LeftButton == MouseButtonState.Pressed) {
                if (e.MouseDevice.Captured == null) {
                    mouseDevice = e.MouseDevice;
                    e.MouseDevice.Capture(origin);
                }
                var position = e.GetPosition(origin);
                _tabContext.Left = position.X - TabContext.TouchSize / 2;
                _tabContext.Top = position.Y - TabContext.TouchSize / 2;
            } else ReleaseMouse();
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            => ReleaseMouse();

        private void Canvas_TouchUp(object sender, TouchEventArgs e)
            => ReleaseMouse();

        void ReleaseMouse() {
            mouseDevice?.Capture(null);
            _tabContext.Left = _tabContext.OX;
            _tabContext.Top = _tabContext.OY;
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e) {
            _tabContext.Height = e.NewSize.Height;
            _tabContext.Width = e.NewSize.Width;
        }
    }
}
