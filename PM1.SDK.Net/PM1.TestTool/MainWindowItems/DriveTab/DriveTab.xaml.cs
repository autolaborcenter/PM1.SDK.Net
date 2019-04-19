﻿using System;
using System.Threading.Tasks;
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

        Task task;

        MouseDevice mouseDevice;

        volatile bool flag;

        public void OnEnter() {
            task = Task.Run(async () => {
                flag = true;
                while (flag) {
                    await Task.Delay(50).ConfigureAwait(false);
                    try {
                        if (_windowContext?.State == MainWindowContext.ConnectionState.Connected)
                            Methods.PhysicalTarget = (_tabContext.Speed, _tabContext.Rudder);
                    } catch (Exception exception) {
                        _windowContext.ErrorInfo = exception.Message;
                    }
                }
                task = null;
            });
        }

        public void OnLeave() {
            flag = false;
            task?.Wait();
        }

        private void Tab_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => _windowContext = e.NewValue as MainWindowContext;

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => _tabContext = e.NewValue as TabContext;

        private void Drag_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (e.MouseDevice.Captured == null) {
                    mouseDevice = e.MouseDevice;
                    e.MouseDevice.Capture(Origin);
                }
                var position = e.GetPosition(Origin);
                _tabContext.X = position.X - TabContext.TouchSize / 2;
                _tabContext.Y = position.Y - TabContext.TouchSize / 2;
            }
            else ReleaseMouse();
        }

        private void Grid_TouchUp(object sender, TouchEventArgs e)
            => ReleaseMouse();

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
            => ReleaseMouse();

        void ReleaseMouse() {
            mouseDevice?.Capture(null);
            _tabContext.X =
            _tabContext.Y = TabContext.Radius;
        }
    }
}
