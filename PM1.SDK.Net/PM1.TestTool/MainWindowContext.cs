namespace Autolabor.PM1.TestTool {
    public class MainWindowContext : BindableBase {
        private bool _connected = false;

        private string _connectedTime = "0.0";
        private string _errorInfo = "";

        private double _progress = 0;

        private string _odometry = "0, 0, 0";

        public bool Connected {
            get => _connected;
            set {
                if (SetProperty(ref _connected, value))
                    Notify(nameof(Disconnected));
            }
        }

        public bool Disconnected => !_connected;

        public string ConnectedTime {
            get => _connectedTime;
            set => SetProperty(ref _connectedTime, value);
        }

        public string ErrorInfo {
            get => _errorInfo;
            set => SetProperty(ref _errorInfo, value);
        }

        public double Progress {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public string Odometry {
            get => _odometry;
            set => SetProperty(ref _odometry, value);
        }
    }
}
