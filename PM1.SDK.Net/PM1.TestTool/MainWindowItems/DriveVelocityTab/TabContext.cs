namespace Autolabor.PM1.TestTool.MainWindowItems.DriveVelocityTab {
    internal class TabContext : BindableBase {

        private double 
            _v, _w, 
            _vRange = 0.25,
            _wRange = 0.25;

        public double V {
            get => _v;
            set => SetProperty(ref _v, value);
        }

        public double W {
            get => _w;
            set => SetProperty(ref _w, value);
        }

        public double VRange {
            get => _vRange;
            set => SetProperty(ref _vRange, value);
        }

        public double WRange {
            get => _wRange;
            set => SetProperty(ref _wRange, value);
        }
    }
}
