using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Autolabor.PM1.TestTool.MainWindowItems.CalibrationTab {
    /// <summary>
    /// CalibrationTab.xaml 的交互逻辑
    /// </summary>
    public partial class CalibrationTab : UserControl, ITabControl {
        private static readonly Methods.Parameter
            OptimizeWidth = new Methods.Parameter(ParameterId.OptimizeWidth),
            Radius = new Methods.Parameter(ParameterId.WheelRadius),
            WidthParameter = new Methods.Parameter(ParameterId.Width);

        private volatile bool _flag;
        private Task _task;

        private MainWindowContext _windowContext;
        private TabContext _tabContext;

        public CalibrationTab() => InitializeComponent();

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => _tabContext = e.NewValue as TabContext;

        private void Tab_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => _windowContext = e.NewValue as MainWindowContext;

        public void OnEnter() {
            try { OptimizeWidth.Current = Math.PI / 60; } catch (Exception exception) {
                _windowContext.ErrorInfo = exception.Message;
            }
            _task = Task.Run(async () => {
                _flag = true;
                while (_flag) {
                    await Task.Delay(50).ConfigureAwait(false);
                    try {
                        switch (_tabContext.State) {
                            case TabContext.StateEnum.Normal:
                                Methods.PhysicalTarget = (0, double.NaN);
                                break;
                            case TabContext.StateEnum.Calibrating0:
                                Methods.PhysicalTarget = (0.3 * Math.PI, 0);
                                break;
                            case TabContext.StateEnum.Calibrating1:
                                Methods.PhysicalTarget = (0.2 * Math.PI, -Math.PI / 2);
                                break;
                        }
                    } catch (Exception exception) {
                        _windowContext.ErrorInfo = exception.Message;
                    }
                }
                _task = null;
            });
        }

        public void OnLeave() {
            _flag = false;
            _task?.Wait();
            try {
                OptimizeWidth.Current
                    = new GlobalParameter<double>(nameof(ParameterId.OptimizeWidth)).Value
                      ?? OptimizeWidth.Default;
            } catch (Exception exception) {
                _windowContext.ErrorInfo = exception.Message;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            try {
                switch (_tabContext.State) {
                    case TabContext.StateEnum.Normal:
                        Methods.ResetOdometry();
                        switch (((Control)sender).Tag) {
                            case "0":
                                _tabContext.State = TabContext.StateEnum.Calibrating0;
                                break;
                            case "1":
                                _tabContext.State = TabContext.StateEnum.Calibrating1;
                                break;
                        }
                        break;
                    case TabContext.StateEnum.Calibrating0:
                        _tabContext.State = TabContext.StateEnum.Normal;
                        new CalculateWindow(Methods.Odometry.x, "米",
                                            Radius.Current.Value) {
                            Owner = Application.Current.MainWindow
                        }.ShowDialog();
                        break;
                    case TabContext.StateEnum.Calibrating1:
                        _tabContext.State = TabContext.StateEnum.Normal;
                        new CalculateWindow(Methods.Odometry.sa.ToDegree(), "°",
                                            WidthParameter.Current.Value) {
                            Owner = Application.Current.MainWindow
                        }.ShowDialog();
                        break;
                }
            } catch (Exception exception) {
                _windowContext.ErrorInfo = exception.Message;
            }
        }
    }
}
