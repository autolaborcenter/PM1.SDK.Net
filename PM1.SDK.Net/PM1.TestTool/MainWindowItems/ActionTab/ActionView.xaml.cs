using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;

namespace Autolabor.PM1.TestTool.MainWindowItems.ActionTab {
    /// <summary>
    /// ActionView.xaml 的交互逻辑
    /// </summary>
    public partial class ActionView : UserControl {

        private class Input {
            public readonly CheckBox check;
            public readonly TextBox text;

            public Input(CheckBox check, TextBox text) {
                this.check = check;
                this.text = text;
            }

            public enum StateEnum {
                Master, Slave, Void, Invalid, Error
            }

            private StateEnum _state = StateEnum.Void;
            private double _value = default;

            public StateEnum State {
                get => _state;
                set {
                    switch (_state = value) {
                        case StateEnum.Master:
                            text.Foreground = Normal;
                            text.IsEnabled =
                            check.IsEnabled = true;
                            check.IsChecked = true;
                            break;
                        case StateEnum.Slave:
                            text.Foreground = Normal;
                            text.IsEnabled =
                            check.IsEnabled = false;
                            check.IsChecked = true;
                            break;
                        case StateEnum.Void:
                            text.Foreground = Normal;
                            text.IsEnabled = true;
                            check.IsEnabled = false;
                            check.IsChecked = false;
                            break;
                        case StateEnum.Invalid:
                            text.Foreground = Normal;
                            text.IsEnabled =
                            check.IsEnabled = false;
                            check.IsChecked = false;
                            break;
                        case StateEnum.Error:
                            text.Foreground = Error;
                            text.IsEnabled =
                            check.IsEnabled = true;
                            check.IsChecked = true;
                            break;
                    }
                }
            }

            public StateEnum UpdateStateByValue() {
                var pre = State;
                return pre == StateEnum.Slave || pre == StateEnum.Invalid
                     ? pre
                     : (State = string.IsNullOrWhiteSpace(text.Text)
                       ? StateEnum.Void
                       : double.TryParse(text.Text, out _value)
                         ? StateEnum.Master
                         : StateEnum.Error);
            }

            public double Value {
                get => _value;
                set {
                    _value = value;
                    if (double.IsNaN(_value = value)) {
                        text.Text = "";
                    } else if (double.IsInfinity(value)) {
                        text.Text = "∞";
                    } else {
                        text.Text = value.ToString("0.###", CultureInfo.InvariantCulture);
                    }
                }
            }

            public Input Master { get; private set; } = null;

            public bool IsMaster => State == StateEnum.Master;

            public bool IsVoid => State == StateEnum.Void;

            public double? this[Input master] {
                set {
                    if (State == StateEnum.Error) return;
                    if (value.HasValue) {
                        Master = master;
                        State = double.IsNaN(value.Value)
                              ? StateEnum.Invalid
                              : StateEnum.Slave;
                        Value = value.Value;
                    } else if (Master == master) {
                        Master = null;
                        State = StateEnum.Void;
                        Value = double.NaN;
                    }
                }
            }

            private static readonly SolidColorBrush
                Normal = new SolidColorBrush(Colors.Black),
                Error = new SolidColorBrush(Colors.Red);
        }

        private readonly Input _v, _w, _r, _s, _a, _t;

        public ActionView() {
            InitializeComponent();
            _v = new Input(VCheck, VBox);
            _w = new Input(WCheck, WBox);
            _r = new Input(RCheck, RBox);
            _s = new Input(SCheck, SBox);
            _a = new Input(ACheck, ABox);
            _t = new Input(TCheck, TBox);
        }

        private bool Check(
            out double v,
            out double w,
            out bool timeBased,
            out double range) {
            v = _v.Value;
            w = _w.Value;
            timeBased = default;
            range = default;

            if (!_v.IsMaster && !_w.IsMaster)
                return false;

            if (_t.IsMaster) {
                timeBased = true;
                range = _t.Value;
                return true;
            } else if (_s.IsMaster || _a.IsMaster) {
                timeBased = false;
                range = SafeNativeMethods.SpatiumCalculate(_s.Value, _a.Value);
                return true;
            } else
                return false;
        }

        private void VBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            switch (_v.UpdateStateByValue()) {
                case Input.StateEnum.Master:
                    if (_v.Value == 0) {
                        switch (_s.State) {
                            case Input.StateEnum.Master:
                                if (_s.Value != 0) {
                                    _v.State = Input.StateEnum.Error;
                                    return;
                                }
                                _v[_s] = 0;
                                break;
                            case Input.StateEnum.Void:
                                _s[_v] = 0;
                                break;
                            case Input.StateEnum.Slave:
                            case Input.StateEnum.Invalid:
                            case Input.StateEnum.Error:
                                break;
                        }
                        if (_w.IsMaster)
                            _r[_v] = _v.Value / RadOf(_w.Value);
                        else switch (_r.State) {
                                case Input.StateEnum.Master:
                                    if (_r.Value != 0) {
                                        _v.State = Input.StateEnum.Error;
                                        return;
                                    }
                                    _r[_v] = 0;
                                    break;
                                case Input.StateEnum.Void:
                                    _r[_v] = 0;
                                    break;
                                case Input.StateEnum.Slave:
                                case Input.StateEnum.Invalid:
                                case Input.StateEnum.Error:
                                    break;
                            }
                    } else {
                        switch (_s.State) {
                            case Input.StateEnum.Master:
                                if (_s.Value == 0) {
                                    _v.State = Input.StateEnum.Error;
                                    return;
                                }
                                break;
                            case Input.StateEnum.Slave:
                                _s[_v] = null;
                                break;
                            case Input.StateEnum.Void:
                            case Input.StateEnum.Invalid:
                            case Input.StateEnum.Error:
                                break;
                        }
                        if (_w.IsMaster) {
                            _r[_v] = _v.Value / RadOf(_w.Value);
                            if (_s.IsMaster && !_a.IsVoid)
                                _a[_r] = DegreeOf(_s.Value / _r.Value);
                            else if (_a.IsMaster && _s.IsVoid)
                                _s[_r] = DegreeOf(_a.Value * _r.Value);
                        } else if (_r.IsMaster || _r.Master == _s)
                            _w[_r] = DegreeOf(_v.Value / _r.Value);
                    }
                    break;
                case Input.StateEnum.Void:
                    _r[_v] = 
                    _w[_r] = 
                    _s[_v] = 
                    _s[_r] = null;
                    break;
            }
            CheckButton.IsEnabled = Check(out _, out _, out _, out _);
        }

        private void WBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            switch (_w.UpdateStateByValue()) {
                case Input.StateEnum.Master:
                    if (_w.Value == 0) {
                        switch (_a.State) {
                            case Input.StateEnum.Master:
                                if (_a.Value != 0) {
                                    _w.State = Input.StateEnum.Error;
                                    return;
                                }
                                _w[_a] = 0;
                                break;
                            case Input.StateEnum.Void:
                                _a[_w] = 0;
                                break;
                            case Input.StateEnum.Slave:
                            case Input.StateEnum.Invalid:
                            case Input.StateEnum.Error:
                                break;
                        }
                        if (_v.IsMaster)
                            _r[_v] = _v.Value / RadOf(_w.Value);
                        else switch (_r.State) {
                                case Input.StateEnum.Master:
                                    if (!double.IsInfinity(_r.Value)) {
                                        _w.State = Input.StateEnum.Error;
                                        return;
                                    }
                                    _r[_v] = double.PositiveInfinity;
                                    break;
                                case Input.StateEnum.Void:
                                    _r[_v] = double.PositiveInfinity;
                                    break;
                                case Input.StateEnum.Slave:
                                case Input.StateEnum.Invalid:
                                case Input.StateEnum.Error:
                                    break;
                            }
                    } else {
                        switch (_a.State) {
                            case Input.StateEnum.Master:
                                if (_a.Value == 0) {
                                    _w.State = Input.StateEnum.Error;
                                    return;
                                }
                                break;
                            case Input.StateEnum.Slave:
                                _a[_w] = null;
                                break;
                            case Input.StateEnum.Void:
                            case Input.StateEnum.Invalid:
                            case Input.StateEnum.Error:
                                break;
                        }
                        if (_v.IsMaster) {
                            _r[_v] = _v.Value / RadOf(_w.Value);
                            if (_s.IsMaster && _a.IsVoid)
                                _a[_r] = DegreeOf(_s.Value / _r.Value);
                            else if (_w.IsMaster && _v.IsVoid)
                                _s[_r] = DegreeOf(_a.Value * _r.Value);
                        } else if (_r.IsMaster || _r.Master == _s)
                            _v[_r] = RadOf(_w.Value) * _r.Value;
                    }
                    break;
                case Input.StateEnum.Void:
                    _r[_v] = 
                    _v[_r] = 
                    _a[_w] = 
                    _a[_r] = null;
                    break;
            }
            CheckButton.IsEnabled = Check(out _, out _, out _, out _);
        }

        private void RBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            switch (_r.UpdateStateByValue()) {
                case Input.StateEnum.Master:
                    if (_r.Value == 0) {
                        switch (_v.State) {
                            case Input.StateEnum.Master:
                                if (_v.Value != 0) {
                                    _r.State = Input.StateEnum.Error;
                                    return;
                                }
                                _r[_v] = 0;
                                break;
                            case Input.StateEnum.Void:
                                _v[_r] = 0;
                                break;
                            case Input.StateEnum.Slave:
                            case Input.StateEnum.Invalid:
                            case Input.StateEnum.Error:
                                break;
                        }
                        switch (_s.State) {
                            case Input.StateEnum.Master:
                                if (_s.Value != 0) {
                                    _r.State = Input.StateEnum.Error;
                                    return;
                                }
                                _r[_s] = 0;
                                break;
                            case Input.StateEnum.Void:
                                _s[_r] = 0;
                                break;
                            case Input.StateEnum.Slave:
                            case Input.StateEnum.Invalid:
                            case Input.StateEnum.Error:
                                break;
                        }
                    } else {
                        if (_v.IsMaster)
                            _w[_r] = DegreeOf(_v.Value / _r.Value);
                        else if (_w.IsMaster)
                            _v[_r] = RadOf(_w.Value) * _r.Value;

                        if (_s.IsMaster)
                            _a[_r] = DegreeOf(_s.Value) / _r.Value;
                        else if (_a.IsMaster)
                            _s[_r] = RadOf(_a.Value) * _r.Value;
                    }
                    break;
                case Input.StateEnum.Void:
                    _v[_r] = 
                    _w[_r] = 
                    _s[_r] = 
                    _a[_r] = null;
                    break;
            }
            CheckButton.IsEnabled = Check(out _, out _, out _, out _);
        }

        private void SBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            switch (_s.UpdateStateByValue()) {
                case Input.StateEnum.Master:
                    if (_t.IsMaster) {
                        _s.State = Input.StateEnum.Error;
                        return;
                    }
                    _t[_s] = double.NaN;
                    if (_s.Value == 0) {
                        switch (_v.State) {
                            case Input.StateEnum.Master:
                                if (_v.Value != 0) {
                                    _s.State = Input.StateEnum.Error;
                                    return;
                                }
                                _s[_v] = 0;
                                break;
                            case Input.StateEnum.Void:
                                _v[_s] = 0;
                                break;
                            case Input.StateEnum.Slave:
                            case Input.StateEnum.Invalid:
                            case Input.StateEnum.Error:
                                break;
                        }
                        if (_a.IsMaster)
                            _r[_s] = _s.Value / RadOf(_a.Value);
                        else switch (_r.State) {
                                case Input.StateEnum.Master:
                                    if (_r.Value != 0) {
                                        _s.State = Input.StateEnum.Error;
                                        return;
                                    }
                                    _r[_s] = 0;
                                    break;
                                case Input.StateEnum.Void:
                                    _r[_s] = 0;
                                    break;
                                case Input.StateEnum.Slave:
                                case Input.StateEnum.Invalid:
                                case Input.StateEnum.Error:
                                    break;
                            }
                    } else {
                        switch (_v.State) {
                            case Input.StateEnum.Master:
                                if (_v.Value == 0) {
                                    _s.State = Input.StateEnum.Error;
                                    return;
                                }
                                break;
                            case Input.StateEnum.Slave:
                                _v[_s] = null;
                                break;
                            case Input.StateEnum.Void:
                            case Input.StateEnum.Invalid:
                            case Input.StateEnum.Error:
                                break;
                        }
                        if (_a.IsMaster) {
                            _r[_s] = _s.Value / RadOf(_a.Value);
                            if (_v.IsMaster && _w.IsVoid)
                                _w[_r] = DegreeOf(_v.Value / _r.Value);
                            else if (_w.IsMaster && _v.IsVoid)
                                _v[_r] = DegreeOf(_w.Value * _r.Value);
                        } else if (_r.IsMaster || _r.Master == _v)
                            _a[_r] = DegreeOf(_s.Value / _r.Value);
                    }
                    break;
                case Input.StateEnum.Void:
                    _t[_s] =
                    _r[_s] =
                    _a[_r] =
                    _v[_s] =
                    _v[_r] = null;
                    break;
            }
            CheckButton.IsEnabled = Check(out _, out _, out _, out _);
        }

        private void ABox_TextChanged(object sender, TextChangedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            switch (_a.UpdateStateByValue()) {
                case Input.StateEnum.Master:
                    if(_t.IsMaster) {
                        _a.State = Input.StateEnum.Error;
                        return;
                    }
                    _t[_a] = double.NaN;
                    if (_a.Value == 0) {
                        switch (_w.State) {
                            case Input.StateEnum.Master:
                                if (_w.Value != 0) {
                                    _a.State = Input.StateEnum.Error;
                                    return;
                                }
                                _a[_w] = 0;
                                break;
                            case Input.StateEnum.Void:
                                _w[_a] = 0;
                                break;
                            case Input.StateEnum.Slave:
                            case Input.StateEnum.Invalid:
                            case Input.StateEnum.Error:
                                break;
                        }
                        if (_s.IsMaster)
                            _r[_s] = _s.Value / RadOf(_a.Value);
                        else switch (_r.State) {
                                case Input.StateEnum.Master:
                                    if (_r.Value != 0) {
                                        _a.State = Input.StateEnum.Error;
                                        return;
                                    }
                                    _r[_s] = double.PositiveInfinity;
                                    break;
                                case Input.StateEnum.Void:
                                    _r[_s] = double.PositiveInfinity;
                                    break;
                                case Input.StateEnum.Slave:
                                case Input.StateEnum.Invalid:
                                case Input.StateEnum.Error:
                                    break;
                            }
                    } else {
                        switch (_w.State) {
                            case Input.StateEnum.Master:
                                if (_w.Value == 0) {
                                    _a.State = Input.StateEnum.Error;
                                    return;
                                }
                                break;
                            case Input.StateEnum.Slave:
                                _w[_a] = null;
                                break;
                            case Input.StateEnum.Void:
                            case Input.StateEnum.Invalid:
                            case Input.StateEnum.Error:
                                break;
                        }
                        if (_s.IsMaster) {
                            _r[_s] = _s.Value / RadOf(_a.Value);
                            if (_v.IsMaster && _w.IsVoid)
                                _w[_r] = DegreeOf(_v.Value / _r.Value);
                            else if (_a.IsMaster && _s.IsVoid)
                                _v[_r] = DegreeOf(_w.Value * _r.Value);
                        } else if (_r.IsMaster || _r.Master == _v)
                            _s[_r] = RadOf(_a.Value) * _r.Value;
                    }
                    break;
                case Input.StateEnum.Void:
                    _t[_a] = 
                    _r[_s] = 
                    _s[_r] = 
                    _w[_a] = 
                    _w[_r] = null;
                    break;
            }
            CheckButton.IsEnabled = Check(out _, out _, out _, out _);
        }

        private void TBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            switch (_t.UpdateStateByValue()) {
                case Input.StateEnum.Master:
                    if (_t.Value <= 0) {
                        _t.State = Input.StateEnum.Error;
                    } else {
                        switch (_s.State) {
                            case Input.StateEnum.Master:
                                _t.State = Input.StateEnum.Error;
                                return;
                            case Input.StateEnum.Void:
                            case Input.StateEnum.Slave:
                            case Input.StateEnum.Invalid:
                                _s[_t] = double.NaN;
                                break;
                            case Input.StateEnum.Error:
                                break;
                        }
                        switch (_a.State) {
                            case Input.StateEnum.Master:
                                _t.State = Input.StateEnum.Error;
                                return;
                            case Input.StateEnum.Void:
                            case Input.StateEnum.Slave:
                            case Input.StateEnum.Invalid:
                                _a[_t] = double.NaN;
                                break;
                            case Input.StateEnum.Error:
                                break;
                        }
                    }
                    break;
                case Input.StateEnum.Void:
                    _s[_t] = 
                    _a[_t] = null;
                    break;
            }
            CheckButton.IsEnabled = Check(out _, out _, out _, out _);
        }

        private static double RadOf(double degree) => degree * Math.PI / 180;
        private static double DegreeOf(double rad) => rad * 180 / Math.PI;

        private void VCheck_Unchecked(object sender, System.Windows.RoutedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            VBox.Text = "";
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e) {

        }

        private void WCheck_Unchecked(object sender, System.Windows.RoutedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            WBox.Text = "";
        }

        private void RCheck_Unchecked(object sender, System.Windows.RoutedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            RBox.Text = "";
        }

        private void SCheck_Unchecked(object sender, System.Windows.RoutedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            SBox.Text = "";
        }

        private void ACheck_Unchecked(object sender, System.Windows.RoutedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            ABox.Text = "";
        }

        private void TCheck_Unchecked(object sender, System.Windows.RoutedEventArgs e) {
            if (!((Control)sender).IsEnabled) return;
            TBox.Text = "";
        }
    }
}
