using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
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
                            check.IsEnabled = true;
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

            public bool IsVoid => State == StateEnum.Void;

            public bool IsError => State == StateEnum.Error;

            private static readonly SolidColorBrush
                Normal = new SolidColorBrush(Colors.Black),
                Error = new SolidColorBrush(Colors.Red);
        }

        private readonly Input _v, _w, _r, _s, _a, _t;
        private readonly IReadOnlyList<Input> _inputs;
        private readonly IReadOnlyDictionary<Input, Input> _signPairs;

        public delegate void OnCompletedHandler(double v, double w, bool timeBased, double range);
        public event OnCompletedHandler OnCompleted;

        public ActionView() {
            InitializeComponent();
            _v = new Input(VCheck, VBox);
            _w = new Input(WCheck, WBox);
            _r = new Input(RCheck, RBox);
            _s = new Input(SCheck, SBox);
            _a = new Input(ACheck, ABox);
            _t = new Input(TCheck, TBox);
            _inputs = new List<Input> { _v, _w, _r, _s, _a, _t };
            _signPairs = new Dictionary<Input, Input> {
                { _v, _s },
                { _s, _v },
                { _w, _a },
                { _a, _w } };
        }

        public void Reset() {
            foreach (var input in _inputs) {
                input.Value = double.NaN;
                input.State = Input.StateEnum.Void;
            }
        }

        private void Calculate(Input theOne) {
            theOne.UpdateStateByValue();

            CoverError();
            if (theOne.IsMaster && !CheckMaster(theOne)) {
                theOne.State = Input.StateEnum.Error;
                return;
            }

            var waitings = _inputs.Where(it => !it.IsMaster && !it.IsError).ToList();
            var complete = _inputs.Where(it => it.IsMaster).ToList();

            while (waitings.Any()
                && waitings.RemoveAll(it => CalculateItem(it, complete)) > 0) ;
            foreach (var input in waitings)
                input.State = Input.StateEnum.Void;
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
            CheckButton.IsEnabled = _inputs.Where(it => it.IsMaster).Count() == 3;
        }

        private void Checked(object sender, RoutedEventArgs e) {
            var input = (Control)sender;
            if (!input.IsEnabled) return;
            switch ((string)input.Tag) {
                case nameof(_v):
                    if (_v.IsVoid) VBox.Text = "0.2";
                    break;
                case nameof(_w):
                    if (_w.IsVoid) WBox.Text = "20";
                    break;
                case nameof(_r):
                    if (_r.IsVoid) RBox.Text = "0.5";
                    break;
                case nameof(_s):
                    if (_s.IsVoid) SBox.Text = "1";
                    break;
                case nameof(_a):
                    if (_a.IsVoid) ABox.Text = "90";
                    break;
                case nameof(_t):
                    if (_t.IsVoid) TBox.Text = "5";
                    break;
            }
        }

        private void Unchecked(object sender, RoutedEventArgs e) {
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

        private void CoverError() {
            var count = 0;
            while (true) {
                var errors = _inputs.Where(it => it.IsError && !double.IsNaN(it.Value)).ToList();
                if (errors.Count == count) return;
                count = errors.Count;
                foreach (var repaired in errors.Where(CheckMaster))
                    repaired.State = Input.StateEnum.Master;
            }
        }

        private bool CheckMaster(Input theOne) 
            => CheckSign(theOne) && CheckNotZero(theOne);

        /// <summary>
        ///     检查“线速度-路程”和“角速度-角度”两对参数符号一致
        /// </summary>
        /// <param name="theOne">新设置的参数</param>
        /// <returns>检查是否通过</returns>
        private bool CheckSign(Input theOne) 
            => !_signPairs.TryGetValue(theOne, out var others)
            || !others.IsMaster
            || Math.Sign(others.Value) == Math.Sign(theOne.Value);

        private bool CheckNotZero(Input theOne) {
            bool ZeroCheck(Input it)
                => it.IsMaster && it.Value == 0;

            bool Recover(bool others) {
                if (theOne.Value == 0) {
                    if (others) {
                        theOne.State = Input.StateEnum.Slave;
                        return true;
                    } else
                        return false;
                } else return !others;
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
            else
                return true;
        }

        private bool CalculateItem(Input input, List<Input> complete) {
            double RadOf(double degree) => degree * Math.PI / 180;
            double DegreeOf(double rad) => rad * 180 / Math.PI;

            if (input == _v) {
                if ((complete.Contains(_s) && _s.Value == 0)
                 || (complete.Contains(_r) && _r.Value == 0)) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = 0;
                    complete.Add(input);
                    return true;
                } else if (complete.Contains(_w)
                        && complete.Contains(_r)
                        && _w.Value != 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = RadOf(_w.Value) * _r.Value; ;
                    complete.Add(input);
                    return true;
                }
            } else if (input == _w) {
                if (complete.Contains(_a) && _a.Value == 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = 0;
                    complete.Add(input);
                    return true;
                } else if (complete.Contains(_v)
                        && complete.Contains(_r)
                        && _v.Value != 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = DegreeOf(_v.Value / _r.Value);
                    complete.Add(input);
                    return true;
                }
            } else if (input == _r) {
                if ((complete.Contains(_v) && _v.Value == 0)
                 || (complete.Contains(_s) && _s.Value == 0)) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = 0;
                    complete.Add(input);
                    return true;
                } else if (complete.Contains(_w) && _w.Value == 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = double.PositiveInfinity;
                    complete.Add(input);
                    return true;
                } else if (complete.Contains(_v) && complete.Contains(_w)) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = _v.Value / RadOf(_w.Value);
                    complete.Add(input);
                    return true;
                } else if (complete.Contains(_s) && complete.Contains(_a)) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = _s.Value / RadOf(_a.Value);
                    complete.Add(input);
                    return true;
                }
            } else if (input == _s) {
                if (complete.Contains(_t)) {
                    input.State = Input.StateEnum.Invalid;
                    return true;
                } else if ((complete.Contains(_v) && _v.Value == 0)
                        || (complete.Contains(_r) && _r.Value == 0)) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = 0;
                    complete.Add(input);
                    return true;
                } else if (complete.Contains(_a)
                        && complete.Contains(_r)
                        && _a.Value != 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = RadOf(_a.Value) * _r.Value; ;
                    complete.Add(input);
                    return true;
                }
            } else if (input == _a) {
                if (complete.Contains(_t)) {
                    input.State = Input.StateEnum.Invalid;
                    return true;
                } else if (complete.Contains(_w) && _w.Value == 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = 0;
                    complete.Add(input);
                    return true;
                } else if (complete.Contains(_s)
                        && complete.Contains(_r)
                        && _s.Value != 0) {
                    input.State = Input.StateEnum.Slave;
                    input.Value = DegreeOf(_s.Value / _r.Value);
                    complete.Add(input);
                    return true;
                }
            } else if (input == _t) {
                if (_s.IsMaster || _a.IsMaster) {
                    input.State = Input.StateEnum.Invalid;
                    return true;
                }
            }
            return false;
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e) {
            double RadOf(double degree) => degree * Math.PI / 180;
            var timeBased = _t.IsMaster;
            OnCompleted?.Invoke(_v.Value,
                                RadOf(_w.Value),
                                timeBased,
                                timeBased ? _t.Value
                                          : SafeNativeMethods.SpatiumCalculate(Math.Abs(_s.Value), Math.Abs(RadOf(_a.Value))));
            Reset();
        }
    }
}
