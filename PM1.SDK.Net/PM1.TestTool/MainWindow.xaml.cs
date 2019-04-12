using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Autolabor.PM1.TestTool {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private readonly MainWindowContext _context;

        public MainWindow() {
            InitializeComponent();
            _context = (MainWindowContext)DataContext;
            SerialPortCombo.Items.Add("自动选择");
            SerialPortCombo.SelectedIndex = 0;
        }

        private void ComboBox_DropDownOpened(object sender, System.EventArgs e) {
            if (!(sender is ComboBox combo)) return;
            combo.Items.Clear();
            combo.Items.Add("自动选择");
            foreach (var port in SerialPort.GetPortNames())
                combo.Items.Add(port);
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e) {
            if (!(sender is ComboBox combo)) return;
            if (combo.SelectedIndex == -1)
                combo.SelectedIndex = 0;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e) {
            if (!(sender is CheckBox box)) return;
            box.IsEnabled = false;
            if (_context.Connected) {
                box.IsChecked = false;
                _context.Connected = false;
                _context.Progress = 0;
                try {
                    Methods.Shutdown();
                } catch (Exception exception) {
                    MessageBox.Show(exception.Message);
                }
                box.IsEnabled = true;
            } else {
                var port = SerialPortCombo.SelectedItem.ToString();
                var progress = .0;
                _ = Task.Run(async () => {
                    while (progress >= 0) {
                        _context.Progress = progress;
                        await Task.Delay(20);
                    }
                });
                _ = Task.Run(async () => {
                    try {
                        var name = Methods.Initialize(
                            port == "自动选择" ? "" : port,
                            new ChassisConfig {
                                Width = double.NaN,
                                Length = double.NaN,
                                WheelRadius = double.NaN,
                                OptimizeWidth = double.NaN,
                                Acceleration = double.NaN
                            },
                            out progress);
                    } catch (Exception exception) {
                        progress = -1;
                        box.Dispatcher.Invoke(() => {
                            box.IsChecked = false;
                            box.IsEnabled = true;
                        });
                        MessageBox.Show(exception.Message);
                        return;
                    }

                    _context.Connected = true;
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (_context.Connected) {
                        _context.TimeString = (stopwatch.ElapsedMilliseconds / 1000).ToString();
                        await Task.Delay(499);
                    }
                });
            }
        }
    }
}
