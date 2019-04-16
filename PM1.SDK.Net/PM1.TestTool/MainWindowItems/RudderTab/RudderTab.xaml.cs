using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Autolabor.PM1.TestTool.MainWindowItems.RudderTab {
    /// <summary>
    /// RudderTab.xaml 的交互逻辑
    /// </summary>
    public partial class RudderTab : UserControl, ITabControl {
        private const string StartText = "已进入后轮零位校准流程，请将机器人移动到合适的位置，确保前方有至少 1.5 米的空旷区域。点击下面的按钮开始校准。校准开始后，机器人将按照内部里程计前进 1.5 米，若后轮零位不准，此动作会发生侧偏。待机器人停止后，请将侧偏的情况反馈给机器人，因此建议标记机器人开始时的方向，以便反馈正确的侧偏。";
        private const string AdjustText = "动作完成。现在点击下面的按钮反馈机器人侧偏情况。点击按钮后，机器人将后退到原位，并尝试调整后轮零位。";
        private const string RestartText = "调整完成，点击按钮开始下一次测试。";

        private TabContext _tabContext;
        private MainWindowContext _windowContext;
        private double _delta;
        private bool? _left;

        public RudderTab() => InitializeComponent();

        public void OnEnter() {
            Methods.Paused = false;
            _tabContext.State = State.Initialization;
            _tabContext.Progress = 0;
        }

        public void OnLeave() { }

        private void Tab_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) 
            => _windowContext = e.NewValue as MainWindowContext;

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) 
            => _tabContext = e.NewValue as TabContext;

        private void FakeProgress()
            => _tabContext.Progress += Math.Min(Math.Abs(2 * _delta), (0.9 - _tabContext.Progress) / 3);

        private void StartButton_Click(object sender, RoutedEventArgs e) {
            _left = null;

            _tabContext.State = State.Ready;
            _tabContext.HelpText = StartText;
            _tabContext.Progress = 0.1;
        }

        private void GoButton_Click(object sender, RoutedEventArgs e) {
            _tabContext.State = State.Forward;

            var flag = true;
            var progress = .0;
            _ = Task.Run(async () => {
                while (flag) {
                    _windowContext.Progress = progress;
                    await Task.Delay(20).ConfigureAwait(false);
                }
                await Task.Delay(100).ConfigureAwait(false);
                _windowContext.Progress = progress;
            });
            _ = Task.Run(() => {
                try {
                    //Methods.DriveSpatial(0.15, 0, 2 * 1.5, out progress);
                    _tabContext.HelpText = AdjustText;
                } catch (Exception exception) {
                    _windowContext.ErrorInfo = exception.Message;
                    return;
                } finally {
                    _tabContext.State = State.Feedback;
                    flag = false;
                }
            });
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e) {
            _tabContext.State = State.Backword;

            var flag = true;
            var progress = .0;
            _ = Task.Run(async () => {
                while (flag) {
                    _windowContext.Progress = progress;
                    await Task.Delay(20).ConfigureAwait(false);
                }
                await Task.Delay(100).ConfigureAwait(false);
                _windowContext.Progress = progress;
            });

            _ = Task.Run(() => {
                try {
                    //Methods.DriveSpatial(-0.15, 0, 2 * 1.5, out progress);
                } catch (Exception exception) {
                    _windowContext.ErrorInfo = exception.Message;
                    return;
                } finally {
                    _tabContext.State = State.Initialization;
                    flag = false;
                }
            });

            _tabContext.Progress = 1;
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e) {
            _tabContext.State = State.Backword;

            var flag = true;
            var progress = .0;
            _ = Task.Run(async () => {
                while (flag) {
                    _windowContext.Progress = progress;
                    await Task.Delay(20).ConfigureAwait(false);
                }
                await Task.Delay(100).ConfigureAwait(false);
                _windowContext.Progress = progress;
            });

            switch (_left) {
                case null:
                    _delta = 0.1;
                    break;
                case false:
                    _delta /= -2;
                    break;
            }
            _left = true;

            _ = Task.Run(() => {
                try {
                    //Methods.DriveSpatial(-0.15, 0, 2 * 1.5, out progress);
                    //Methods.AdjustRudder(_delta, out progress);
                    _tabContext.HelpText = RestartText;
                    FakeProgress();
                } catch (Exception exception) {
                    _windowContext.ErrorInfo = exception.Message;
                    return;
                } finally {
                    _tabContext.State = State.Ready;
                    flag = false;
                }
            });
        }

        private void RightButton_Click(object sender, RoutedEventArgs e) {
            _tabContext.State = State.Backword;

            var flag = true;
            var progress = .0;
            _ = Task.Run(async () => {
                while (flag) {
                    _windowContext.Progress = progress;
                    await Task.Delay(20).ConfigureAwait(false);
                }
                await Task.Delay(100).ConfigureAwait(false);
                _windowContext.Progress = progress;
            });

            switch (_left) {
                case null:
                    _delta = -0.1;
                    break;
                case true:
                    _delta /= -2;
                    break;
            }
            _left = false;

            _ = Task.Run(() => {
                try {
                    //Methods.DriveSpatial(-0.15, 0, 2 * 1.5, out progress);
                    //Methods.AdjustRudder(_delta, out progress);
                    _tabContext.HelpText = RestartText;
                    FakeProgress();
                } catch (Exception exception) {
                    _windowContext.ErrorInfo = exception.Message;
                    return;
                } finally {
                    _tabContext.State = State.Ready;
                    flag = false;
                }
            });
        }
    }
}
