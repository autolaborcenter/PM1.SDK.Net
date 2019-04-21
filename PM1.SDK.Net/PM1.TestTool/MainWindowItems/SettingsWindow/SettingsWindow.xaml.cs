using System.Windows;

namespace Autolabor.PM1.TestTool.MainWindowItems.SettingsWindow {
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            InitializeComponent();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
            if (sender == LBox) {
                HelpText.Text = "机器人的长度定义为机器人动力轮轴到转向轮转动中心的距离，这个值只能通过测量得到。";

            } else if (sender == WBox) {
                HelpText.Text = "机器人的宽度定义为机器人两动力轮之间的距离，可以通过标定得到，参见主界面“参数标定”选项卡。";

            } else if (sender == DBox) {
                HelpText.Text = "这个量是转向轮的轮胎直径，可以通过标定得到，参见主界面“参数标定”选项卡。";

            } 
        }
    }
}
