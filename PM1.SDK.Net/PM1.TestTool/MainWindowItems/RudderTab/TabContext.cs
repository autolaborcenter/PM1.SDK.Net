using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Autolabor.PM1.TestTool.MainWindowItems.RudderTab {
    internal enum State {
        Initialization,
        Ready,
        Forward,
        Feedback,
        Backword
    }

    internal class TabContext : BindableBase {
        private double _progress = 0;
        private string _helpText = "";
        private State _state = State.Initialization;

        public double Progress {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public string HelpText {
            get => _helpText;
            set => SetProperty(ref _helpText, value);
        }

        public State State {
            get => _state;
            set {
                if (!SetProperty(ref _state, value)) return;
                Notify(nameof(StartButtonVisible));
                Notify(nameof(ForwardButtonVisible));
                Notify(nameof(ForwardButtonEnabled));
                Notify(nameof(BackwardButtonEnabled));
            }
        }

        public Visibility StartButtonVisible
            => _state == State.Initialization
               ? Visibility.Visible
               : Visibility.Collapsed;

        public Visibility ForwardButtonVisible
            => _state == State.Ready || _state == State.Forward
               ? Visibility.Visible
               : Visibility.Collapsed;

        public bool ForwardButtonEnabled
            => _state != State.Forward;

        public bool BackwardButtonEnabled
            => _state != State.Backword;
    }
}
