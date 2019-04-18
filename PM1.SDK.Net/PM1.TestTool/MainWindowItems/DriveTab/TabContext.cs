using System;

namespace Autolabor.PM1.TestTool.MainWindowItems.DriveTab {
    internal class TabContext : BindableBase {

        public static double Size => 340;
        public static double TouchSize => 40;
        public static double Radius => (Size - TouchSize) / 2;
        public static double MaxSpeed => 6 * Math.PI;

        private double _speedRange,
                       _x,
                       _y;
        public TabContext() {
            _speedRange = 0.1 * MaxSpeed;
            _x = _y = Radius;
            LimitedLeft = LimitedTop = Radius;
        }

        public double SpeedRange {
            get => _speedRange;
            set {
                if (!SetProperty(ref _speedRange, value)) return;
                Notify(nameof(SpeedRatioText));
            }
        }

        public string SpeedRatioText
            => ToolFunctions.Format("0.%", _speedRange / MaxSpeed);

        public double X {
            get => _x;
            set {
                if (_x == value) return;
                _x = value;
                Update();
            }
        }

        public double Y {
            get => _y;
            set {
                if (_y == value) return;
                _y = value;
                Update();
            }
        }

        public double Speed {
            get {
                var xo = _x - Radius;
                var yo = _y - Radius;
                return -SpeedRange * Math.Sign(yo) * Math.Min(1, Math.Sqrt(xo * xo + yo * yo) / (Size / 2));
            }
        }

        public double Rudder {
            get {
                var xo = _x - Radius;
                return Math.PI / 2 * Math.Tan(2 * Math.Sign(xo) * Math.Min(1, Math.Abs(xo) / (Size / 2)));
            }
        }

        public double LimitedLeft { get; private set; }

        public double LimitedTop { get; private set; }

        private void Update() {
            var xo = _x - Radius;
            var yo = _y - Radius;
            var tan = Math.Atan2(yo, xo);
            var r = Math.Min(Size / 2, Math.Sqrt(xo * xo + yo * yo));

            LimitedLeft = r * Math.Cos(tan) + Radius;
            Notify(nameof(LimitedLeft));

            LimitedTop = r * Math.Sin(tan) + Radius;
            Notify(nameof(LimitedTop));
        }
    }
}
