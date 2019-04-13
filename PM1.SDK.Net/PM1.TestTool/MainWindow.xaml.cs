using System;
using System.Diagnostics;
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
        private CancellationTokenSource connecting;

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
            foreach (var port in SerialPort.GetPortNames())
                combo.Items.Add(port);
#if DEBUG
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
                connecting.Cancel();
            else
                await Connect((CheckBox)sender).ConfigureAwait(false);
        }

        private async Task Connect(CheckBox box) {
            box.IsEnabled = false;

            var cancellation = new CancellationTokenSource();
            var progress = .0;
            _ = Task.Run(async () => {
                while (!cancellation.IsCancellationRequested) {
                    _context.Progress = progress;
                    await Task.Delay(20, cancellation.Token).ConfigureAwait(false);
                }
            }, cancellation.Token);

            var port = SerialPortCombo.SelectedItem.ToString();
            await Task.Run(async () => {
                try {
#if DEBUG
                    if (port != UITestString)
#endif
                        port = Methods.Initialize(port == AutoSelectString ? "" : port, null, out progress);

                    _context.Connected = true;
                    Dispatch(SerialPortCombo, (it) => it.SelectedItem = port);
                } catch (Exception exception) {
                    MessageBox.Show(exception.Message);
                    return;
                } finally {
                    cancellation.Cancel();
                    Dispatch(box, (it) => it.IsEnabled = true);
                }

                connecting = new CancellationTokenSource();
                await Handle(box).ConfigureAwait(false);
            }).ConfigureAwait(true);
        }

        private async Task Handle(CheckBox box) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try {
                while (!connecting.IsCancellationRequested) {
                    _context.ConnectedTime = (stopwatch.ElapsedMilliseconds / 1000.0).ToString("0.0");
                    try {
                        //var (_, _, x, y, theta, _, _, _) = Methods.Odometry;
                        //_context.Odometry = string.Format("{0}, {1}, {2}°",
                        //                            x.ToString("0.0"),
                        //                            y.ToString("0.0"),
                        //                            theta.ToString("0.0"));
                    } catch (Exception exception) {
                        MessageBox.Show(exception.Message);
                        return;
                    }
                    await Task.Delay(99, connecting.Token).ConfigureAwait(false);
                }
            } catch (TaskCanceledException) {
                try {
                    Methods.Shutdown();
                } catch (Exception exception) {
                    MessageBox.Show(exception.Message);
                }
            } catch (Exception exception) {
                MessageBox.Show(exception.Message);
            } finally {
                _context.Connected = false;
            }
        }

        private void Odometry_Reset_Button_Click(object sender, RoutedEventArgs e) {
            try {
                Methods.ResetOdometry();
            } catch (Exception exception) {
                MessageBox.Show(exception.Message);
            }
        }

        private static void Dispatch<T>(T control, Action<T> action)
            where T : Control => control.Dispatcher.Invoke(action, control);
    }
}
