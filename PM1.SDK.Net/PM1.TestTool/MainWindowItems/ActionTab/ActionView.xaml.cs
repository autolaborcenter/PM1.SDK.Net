using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            private double _value = double.NaN;

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
                            Value = double.NaN;
                            text.Foreground = Normal;
                            text.IsEnabled = true;
                            check.IsEnabled = false;
                            check.IsChecked = false;
                            break;
                        case StateEnum.Invalid:
                            Value = double.NaN;
                            text.Foreground = Normal;
                            text.IsEnabled =
                            check.IsEnabled = false;
                            check.IsChecked = false;
                            break;
                        case StateEnum.Error:
                            _value = double.NaN;
                            text.Foreground = Error;
                            text.IsEnabled =
                            check.IsEnabled = true;
                            check.IsChecked = true;
                            break;
                    }
                }
            }

            public void UpdateStateByValue() {
                var pre = State;
                if (pre != StateEnum.Slave && pre != StateEnum.Invalid)
                    State = string.IsNullOrWhiteSpace(text.Text)
                            ? StateEnum.Void
                            : double.TryParse(text.Text, out _value)
                              ? StateEnum.Master
                              : StateEnum.Error;
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

            public bool IsMaster => State == StateEnum.Master;

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

        private bool CheckNotZero(Input theOne) {
            if (theOne != _t && theOne.IsMaster) {
                bool ZeroCheck(Input it)
                    => it.IsMaster && it.Value == 0;

                bool Recover(bool others) {
                    if (theOne.Value == 0) {
                        if (others)
                            theOne.State = Input.StateEnum.Slave;
                    } else {
                        if (others) {
                            theOne.State = Input.StateEnum.Error;
                            return false;
                        }
                    }
                    return true;
                }

                if (theOne == _v)
                    return Recover(ZeroCheck(_s) || ZeroCheck(_r));
                else if (theOne == _w)
                    return Recover(ZeroCheck(_a));
                else if (theOne == _r)
                    return Recover(ZeroCheck(_v) || ZeroCheck(_s));
                else if (theOne == _s)
                    return Recover(ZeroCheck(_v) || ZeroCheck(_r));
                else if (theOne == _a)
                    return Recover(ZeroCheck(_w));
            }
            return true;
        }

        private bool CalculateItem(Input input,List<Input> complete) {
            double RadOf(double degree) => degree * Math.PI / 180;
            double DegreeOf(double rad) => rad * 180 / Math.PI;

            if (input == _v) {
                if (complete.Contains(_s) && _s.Value == 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = 0;
                    complete.Add(input);
                } else if (complete.Contains(_w) 
                        && complete.Contains(_r)
                        && _w.Value != 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = RadOf(_w.Value) * _r.Value; ;
                    complete.Add(input);
                }
            } else if (input == _w) {
                if (complete.Contains(_a) && _a.Value == 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = 0;
                    complete.Add(input);
                } else if (complete.Contains(_v) 
                        && complete.Contains(_r)
                        && _v.Value != 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = DegreeOf(_v.Value / _r.Value);
                    complete.Add(input);
                }
            } else if (input == _r) {
                if (complete.Contains(_v) && _v.Value == 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = 0;
                    complete.Add(input);
                } else if (complete.Contains(_w) && _w.Value == 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = double.PositiveInfinity;
                    complete.Add(input);
                } else if (complete.Contains(_v) && complete.Contains(_w)) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = _v.Value / RadOf(_w.Value);
                    complete.Add(input);
                } else if (complete.Contains(_s) && complete.Contains(_a)) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = _s.Value / RadOf(_a.Value);
                    complete.Add(input);
                }
            } else if (input == _s) {
                if (complete.Contains(_t)) {
                    input.State = Input.StateEnum.Invalid;
                    return false;
                } else if (complete.Contains(_v) && _v.Value == 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = 0;
                    complete.Add(input);
                } else if (complete.Contains(_a) 
                        && complete.Contains(_r)
                        && _a.Value != 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = RadOf(_a.Value) * _r.Value; ;
                    complete.Add(input);
                }
            } else if (input == _a) {
                if (complete.Contains(_t)) {
                    input.State = Input.StateEnum.Invalid;
                    return false;
                } else if (complete.Contains(_w) && _w.Value == 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = 0;
                    complete.Add(input);
                } else if (complete.Contains(_s) 
                        && complete.Contains(_r)
                        && _s.Value != 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = DegreeOf(_s.Value / _r.Value);
                    complete.Add(input);
                }
            } else if (input == _t) {
                if (_s.IsMaster || _a.IsMaster) {
                    input.State = Input.StateEnum.Invalid;
                    return false;
                }
            }
            return true;
        }

        private void Calculate(Input theOne) {
            theOne.UpdateStateByValue();

            if (!CheckNotZero(theOne)) return;

            var waitings = new List<Input> { _v, _w, _r, _s, _a, _t };
            var complete = waitings.Where(it => it.IsMaster).ToList();
            waitings.RemoveAll(it => complete.Contains(it) || it.State == Input.StateEnum.Error);

            while (waitings.Any()) {
                var droping = new List<Input>();
                foreach (var input in waitings) {
                    if (!CalculateItem(input, complete))
                        droping.Add(input);
                }
                var x = waitings.RemoveAll(it => droping.Contains(it));
                var y = waitings.RemoveAll(it => complete.Contains(it));
                if (x + y == 0) break;
            }
            foreach(var input in waitings) 
                input.State = Input.StateEnum.Void;
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

        private void TextChanged(object sender, TextChangedEventArgs e) {
            var input = (Control)sender;
            if (!input.IsEnabled) return;
            switch ((string)input.Tag) {
                case nameof(_v):
                    Calculate(_v);
                    break;
                case nameof(_w):
                    Calculate(_w);
                    break;
                case nameof(_r):
                    Calculate(_r);
                    break;
                case nameof(_s):
                    Calculate(_s);
                    break;
                case nameof(_a):
                    Calculate(_a);
                    break;
                case nameof(_t):
                    Calculate(_t);
                    break;
            }
            CheckButton.IsEnabled = Check(out _, out _, out _, out _);
        }

        private void Unchecked(object sender, System.Windows.RoutedEventArgs e) {
            var input = (Control)sender;
            if (!input.IsEnabled) return;
            switch ((string)input.Tag) {
                case nameof(_v):
                    VBox.Text = "";
                    break;
                case nameof(_w):
                    WBox.Text = "";
                    break;
                case nameof(_r):
                    RBox.Text = "";
                    break;
                case nameof(_s):
                    SBox.Text = "";
                    break;
                case nameof(_a):
                    ABox.Text = "";
                    break;
                case nameof(_t):
                    TBox.Text = "";
                    break;
            }
        }
    }
}
