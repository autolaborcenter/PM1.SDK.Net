using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            try {
                Methods.Shutdown();
            } catch (Exception) {
                // ignore
            }
        }

        private double _delta = 0.1;
        private bool? _left = null;

        private void StartButton_Click(object sender, RoutedEventArgs e) {
            HelpText.Text = "已进入后轮零位校准流程，请将机器人移动到合适的位置，确保前方有至少 1.5 米的空旷区域。校准开始后，机器人将按照内部里程计前进 1.5 米，若后轮零位不准，此动作会发生侧偏。待机器人停止后，请将侧偏的情况反馈给机器人，因此建议标记机器人开始时的方向，以便反馈正确的侧偏。";
            StartButton.Visibility = Visibility.Hidden;
            GoButton.Visibility = Visibility.Visible;
            _delta = 0.1;
            _left = null;
        }

        private void GoButton_Click(object sender, RoutedEventArgs e) {
            HelpText.Text = "正在前进。";
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
            _ = Task.Run(() => {
                try {
                    Methods.DriveSpatial(0.15, 0, 2 * 1.5, out progress);
                    this.Dispatch(_ => {
                        HelpText.Text = "动作完成。现在点击下面的按钮反馈机器人侧偏情况。点击按钮后，机器人将后退到原位，并尝试调整后轮零位。";
                        GoButton.Visibility = Visibility.Hidden;
                    });
                } catch (Exception exception) {
                    _context.ErrorInfo = exception.Message;
                    return;
                } finally {
                    flag = false;
                }
            });
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e) {
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

            switch (_left) {
                case null:
                    _delta = 0.1;
                    break;
                case false:
                    _delta /= -2;
                    break;
            }
            _left = true;

            _ = Task.Run(() => {
                try {
                    Methods.DriveSpatial(-0.15, 0, 2 * 1.5, out progress);
                    StartButton.Dispatch(it => it.Visibility = Visibility.Visible);
                } catch (Exception exception) {
                    _context.ErrorInfo = exception.Message;
                    return;
                } finally {
                    flag = false;
                }
            });
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e) {
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

            switch (_left) {
                case null:
                    _delta = 0.1;
                    break;
                case false:
                    _delta /= -2;
                    break;
            }
            _left = true;

            _ = Task.Run(() => {
                try {
                    Methods.DriveSpatial(-0.15, 0, 2 * 1.5, out progress);
                    Methods.AdjustRudder(_delta, out progress);
                    this.Dispatch(_ => {
                        HelpText.Text = "调整完成，准备下一次测试。";
                        GoButton.Visibility = Visibility.Visible;
                    });
                } catch (Exception exception) {
                    _context.ErrorInfo = exception.Message;
                    return;
                } finally {
                    flag = false;
                }
            });
        }

        private void RightButton_Click(object sender, RoutedEventArgs e) {
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

            switch (_left) {
                case null:
                    _delta = -0.1;
                    break;
                case true:
                    _delta /= -2;
                    break;
            }
            _left = false;

            _ = Task.Run(() => {
                try {
                    Methods.DriveSpatial(-0.15, 0, 2 * 1.5, out progress);
                    Methods.AdjustRudder(_delta, out progress);
                    this.Dispatch(_ => {
                        HelpText.Text = "调整完成，准备下一次测试。";
                        GoButton.Visibility = Visibility.Visible;
                    });
                } catch (Exception exception) {
                    _context.ErrorInfo = exception.Message;
                    return;
                } finally {
                    flag = false;
                }
            });
        }
    }
}