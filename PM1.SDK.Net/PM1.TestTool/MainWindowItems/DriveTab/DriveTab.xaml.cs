using System.Windows.Controls;

namespace Autolabor.PM1.TestTool.MainWindowItems.DriveTab {
    /// <summary>
    /// DriveTab.xaml 的交互逻辑
    /// </summary>
    public partial class DriveTab : UserControl, ITabControl {
        private TabContext _tabContext;

        public DriveTab() => InitializeComponent();

        public void OnEnter() { }

        public void OnLeave() { }

        private void Grid_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
            => _tabContext = e.NewValue as TabContext;
    }
}
