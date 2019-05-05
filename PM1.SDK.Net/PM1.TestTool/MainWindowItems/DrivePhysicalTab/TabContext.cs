using System;

namespace Autolabor.PM1.TestTool.MainWindowItems.DrivePhysicalTab {
    internal class TabContext : BindableBase {
        public static double MaxSpeed => 6 * Math.PI;
        public static double TouchSize => 40;
        public double Radius => (Size - TouchSize) / 2;

        private double _size,
                       _speedRange,
                       _x,
                       _y;
        public TabContext() {
            _speedRange = 0.1 * MaxSpeed;
            _x = _y = Radius;
            LimitedLeft = LimitedTop = Radius;
        }

        public double Size {
            get => _size;
            set {
                if(!SetProperty(ref _size, value)) return;
                _x = _y = Radius;
                Update();
            }
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
                var yo = _y - Radius;
                var rr = xo * xo + yo * yo;
                return rr < 1E-2 ? double.NaN : -(Math.Atan2(Math.Abs(yo), xo) - Math.PI / 2);
            }
        }

        public double LimitedLeft { get; private set; }

        public double LimitedTop { get; private set; }

        private void Update() {
            var xo = _x - Radius;
            var yo = _y - Radius;
            var theta = Math.Atan2(yo, xo);
            var r = Math.Min(Size / 2, Math.Sqrt(xo * xo + yo * yo));

            LimitedLeft = r * Math.Cos(theta) + Radius;
            Notify(nameof(LimitedLeft));

            LimitedTop = r * Math.Sin(theta) + Radius;
            Notify(nameof(LimitedTop));
        }
    }
}
