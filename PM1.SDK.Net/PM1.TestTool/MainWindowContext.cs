namespace Autolabor.PM1.TestTool
{
    public class MainWindowContext : BindableBase
    {
        private bool _connected = false;

        private string _timeString = "0";
        private string _errorInfo = "";

        private double _progress = 0;

        public bool Connected {
            get => _connected;
            set {
                if(SetProperty(ref _connected, value))
                    Notify(nameof(Disconnected));
            }
        }

        public bool Disconnected => !_connected;

        public string TimeString {
            get => _timeString;
            set => SetProperty(ref _timeString, value);
        }

        public string ErrorInfo {
            get => _errorInfo;
            set => SetProperty(ref _errorInfo, value);
        }

        public double Progress {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }
    }
}
