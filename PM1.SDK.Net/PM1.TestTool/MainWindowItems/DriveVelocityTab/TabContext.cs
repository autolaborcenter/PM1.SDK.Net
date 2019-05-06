using System;

namespace Autolabor.PM1.TestTool.MainWindowItems.DriveVelocityTab {
    internal class TabContext : BindableBase {
        private static readonly GlobalParameter<double>
            _gMaxV = new GlobalParameter<double>(nameof(Parameters.IdEnum.MaxV)),
            _gMaxW = new GlobalParameter<double>(nameof(Parameters.IdEnum.MaxW));

        private static readonly Parameter
            _maxV = Methods.Parameters[Parameters.IdEnum.MaxV],
            _maxW = Methods.Parameters[Parameters.IdEnum.MaxW];

        public static double TouchSize => 40;

        private double
            _canvasWidth,
            _canvasHeight,
            _top,
            _left,

            _vRange = 0.25,
            _wRange = 0.25;

        public double V {
            get {
                var temp = -2 * (Top - OY) / Height;
                return Math.Abs(temp) < 0.1 ? 0 : (_gMaxV.Value ?? _maxV.Default) * _vRange * temp;
            }
        }

        public double W => (V < 0 ? -1 : 1) * (_gMaxW.Value ?? _maxW.Default) * _wRange * 2 * (Left - OX) / Width;

        public double VRange {
            get => _vRange;
            set => SetProperty(ref _vRange, value);
        }

        public double WRange {
            get => _wRange;
            set => SetProperty(ref _wRange, value);
        }

        public double Height {
            get => _canvasHeight;
            set {
                if (!SetProperty(ref _canvasHeight, value)) return;
                Top = OY;
            }
        }

        public double Width {
            get => _canvasWidth;
            set {
                if (!SetProperty(ref _canvasWidth, value)) return;
                Left = OX;
            }
        }

        public double OX => (Width - TouchSize) / 2;

        public double OY => (Height - TouchSize) / 2;

        public double Top {
            get => _top;
            set => SetProperty(ref _top, Math.Min(Height - TouchSize, Math.Max(0, value)));
        }

        public double Left {
            get => _left;
            set => SetProperty(ref _left, Math.Min(Width - TouchSize, Math.Max(0, value)));
        }
    }
}
