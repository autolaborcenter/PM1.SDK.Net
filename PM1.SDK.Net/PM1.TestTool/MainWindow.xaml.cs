using System;
using System.Diagnostics;
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

        private void ComboBox_DropDownOpened(object sender, System.EventArgs e) {
            if (!(sender is ComboBox combo)) return;
            combo.Items.Clear();
            combo.Items.Add(AutoSelectString);
            combo.Items.Add(new Separator());
            foreach (var port in SerialPort.GetPortNames())
                combo.Items.Add(port);
#if DEBUG
            if (combo.Items.Count > 2)
                combo.Items.Add(new Separator());
            combo.Items.Add(UITestString);
#endif
        }

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
            await Task.Run(async () => {
                try {
#if DEBUG
                    if (port != UITestString)
#endif
                        port = Methods.Initialize(port == AutoSelectString ? "" : port, null, out progress);

                    _context.Connected = true;
                    SerialPortCombo.Dispatch((it) => it.SelectedItem = port);
                } catch (Exception exception) {
                    box.Dispatch((it) => it.IsChecked = false);
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

                Grid.SetRow(ActionEdtor, 3);
                Grid.SetColumn(ActionEdtor, 1);

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

                Grid.SetRow(ActionEdtor, 1);
                Grid.SetColumn(ActionEdtor, 2);

                if (e.NewSize.Width > 800) {
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

        private void Grid_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ActionList.Items.Clear();
            ActionEdtor.Reset();
        }

        private void ActionEdtor_OnCompleted(double v, double w, bool timeBased, double range) {
            MessageBox.Show(string.Format("{0}, {1}, {2}, {3}", v, w, timeBased, range));
        }
    }
}
