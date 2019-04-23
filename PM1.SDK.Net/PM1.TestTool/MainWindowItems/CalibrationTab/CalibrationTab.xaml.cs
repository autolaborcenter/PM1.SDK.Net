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
        private static readonly Methods.Parameter OptimizeWidth
            = new Methods.Parameter(ParameterId.OptimizeWidth);

        private volatile bool _flag;
        private Task _task;

        private TabContext _tabContext;

        public CalibrationTab() => InitializeComponent();

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => _tabContext = e.NewValue as TabContext;

        public void OnEnter() {
            try { OptimizeWidth.Current = Math.PI / 60; } catch { }
            _task = Task.Run(async () => {
                _flag = true;
                while (_flag) {
                    await Task.Delay(50).ConfigureAwait(false);
                    switch (_tabContext.State) {
                        case TabContext.StateEnum.Normal:
                            Methods.PhysicalTarget = (0, double.NaN);
                            break;
                        case TabContext.StateEnum.Calibrating0:
                            Methods.PhysicalTarget = (0.3 * Math.PI, 0);
                            break;
                        case TabContext.StateEnum.Calibrating1:
                            Methods.PhysicalTarget = (0.3 * Math.PI, -Math.PI / 2);
                            break;
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
            } catch { }
        }

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            switch (((Control)sender).Tag) {
                case "0":
                    _tabContext.State = TabContext.StateEnum.Calibrating0;
                    break;
                case "1":
                    _tabContext.State = TabContext.StateEnum.Calibrating1;
                    break;
            }
        }

        private void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) 
            => _tabContext.State = TabContext.StateEnum.Normal;
    }
}
