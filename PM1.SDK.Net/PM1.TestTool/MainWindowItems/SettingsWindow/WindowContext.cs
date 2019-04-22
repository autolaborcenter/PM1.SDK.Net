using System.Runtime.CompilerServices;

namespace Autolabor.PM1.TestTool.MainWindowItems.SettingsWindow {
    internal class WindowContext : BindableBase {
        private readonly Methods.Parameter
            _pLength = new Methods.Parameter(ParameterId.Length),
            _pWidth = new Methods.Parameter(ParameterId.Width),
            _pWheelRadius = new Methods.Parameter(ParameterId.WheelRadius),
            _pOptimizeWidth = new Methods.Parameter(ParameterId.OptimizeWidth),
            _pAcceleration = new Methods.Parameter(ParameterId.Acceleration),
            _pMaxV = new Methods.Parameter(ParameterId.MaxV),
            _pMaxW = new Methods.Parameter(ParameterId.MaxW);

        private readonly GlobalParameter<double>
            _gLength = new GlobalParameter<double>(nameof(Length)),
            _gWidth = new GlobalParameter<double>(nameof(Width)),
            _gWheelRadius = new GlobalParameter<double>(nameof(WheelRadius)),
            _gOptimizeWidth = new GlobalParameter<double>(nameof(OptimizeWidth)),
            _gAcceleration = new GlobalParameter<double>(nameof(Acceleration)),
            _gMaxV = new GlobalParameter<double>(nameof(VMax)),
            _gMaxW = new GlobalParameter<double>(nameof(WMax));

        private string _helpText;

        private bool SetProperty(GlobalParameter<double> field,
                                 double value,
                                 [CallerMemberName] string propertyName = null)
            => SetProperty(field.Value ?? double.NaN,
                           value, 
                           it => field.Value = it,
                           propertyName);

        public void ResetParameters() {
            Width = _pWidth.Default;
            Length = _pLength.Default;
            WheelRadius = _pWheelRadius.Default;
            OptimizeWidth = _pOptimizeWidth.Default;
            Acceleration = _pAcceleration.Default;
            VMax = _pMaxV.Default;
            WMax = _pMaxW.Default;
        }

        public double Width {
            get => _gWidth.Value ?? _pWidth.Default;
            set {
                try { _pWidth.Current = value; } catch { }
                SetProperty(_gWidth, value);
            }
        }

        public double Length {
            get => _gLength.Value ?? _pLength.Default;
            set {
                try { _pLength.Current = value; } catch { }
                SetProperty(_gLength, value);
            }
        }

        public double WheelRadius {
            get => _gWheelRadius.Value ?? _pWheelRadius.Default;
            set {
                try { _pWheelRadius.Current = value; } catch { }
                SetProperty(_gWheelRadius, value);
            }
        }

        public double OptimizeWidth {
            get => _gOptimizeWidth.Value ?? _pOptimizeWidth.Default;
            set {
                try { _pWheelRadius.Current = value; } catch { }
                SetProperty(_gWheelRadius, value);
            }
        }

        public double Acceleration {
            get => _gAcceleration.Value ?? _pAcceleration.Default;
            set {
                try { _pAcceleration.Current = value; } catch { }
                SetProperty(_gAcceleration, value);
            }
        }

        public double VMax {
            get => _gMaxV.Value ?? _pMaxV.Default;
            set {
                try { _pMaxV.Current = value; } catch { }
                SetProperty(_gMaxV, value);
            }
        }

        public double WMax {
            get => _gMaxW.Value ?? _pMaxW.Default;
            set {
                try { _pMaxW.Current = value; } catch { }
                SetProperty(_gMaxW, value);
            }
        }

        public string HelpText {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }
    }
}
