using System;
using System.Windows.Media;

namespace Autolabor.PM1.TestTool {
    public class MainWindowContext : BindableBase {
        public enum WindowState {
            Disconnected,
            Connecting,
            Connected,
        }

        private WindowState _windowState = WindowState.Disconnected;
        private double _progress = 0;
        private string _connectedTime = "0.0";
        private string _odometry = "0.0, 0.0, 0.0°";
        private string _errorInfo = "";
        private StateEnum? _chassisState = StateEnum.Offline;

        public WindowState State {
            get => _windowState;
            set {
                if (!SetProperty(ref _windowState, value)) return;
                Notify(nameof(CheckBoxEnabled));
                Notify(nameof(CheckBoxChecked));
                Notify(nameof(ComboBoxEnabled));
                Notify(nameof(ElementsEnabled));
            }
        }

        public bool CheckBoxChecked => _windowState != WindowState.Disconnected;

        public bool CheckBoxEnabled => _windowState != WindowState.Connecting;

        public bool ComboBoxEnabled => _windowState == WindowState.Disconnected;

        public bool ElementsEnabled => _windowState == WindowState.Connected;

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

        public StateEnum? ChassisState {
            get => _chassisState;
            set {
                if (Equals(_chassisState, value)) return;
                _chassisState = value;
                if (!value.HasValue) return;

                Notify(nameof(IsLocked));
                Notify(nameof(IsUnlocked));
                Notify(nameof(IsError));
                Notify(nameof(StateAreaBrush));
            }
        }

        public bool IsLocked => _chassisState == StateEnum.Locked;

        public bool IsUnlocked => _chassisState == StateEnum.Unlocked;

        public bool IsError => _chassisState == StateEnum.Error;

        public Brush StateAreaBrush {
            get {
                switch (_chassisState) {
                    case StateEnum.Offline:
                        return OfflineBrush;
                    case StateEnum.Unlocked:
                        return UnLockedBrush;
                    case StateEnum.Locked:
                        return LockBrush;
                    case StateEnum.Error:
                        return ErrorBrush;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_chassisState), _chassisState, "imposible!");
                }
            }
        }

        private static readonly SolidColorBrush
            ErrorBrush = new SolidColorBrush(Colors.Firebrick),
            LockBrush = new SolidColorBrush(Colors.DimGray),
            UnLockedBrush = new SolidColorBrush(Colors.LawnGreen),
            OfflineBrush = new SolidColorBrush(Colors.LightGray);
    }
}
