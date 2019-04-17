using System;

namespace Autolabor.PM1.TestTool.MainWindowItems.DriveTab {
    internal class TabContext : BindableBase {

        public static double Size => 360;

        public static double TouchSize => 30;

        public static double MaxSpeed => 6 * Math.PI;

        private double _speed;

        public TabContext() 
            => _speed = 0.1 * MaxSpeed;

        public double Speed {
            get => _speed;
            set {
                if(!SetProperty(ref _speed, value))return;
                Notify(nameof(SpeedRatioText));
            }
        }

        public string SpeedRatioText
            => ToolFunctions.Format("0.%", _speed / MaxSpeed);
    }
}
