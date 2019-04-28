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
                            _text.Foreground = ToolFunctions.NormalBrush;
                            _text.IsEnabled =
                            _check.IsEnabled = true;
                            _check.IsChecked = true;
                            break;
                        case StateEnum.Slave:
                            _text.Foreground = ToolFunctions.NormalBrush;
                            _text.IsEnabled =
                            _check.IsEnabled = false;
                            _check.IsChecked = true;
                            break;
                        case StateEnum.Void:
                            Value = double.NaN;
                            _text.Foreground = ToolFunctions.NormalBrush;
                            _text.IsEnabled = true;
                            _check.IsEnabled = true;
                            _check.IsChecked = false;
                            break;
                        case StateEnum.Invalid:
                            Value = double.NaN;
                            _text.Foreground = ToolFunctions.NormalBrush;
                            _text.IsEnabled =
                            _check.IsEnabled = false;
                            _check.IsChecked = false;
                            break;
                        case StateEnum.Error:
                            _value = double.NaN;
                            _text.Foreground = ToolFunctions.ErrorBrush;
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

                if (_value < _minium || _maxium < _value) {
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

        /// <summary>
        ///     重置控件
        /// </summary>
        public void Reset() {
            foreach (var input in _inputs) {
                input.Value = double.NaN;
                input.State = Input.StateEnum.Void;
            }
        }

        /// <summary>
        ///     计算所有从动参数
        /// </summary>
        /// <param name="theOne">本次修改的一项</param>
        private void Calculate(Input theOne) {
            var error = theOne.UpdateStateByValue();
            if (!string.IsNullOrWhiteSpace(error)) {
                ErrorInfo.Text = error;
                return;
            }

            if (theOne.IsMaster) {
                var conflict = CheckSign(theOne);
                if (conflict != null) {
                    theOne.State = Input.StateEnum.Error;
                    ErrorInfo.Text = "速度设定与约束设定冲突";
                    if (conflict.IsMaster) return;
                }
            }

            var complete = _inputs.Where(it => it.IsMaster).ToList();
            var waitings = _inputs.Except(complete).Where(it => it.State != Input.StateEnum.Error).ToList();

            while (waitings.Any()
                && waitings.RemoveAll(it => CalculateItem(it, complete)) > 0) ;
            foreach (var input in waitings)
                input.State = Input.StateEnum.Void;

            if (theOne.IsMaster) {
                foreach (var input in new[] { _v, _w }.Where(it => it.State == Input.StateEnum.Slave))
                    if (!input.CheckRange()) {
                        theOne.State = Input.StateEnum.Error;
                        ErrorInfo.Text = "速度超出有效范围";
                        return;
                    }
            } else if (theOne.State == Input.StateEnum.Error && null == CheckSign(theOne))
                theOne.State = Input.StateEnum.Master;

            var voids = _inputs.Where(it => it.IsVoid).ToHashSet();
            if (voids.Count == _inputs.Count)
                ErrorInfo.Text = "至少输入一项速度和一项约束";
            else if (voids.Intersect(new[] { _v, _w }).Count() == 2)
                ErrorInfo.Text = "至少输入一项速度";
            else if (voids.Intersect(new[] { _s, _a, _t }).Count() == 3)
                ErrorInfo.Text = "至少输入一项约束";
            else if (_inputs.Where(it => it.IsMaster).Count() == 2)
                ErrorInfo.Text = _inputs.Any(it => it.IsVoid)
                                 ? "再输入一项以确定动作"
                                 : "无效的动作";
            else
                ErrorInfo.Text = "";
        }

        /// <summary>
        ///     修改参数时重新计算从动参数
        /// </summary>
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
            if (CheckButton.IsEnabled = _inputs.Where(it => it.IsMaster).Count() == 3)
                ErrorInfo.Text = Math.Abs(_v.Value) > 1 || Math.Abs(_w.Value) > 60
                    ? "速度较快，请谨慎操作"
                    : "点击确定时动作开始";
        }

        /// <summary>
        ///     失去焦点时清除异常
        /// </summary>
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

        /// <summary>
        ///     为一个主要参数提供默认设定值
        /// </summary>
        private void Checked(object sender, RoutedEventArgs e) {
            bool IsValid(Input one)
                => one.IsMaster && one.Value != 0;

            void Set(Input one, Input related, Input opposite0, Input opposite1, double target) {
                if (one.IsVoid) one.Value = !double.IsNaN(related.Value)
                                            ? Math.Sign(related.Value) * target
                                            : !IsValid(opposite0) && !IsValid(opposite1)
                                              ? target
                                              : 0;
            }

            var input = (Control)sender;
            if (!input.IsEnabled) return;
            switch ((string)input.Tag) {
                case nameof(_v):
                    Set(_v, _s, _w, _a, 0.2);
                    break;
                case nameof(_w):
                    Set(_w, _a, _v, _s, 20);
                    break;
                case nameof(_r):
                    if (_r.IsVoid) _r.Value = 0.5;
                    break;
                case nameof(_s):
                    Set(_s, _v, _w, _a, 1);
                    break;
                case nameof(_a):
                    Set(_a, _w, _v, _s, 90);
                    break;
                case nameof(_t):
                    if (_t.IsVoid) _t.Value = 5;
                    break;
            }
        }

        /// <summary>
        ///     取消一个主要参数设定
        /// </summary>
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
        private Input CheckSign(Input theOne)
            => !_signPairs.TryGetValue(theOne, out var others) 
            || double.IsNaN(others.Value)
            || Math.Sign(others.Value) == Math.Sign(theOne.Value)
               ? null
               : others;

        /// <summary>
        ///     计算从动属性
        /// </summary>
        /// <param name="input">待设定项</param>
        /// <param name="ranference">参考项</param>
        /// <returns>是否确定</returns>
        private bool CalculateItem(Input input, List<Input> ranference) {
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
                }
                else if (ranference.Contains(_w)
                      && ranference.Contains(_r)
                      && _w.Value != 0) {
                    SetSlave(_w.Value.ToRad() * _r.Value);
                    return true;
                }
            }
            else if (input == _w) {
                if (ranference.Contains(_a) && _a.Value == 0) {
                    SetSlave(0);
                    return true;
                }
                else if (ranference.Contains(_v)
                      && ranference.Contains(_r)
                      && _v.Value != 0) {
                    SetSlave((_v.Value / _r.Value).ToDegree());
                    return true;
                }
            }
            else if (input == _r) {
                if ((ranference.Contains(_v) && _v.Value == 0)
                 || (ranference.Contains(_s) && _s.Value == 0)) {
                    SetSlave(0);
                    return true;
                }
                else if (ranference.Contains(_w) && _w.Value == 0) {
                    SetSlave(double.PositiveInfinity);
                    return true;
                }
                else if (ranference.Contains(_v) && ranference.Contains(_w)) {
                    SetSlave(_v.Value / _w.Value.ToRad());
                    return true;
                }
                else if (ranference.Contains(_s) && ranference.Contains(_a)) {
                    SetSlave(_s.Value / _a.Value.ToRad());
                    return true;
                }
            }
            else if (input == _s) {
                if (ranference.Contains(_t)) {
                    input.State = Input.StateEnum.Invalid;
                    return true;
                }
                else if ((ranference.Contains(_v) && _v.Value == 0)
                      || (ranference.Contains(_r) && _r.Value == 0)) {
                    SetSlave(0);
                    return true;
                }
                else if (ranference.Contains(_a)
                      && ranference.Contains(_r)
                      && _a.Value != 0) {
                    SetSlave(_a.Value.ToRad() * _r.Value);
                    return true;
                }
            }
            else if (input == _a) {
                if (ranference.Contains(_t)) {
                    input.State = Input.StateEnum.Invalid;
                    return true;
                }
                else if (ranference.Contains(_w) && _w.Value == 0) {
                    SetSlave(0);
                    return true;
                }
                else if (ranference.Contains(_s)
                      && ranference.Contains(_r)
                      && _s.Value != 0) {
                    SetSlave((_s.Value / _r.Value).ToDegree());
                    return true;
                }
            }
            else if (input == _t) {
                if (_s.IsMaster || _a.IsMaster) {
                    input.State = Input.StateEnum.Invalid;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     确认按钮的点击事件
        /// </summary>
        private void CheckButton_Click(object sender, RoutedEventArgs e) {
            var timeBased = _t.IsMaster;
            OnCompleted?.Invoke(_v.Value,
                                _w.Value.ToRad(),
                                timeBased,
                                timeBased ? _t.Value
                                          : Methods.CalculateSpatium(Math.Abs(_s.Value),
                                                                     Math.Abs(_a.Value.ToRad())));
            Reset();
        }
    }
}
