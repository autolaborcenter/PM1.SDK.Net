using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM1.TestTool
{
    public class MainWindowContext : BindableBase
    {
        private bool _connected = false;

        private string _timeString = "0";

        private double _progress = 0;

        public bool Connected {
            get => _connected;
            set {
                if (Equals(_connected, value)) return;
                _connected = value;
                Notify(nameof(Connected));
                Notify(nameof(FunctionEnabled));
            }
        }

        public bool FunctionEnabled => !_connected;

        public string TimeString {
            get => _timeString;
            set => SetProperty(ref _timeString, value);
        }

        public double Progress {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }
    }
}
