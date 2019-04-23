using System.Windows;

namespace Autolabor.PM1.TestTool.MainWindowItems.CalibrationTab {
    internal class TabContext : BindableBase {
        public enum StateEnum {
            Normal,
            Calibrating0,
            Calibrating1
        }

        private StateEnum _state = StateEnum.Normal;

        public StateEnum State {
            get => _state;
            set => SetProperty(ref _state, value);
        }
    }
}
