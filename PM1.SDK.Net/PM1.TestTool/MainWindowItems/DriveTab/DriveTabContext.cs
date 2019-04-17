using System;
using System.Windows;

namespace Autolabor.PM1.TestTool.MainWindowItems.DriveTab {
    internal class TabContext : BindableBase {

        public static double Size => 360;
        public static double TouchSize => 40;
        public static double Radius => (Size - TouchSize) / 2;
        public static double MaxSpeed => 6 * Math.PI;

        private double _speed,
                       _x,
                       _y;

        private double _limitedLeft, 
                       _limitedTop;

        public TabContext() {
            _speed = 0.1 * MaxSpeed;
            _x = _y = Size / 2;
            _limitedLeft = _limitedTop = Radius;
        }

        public double Speed {
            get => _speed;
            set {
                var pre = _speed;
                if (!SetProperty(ref _speed, value)) return;
                Notify(nameof(SpeedRatioText));
            }
        }

        public string SpeedRatioText
            => ToolFunctions.Format("0.%", _speed / MaxSpeed);

        public double X {
            get => _x;
            set {
                if (!SetProperty(ref _x, value)) return;
                Update();
            }
        }

        public double Y {
            get => _y;
            set {
                if (!SetProperty(ref _y, value)) return;
                Update();
            }
        }

        public double LimitedLeft => _limitedLeft;

        public double LimitedTop => _limitedTop;

        private void Update() {
            var xo = _x - Size / 2;
            var yo = _y - Size / 2;
            var tan = Math.Atan2(yo, xo);
            var r = Math.Min(Radius, Math.Sqrt(xo * xo + yo * yo));
            _limitedLeft = r * Math.Cos(tan) + Radius;
            _limitedTop = r * Math.Sin(tan) + Radius;
            Notify(nameof(LimitedLeft));
            Notify(nameof(LimitedTop));
        }
    }
}
