using System.Windows;

namespace Autolabor.PM1.TestTool.MainWindowItems.CalibrationTab {
    internal class CalculateContext : BindableBase {
        private string _unit;
        private double _actual, _odometry, _current;

        public string Unit {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public double Actual {
            get => _actual;
            set {
                if (SetProperty(ref _actual, value))
                    Notify(nameof(Calculated));
            }
        }

        public double Odometry {
            get => _odometry;
            set {
                if (SetProperty(ref _odometry, value)) {
                    _actual = _odometry;
                    Notify(nameof(Actual));
                    Notify(nameof(Calculated));
                }
            }
        }

        public double Current {
            get => _current;
            set {
                if (SetProperty(ref _current, value))
                    Notify(nameof(Calculated));
            }
        }

        public double Calculated => _actual / _odometry * _current;
    }

    /// <summary>
    /// CalculateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CalculateWindow : Window {
        private CalculateContext _context;

        public CalculateWindow(double odometry,string unit, double current) {
            _context = new CalculateContext {
                Odometry = odometry,
                Unit = unit,
                Current = current
            };

            InitializeComponent();
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var @new = (CalculateContext)e.NewValue;
            @new.Odometry = _context.Odometry;
            @new.Current = _context.Current;
            @new.Unit = _context.Unit;
            _context = @new;
        }
    }
}
