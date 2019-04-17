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

        public TabContext() {
            _speed = 0.1 * MaxSpeed;
            _x = _y = Size / 2;
        }

        public double Speed {
            get => _speed;
            set {
                var pre = _speed;
                if (!SetProperty(ref _speed, value)) return;
                Notify(nameof(SpeedRatioText));

                if (pre <= 0) return;
                _x *= _speed / pre;
                _y *= _speed / pre;
            }
        }

        public string SpeedRatioText
            => ToolFunctions.Format("0.%", _speed / MaxSpeed);

        public double X {
            get => _x;
            set {
                if (!SetProperty(ref _x, value)) return;
                Notify(nameof(LimitedLeft));
            }
        }

        public double Y {
            get => _y;
            set {
                if (!SetProperty(ref _y, value)) return;
                Notify(nameof(LimitedTop));
            }
        }

        public double LimitedLeft => Limit(_x, _y).x;

        public double LimitedTop => Limit(_x, _y).y;

        private (double x, double y) Limit(double x, double y) {
            var xo = x - Size / 2;
            var yo = y - Size / 2;
            var tan = Math.Atan2(yo, xo);
            var r = Math.Min(Radius, Math.Sqrt(xo * xo + yo * yo));
            return (r * Math.Cos(tan) + Radius,
                    r * Math.Sin(tan) + Radius);
        }
    }
}
