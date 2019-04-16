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

        /// <summary>
        ///     输入点
        /// </summary>
        private class Input {
            private readonly CheckBox _check;
            private readonly TextBox _text;
            private readonly double _minium, _maxium;

            public Input(CheckBox check,
                         TextBox text,
                         double minium = double.NegativeInfinity,
                         double maxium = double.PositiveInfinity) {
                _check = check;
                _text = text;
                _minium = minium;
                _maxium = maxium;
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
                            _text.Foreground = Normal;
                            _text.IsEnabled =
                            _check.IsEnabled = true;
                            _check.IsChecked = true;
                            break;
                        case StateEnum.Slave:
                            _text.Foreground = Normal;
                            _text.IsEnabled =
                            _check.IsEnabled = false;
                            _check.IsChecked = true;
                            break;
                        case StateEnum.Void:
                            Value = double.NaN;
                            _text.Foreground = Normal;
                            _text.IsEnabled = true;
                            _check.IsEnabled = true;
                            _check.IsChecked = false;
                            break;
                        case StateEnum.Invalid:
                            Value = double.NaN;
                            _text.Foreground = Normal;
                            _text.IsEnabled =
                            _check.IsEnabled = false;
                            _check.IsChecked = false;
                            break;
                        case StateEnum.Error:
                            _value = double.NaN;
                            _text.Foreground = Error;
                            _text.IsEnabled =
                            _check.IsEnabled = true;
                            _check.IsChecked = true;
                            break;
                    }
                }
            }

            public bool CheckRange()
                => !double.IsNaN(_value) && _minium <= _value && _value <= _maxium;

            public string UpdateStateByValue() {
                if (_state == StateEnum.Slave 
                 || _state == StateEnum.Invalid)
                    return "";

                if (string.IsNullOrWhiteSpace(_text.Text)) {
                    State = StateEnum.Void;
                    return "";
                }

                if (!double.TryParse(_text.Text, out _value)) {
                    State = StateEnum.Error;
                    return "词法错误";
                }

                if(_value < _minium || _maxium < _value) {
                    State = StateEnum.Error;
                    return "超出有效范围";
                }

                State = StateEnum.Master;
                return "";
            }

            public bool ClearError() {
                if (State == StateEnum.Error) {
                    State = StateEnum.Void;
                    return true;
                }
                return false;
            }

            public double Value {
                get => _value;
                set => _text.Text = double.IsNaN(_value = value)
                                    ? ""
                                    : double.IsInfinity(value)
                                      ? "∞"
                                      : value.ToString("0.###", CultureInfo.InvariantCulture);
            }

            public bool IsMaster => State == StateEnum.Master;

            public bool IsVoid => State == StateEnum.Void;

            public bool IsSlave => State == StateEnum.Slave;

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
            _v = new Input(VCheck, VBox, -2, 2);
            _w = new Input(WCheck, WBox, -120, 120);
            _r = new Input(RCheck, RBox);
            _s = new Input(SCheck, SBox);
            _a = new Input(ACheck, ABox);
            _t = new Input(TCheck, TBox, 0);
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
            var error = theOne.UpdateStateByValue();
            if (!string.IsNullOrWhiteSpace(error)) {
                ErrorInfo.Text = error;
                return;
            }

            if (theOne.IsMaster) {
                if (!CheckSign(theOne)) {
                    theOne.State = Input.StateEnum.Error;
                    ErrorInfo.Text = "与对应项符号不一致";
                    return;
                }
            }

            ErrorInfo.Text = "";

            var complete = _inputs.Where(it => it.IsMaster).ToList();
            var waitings = _inputs.Except(complete).ToList();

            while (waitings.Any()
                && waitings.RemoveAll(it => CalculateItem(it, complete)) > 0) ;
            foreach (var input in waitings)
                input.State = Input.StateEnum.Void;

            if (theOne.IsMaster) {
                foreach(var input in _inputs.Where(it => it.IsSlave)) {
                    if (!input.CheckRange()) {
                        theOne.State = Input.StateEnum.Error;
                        ErrorInfo.Text = "超出有效范围";
                        return;
                    }
                }
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e) {
            var input = (Control)sender;
            if (!input.IsEnabled) return;
            switch ((string)input.Tag) {
                case nameof(_v): Calculate(_v); break;
                case nameof(_w): Calculate(_w); break;
                case nameof(_r): Calculate(_r); break;
                case nameof(_s): Calculate(_s); break;
                case nameof(_a): Calculate(_a); break;
                case nameof(_t): Calculate(_t); break;
            }
            if(CheckButton.IsEnabled = _inputs.Where(it => it.IsMaster).Count() == 3)
                if (Math.Abs(_v.Value) > 1 || Math.Abs(_w.Value) > 60) 
                    ErrorInfo.Text = "速度较快，请谨慎操作";
        }

        private void Box_LostFocus(object sender, RoutedEventArgs e) {
            void Clear(Input one) {
                if (one.ClearError()) ErrorInfo.Text = "错误已清除";
            }

            var input = (Control)sender;
            if (!input.IsEnabled) return;
            switch ((string)input.Tag) {
                case nameof(_v): Clear(_v); break;
                case nameof(_w): Clear(_w); break;
                case nameof(_r): Clear(_r); break;
                case nameof(_s): Clear(_s); break;
                case nameof(_a): Clear(_a); break;
                case nameof(_t): Clear(_t); break;
            }
        }

        private void Checked(object sender, RoutedEventArgs e) {
            var input = (Control)sender;
            if (!input.IsEnabled) return;
            switch ((string)input.Tag) {
                case nameof(_v):
                    if (_v.IsVoid) _v.Value = 0.2;
                    break;
                case nameof(_w):
                    if (_w.IsVoid) _w.Value = 20;
                    break;
                case nameof(_r):
                    if (_r.IsVoid) _r.Value = 0.5;
                    break;
                case nameof(_s):
                    if (_s.IsVoid) _s.Value = 1;
                    break;
                case nameof(_a):
                    if (_a.IsVoid) _a.Value = 90;
                    break;
                case nameof(_t):
                    if (_t.IsVoid) _t.Value = 5;
                    break;
            }
        }

        private void Unchecked(object sender, RoutedEventArgs e) {
            var input = (Control)sender;
            if (!input.IsEnabled) return;
            switch ((string)input.Tag) {
                case nameof(_v): _v.State = Input.StateEnum.Void; break;
                case nameof(_w): _w.State = Input.StateEnum.Void; break;
                case nameof(_r): _r.State = Input.StateEnum.Void; break;
                case nameof(_s): _s.State = Input.StateEnum.Void; break;
                case nameof(_a): _a.State = Input.StateEnum.Void; break;
                case nameof(_t): _t.State = Input.StateEnum.Void; break;
            }
        }

        /// <summary>
        ///     检查“线速度-路程”和“角速度-角度”两对参数符号一致
        /// </summary>
        /// <param name="theOne">新设置的参数</param>
        /// <returns>检查是否通过</returns>
        private bool CheckSign(Input theOne)
            => !_signPairs.TryGetValue(theOne, out var others)
            || !others.IsMaster
            || Math.Sign(others.Value) == Math.Sign(theOne.Value);

        /// <summary>
        ///     计算从动属性
        /// </summary>
        /// <param name="input">待设定项</param>
        /// <param name="ranference">参考项</param>
        /// <returns>是否确定</returns>
        private bool CalculateItem(Input input, List<Input> ranference) {
            double RadOf(double degree) => degree * Math.PI / 180;
            double DegreeOf(double rad) => rad * 180 / Math.PI;
            void SetSlave(double value) {
                input.State = Input.StateEnum.Slave;
                input.Value = value;
                ranference.Add(input);
            }

            if (input == _v) {
                if ((ranference.Contains(_s) && _s.Value == 0)
                 || (ranference.Contains(_r) && _r.Value == 0)) {
                    SetSlave(0);
                    return true;
                } else if (ranference.Contains(_w)
                        && ranference.Contains(_r)
                        && _w.Value != 0) {
                    SetSlave(RadOf(_w.Value) * _r.Value);
                    return true;
                }
            } else if (input == _w) {
                if (ranference.Contains(_a) && _a.Value == 0) {
                    SetSlave(0);
                    return true;
                } else if (ranference.Contains(_v)
                        && ranference.Contains(_r)
                        && _v.Value != 0) {
                    SetSlave(DegreeOf(_v.Value / _r.Value));
                    return true;
                }
            } else if (input == _r) {
                if ((ranference.Contains(_v) && _v.Value == 0)
                 || (ranference.Contains(_s) && _s.Value == 0)) {
                    SetSlave(0);
                    return true;
                } else if (ranference.Contains(_w) && _w.Value == 0) {
                    SetSlave(double.PositiveInfinity);
                    return true;
                } else if (ranference.Contains(_v) && ranference.Contains(_w)) {
                    SetSlave(_v.Value / RadOf(_w.Value));
                    return true;
                } else if (ranference.Contains(_s) && ranference.Contains(_a)) {
                    SetSlave(_s.Value / RadOf(_a.Value));
                    return true;
                }
            } else if (input == _s) {
                if (ranference.Contains(_t)) {
                    input.State = Input.StateEnum.Invalid;
                    return true;
                } else if ((ranference.Contains(_v) && _v.Value == 0)
                        || (ranference.Contains(_r) && _r.Value == 0)) {
                    SetSlave(0);
                    return true;
                } else if (ranference.Contains(_a)
                        && ranference.Contains(_r)
                        && _a.Value != 0) {
                    SetSlave(RadOf(_a.Value) * _r.Value);
                    return true;
                }
            } else if (input == _a) {
                if (ranference.Contains(_t)) {
                    input.State = Input.StateEnum.Invalid;
                    return true;
                } else if (ranference.Contains(_w) && _w.Value == 0) {
                    SetSlave(0);
                    return true;
                } else if (ranference.Contains(_s)
                        && ranference.Contains(_r)
                        && _s.Value != 0) {
                    SetSlave(DegreeOf(_s.Value / _r.Value));
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
                                          : SafeNativeMethods.SpatiumCalculate(Math.Abs(_s.Value), 
                                                                               Math.Abs(RadOf(_a.Value))));
            Reset();
        }
    }
}
