using System.Windows;
using System.Windows.Controls;

namespace Autolabor.PM1.TestTool.MainWindowItems.SettingsWindow {
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window {
        private WindowContext _context;

        public SettingsWindow() => InitializeComponent();

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
            switch (((Control)sender).Tag) {
                case nameof(WindowContext.Length):
                    _context.HelpText = "机器人的长度定义为机器人动力轮轴到转向轮转动中心的距离，这个值只能通过测量得到。";
                    break;
                case nameof(WindowContext.Width):
                    _context.HelpText = "机器人的宽度定义为机器人两动力轮之间的距离，可以通过标定得到，参见主界面“参数标定”选项卡。";
                    break;
                case nameof(WindowContext.WheelRadius):
                    _context.HelpText = "这个量是转向轮的轮胎直径，可以通过标定得到，参见主界面“参数标定”选项卡。";
                    break;
                case nameof(WindowContext.OptimizeWidth):
                    _context.HelpText = "优化宽度表示对轮速优化的限度。若后轮当前转角与目标转角的偏差超过这个宽度，速度降为零。";
                    break;
                case nameof(WindowContext.Acceleration):
                    _context.HelpText = "这是轮速的最大加速度，值越大，启停越平稳，但停止时无法立刻停下，会向前滑行一段距离。";
                    break;
                case nameof(WindowContext.MaxV):
                    _context.HelpText = "最大线速度限制。";
                    break;
                case nameof(WindowContext.MaxW):
                    _context.HelpText = "最大角速度限制。";
                    break;
            }
        }

        private void Grid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) 
            => _context = e.NewValue as WindowContext;

        private void Revert_Click(object sender, RoutedEventArgs e)
            => _context.ResetParameters();
    }
}
