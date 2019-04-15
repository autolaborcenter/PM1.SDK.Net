using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Autolabor.PM1.TestTool {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private const string AutoSelectString = "自动选择";
        private const string UITestString = "界面测试";

        private readonly MainWindowContext _context;
        private CancellationTokenSource _connecting;

        public MainWindow() {
            InitializeComponent();
            _context = (MainWindowContext)DataContext;
            SerialPortCombo.Items.Add(AutoSelectString);
            SerialPortCombo.SelectedIndex = 0;
        }

        public void RefreshCombo() {
            SerialPortCombo.Items.Clear();
            SerialPortCombo.Items.Add(AutoSelectString);
            SerialPortCombo.Items.Add(new Separator());
            foreach (var port in SerialPort.GetPortNames())
                SerialPortCombo.Items.Add(port);
#if DEBUG
            if (SerialPortCombo.Items.Count > 2)
                SerialPortCombo.Items.Add(new Separator());
            SerialPortCombo.Items.Add(UITestString);
#endif
        }

        private void ComboBox_DropDownOpened(object sender, System.EventArgs e)
            => RefreshCombo();

        private void ComboBox_DropDownClosed(object sender, EventArgs e) {
            if (!(sender is ComboBox combo)) return;
            if (combo.SelectedIndex == -1)
                combo.SelectedIndex = 0;
        }

        private async void CheckBox_Click(object sender, RoutedEventArgs e) {
            if (_context.Connected)
                _connecting.Cancel();
            else
                await Connect((CheckBox)sender).ConfigureAwait(false);
        }

        private async Task Connect(CheckBox box) {
            box.IsEnabled = false;
            _context.Connected = true;
            _context.ErrorInfo = "";

            var flag = true;
            var progress = .0;
            _ = Task.Run(async () => {
                while (flag) {
                    _context.Progress = progress;
                    await Task.Delay(20).ConfigureAwait(false);
                }
                await Task.Delay(100).ConfigureAwait(false);
                _context.Progress = progress;
            });

            var port = SerialPortCombo.SelectedItem.ToString();
            {
                var temp = SerialPortCombo.SelectedItem;
                RefreshCombo();
                SerialPortCombo.SelectedItem = temp;
            }
            await Task.Run(async () => {
                try {
#if DEBUG
                    if (port != UITestString)
#endif
                        port = Methods.Initialize(port == AutoSelectString ? "" : port, null, out progress);

                    SerialPortCombo.Dispatch((it) => it.SelectedItem = port);
                } catch (Exception exception) {
                    _context.Connected = false;
                    _context.ErrorInfo = exception.Message;
                    return;
                } finally {
                    flag = false;
                    box.Dispatch((it) => it.IsEnabled = true);
                }

                _connecting = new CancellationTokenSource();
                await ToolFunctions.Handle(_context, _connecting).ConfigureAwait(false);
            }).ConfigureAwait(true);
        }

        private void Odometry_Reset_Button_Click(object sender, RoutedEventArgs e) {
            try {
                Methods.ResetOdometry();
            } catch (Exception exception) {
                _context.ErrorInfo = exception.Message;
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e) {
            var toggle = (ToggleButton)sender;
            if (toggle.IsChecked == true) {
                Methods.Paused = true;
                toggle.Content = "已暂停";
            } else {
                Methods.Paused = false;
                toggle.Content = "暂停";
            }
        }

        private void Clear_Error_Info(object sender, RoutedEventArgs e)
            => _context.ErrorInfo = "";

        private StateEnum ChassisState {
            set {
                try {
                    _context.State = null;
                    Methods.State = value;
                } catch (Exception exception) {
                    _context.ErrorInfo = exception.Message;
                }
            }
        }

        private void Unlock_Click(object sender, RoutedEventArgs e)
            => ChassisState = StateEnum.Unlocked;

        private void Lock_Click(object sender, RoutedEventArgs e)
            => ChassisState = StateEnum.Locked;

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            var grid = (Grid)sender;

            if (e.NewSize.Height > 500) {
                grid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                grid.RowDefinitions[3].Height = GridLength.Auto;

                grid.ColumnDefinitions[2].Width = GridLength.Auto;

                Grid.SetRow(ActionTitle, 2);
                Grid.SetColumn(ActionTitle, 1);

                Grid.SetRow(ActionEditor, 3);
                Grid.SetColumn(ActionEditor, 1);

                if (e.NewSize.Width > 500) {
                    CanvasBorder.BorderThickness = new Thickness(1);
                    grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[1].Width = new GridLength(240, GridUnitType.Pixel);
                } else {
                    CanvasBorder.BorderThickness = new Thickness(0);
                    grid.ColumnDefinitions[0].Width = GridLength.Auto;
                    grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                }
            } else {
                grid.RowDefinitions[1].Height = GridLength.Auto;
                grid.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);

                Grid.SetRow(ActionTitle, 0);
                Grid.SetColumn(ActionTitle, 2);

                Grid.SetRow(ActionEditor, 1);
                Grid.SetColumn(ActionEditor, 2);

                if (e.NewSize.Width > 600) {
                    CanvasBorder.BorderThickness = new Thickness(1);
                    grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[1].Width = GridLength.Auto;
                    grid.ColumnDefinitions[2].Width = new GridLength(240, GridUnitType.Pixel);
                } else {
                    CanvasBorder.BorderThickness = new Thickness(0);
                    grid.ColumnDefinitions[0].Width = GridLength.Auto;
                    grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                }
            }
        }

        private void Grid_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
            => ActionList.Items.Clear();

        private struct ActionConfig {
            public double v, w, range;
            public bool timeBased;

            public override string ToString()
                => string.Format(
                    CultureInfo.InvariantCulture,
                    "v = {0}m/s | ω = {1}°/s | {2}{3}",
                    ToolFunctions.Format("0.##",v ),
                    ToolFunctions.Format("0.#",w / Math.PI * 180),
                    ToolFunctions.Format("0.#",range),
                    timeBased ? "s" : "m");
        }

        private Task task = null;

        private void ActionEditor_OnCompleted(double v, double w, bool timeBased, double range) {

            ActionList.Items.Add(new ActionConfig { v = v, w = w, range = range, timeBased = timeBased });

            if (task == null) {
                var flag = true;
                var progress = .0;
                _ = Task.Run(async () => {
                    while (flag) {
                        _context.Progress = progress;
                        await Task.Delay(20).ConfigureAwait(false);
                    }
                    await Task.Delay(100).ConfigureAwait(false);
                    _context.Progress = progress;
                });
                task = Task.Run(() => {
                    try {
                        while (ActionList.Items.Count > 0) {
                            ActionList.Dispatch(it => it.SelectedIndex = 0);
                            if (ActionList.Items[0] is ActionConfig action) {
                                ActionList.Dispatch(it => it.SelectedIndex = 0);
                                if (action.timeBased)
                                    Methods.DriveTiming(action.v, action.w, action.range, out progress);
                                else
                                    Methods.DriveSpatial(action.v, action.w, action.range, out progress);
                            }
                            ActionList.Dispatch(it => it.Items.RemoveAt(0));
                        }
                    } catch (Exception exception) {
                        _context.ErrorInfo = exception.Message;
                    }
                    task = null;
                    flag = false;
                });
            }
        }
    }
}