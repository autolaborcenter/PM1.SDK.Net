using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PM1.TestTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowContext _context;

        public MainWindow()
        {
            InitializeComponent();
            _context = (MainWindowContext) DataContext;
        }

        private void ComboBox_DropDownOpened(object sender, System.EventArgs e)
        {
            if (!(sender is ComboBox combo)) return;
            combo.Items.Clear();
            combo.Items.Add("自动选择");
            foreach(var port in SerialPort.GetPortNames())
                combo.Items.Add(port);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _context.Connected = true;
            Task.Run(async () =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (_context.Connected)
                {
                    _context.TimeString = (stopwatch.ElapsedMilliseconds / 1000).ToString();
                    await Task.Delay(499);
                }
            });
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _context.Connected = false;
            _context.Progress = 0;
        }
    }
}
