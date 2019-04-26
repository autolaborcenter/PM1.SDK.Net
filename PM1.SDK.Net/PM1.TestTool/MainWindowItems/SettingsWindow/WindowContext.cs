using System.Runtime.CompilerServices;
using System.Windows;
using static Autolabor.PM1.Parameters;

namespace Autolabor.PM1.TestTool.MainWindowItems.SettingsWindow {
    internal class WindowContext : BindableBase {
        private readonly GlobalParameter<double>
            _gLength = new GlobalParameter<double>(nameof(Length)),
            _gWidth = new GlobalParameter<double>(nameof(Width)),
            _gWheelRadius = new GlobalParameter<double>(nameof(WheelRadius)),
            _gOptimizeWidth = new GlobalParameter<double>(nameof(OptimizeWidth)),
            _gAcceleration = new GlobalParameter<double>(nameof(Acceleration)),
            _gMaxV = new GlobalParameter<double>(nameof(MaxV)),
            _gMaxW = new GlobalParameter<double>(nameof(MaxW));

        private string _helpText;

        private bool SetProperty(GlobalParameter<double> field,
                                 double value,
                                 [CallerMemberName] string propertyName = null)
            => SetProperty(field.Value ?? double.NaN,
                           value, 
                           it => field.Value = it,
                           propertyName);

        public void ResetParameters() {
            Application.Current.Properties.Clear();
            Notify(nameof(Width));
            Notify(nameof(Length));
            Notify(nameof(WheelRadius));
            Notify(nameof(OptimizeWidth));
            Notify(nameof(Acceleration));
            Notify(nameof(MaxV));
            Notify(nameof(MaxW));
        }

        public double Width {
            get => _gWidth.Value ?? Methods.Parameters[IdEnum.Width].Default;
            set {
                try { Methods.Parameters[IdEnum.Width].Value = value; } catch { }
                SetProperty(_gWidth, value);
            }
        }

        public double Length {
            get => _gLength.Value ?? Methods.Parameters[IdEnum.Length].Default;
            set {
                try { Methods.Parameters[IdEnum.Length].Value = value; } catch { }
                SetProperty(_gLength, value);
            }
        }

        public double WheelRadius {
            get => _gWheelRadius.Value ?? Methods.Parameters[IdEnum.WheelRadius].Default;
            set {
                try { Methods.Parameters[IdEnum.WheelRadius].Value = value; } catch { }
                SetProperty(_gWheelRadius, value);
            }
        }

        public double OptimizeWidth {
            get => _gOptimizeWidth.Value ?? Methods.Parameters[IdEnum.OptimizeWidth].Default;
            set {
                try { Methods.Parameters[IdEnum.OptimizeWidth].Value = value; } catch { }
                SetProperty(_gOptimizeWidth, value);
            }
        }

        public double Acceleration {
            get => _gAcceleration.Value ?? Methods.Parameters[IdEnum.Acceleration].Default;
            set {
                try { Methods.Parameters[IdEnum.Acceleration].Value = value; } catch { }
                SetProperty(_gAcceleration, value);
            }
        }

        public double MaxV {
            get => _gMaxV.Value ?? Methods.Parameters[IdEnum.MaxV].Default;
            set {
                try { Methods.Parameters[IdEnum.MaxV].Value = value; } catch { }
                SetProperty(_gMaxV, value);
            }
        }

        public double MaxW {
            get => _gMaxW.Value ?? Methods.Parameters[IdEnum.MaxW].Default;
            set {
                try { Methods.Parameters[IdEnum.MaxW].Value = value; } catch { }
                SetProperty(_gMaxW, value);
            }
        }

        public string HelpText {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }
    }
}
