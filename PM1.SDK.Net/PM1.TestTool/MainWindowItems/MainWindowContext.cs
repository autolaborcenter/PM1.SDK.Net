using System;
using System.Windows.Media;

namespace Autolabor.PM1.TestTool {
    public class MainWindowContext : BindableBase {
        private bool _connected = false;
        private double _progress = 0;
        private string _connectedTime = "0.0";
        private string _odometry = "0.0, 0.0, 0.0°";
        private string _errorInfo = "";
        private StateEnum? _state = StateEnum.Offline;

        public bool Connected {
            get => _connected;
            set {
                if (SetProperty(ref _connected, value)) {
                    Notify(nameof(Disconnected));
                    if (!value) State = StateEnum.Offline;
                }
            }
        }

        public bool Disconnected => !_connected;

        public double Progress {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public string ConnectedTime {
            get => _connectedTime;
            set => SetProperty(ref _connectedTime, value);
        }

        public string Odometry {
            get => _odometry;
            set => SetProperty(ref _odometry, value);
        }

        public string ErrorInfo {
            get => _errorInfo;
            set => SetProperty(ref _errorInfo, value);
        }

        private static readonly SolidColorBrush
            ErrorBrush = new SolidColorBrush(Colors.Firebrick),
            LockBrush = new SolidColorBrush(Colors.DimGray),
            UnLockedBrush = new SolidColorBrush(Colors.LawnGreen),
            OfflineBrush = new SolidColorBrush(Colors.LightGray);

        public StateEnum? State {
            get => _state;
            set {
                if (Equals(_state, value)) return;
                _state = value;
                if (!value.HasValue) return;

                Notify(nameof(IsLocked));
                Notify(nameof(IsUnlocked));
                Notify(nameof(IsError));
                Notify(nameof(StateAreaBrush));
            }
        }

        public bool IsLocked => _state == StateEnum.Locked;
        public bool IsUnlocked => _state == StateEnum.Unlocked;
        public bool IsError => _state == StateEnum.Error;

        public Brush StateAreaBrush {
            get {
                switch (_state) {
                    case StateEnum.Offline:
                        return OfflineBrush;
                    case StateEnum.Unlocked:
                        return UnLockedBrush;
                    case StateEnum.Locked:
                        return LockBrush;
                    case StateEnum.Error:
                        return ErrorBrush;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_state), _state, "imposible!");
                }
            }
        }
    }
}
