namespace Autolabor.PM1.TestTool.MainWindowItems.DriveVelocityTab {
    internal class TabContext : BindableBase {

        private double _v, _w;

        public double V {
            get => _v;
            set => SetProperty(ref _v, value);
        }

        public double W {
            get => _w;
            set => SetProperty(ref _w, value);
        }
    }
}
