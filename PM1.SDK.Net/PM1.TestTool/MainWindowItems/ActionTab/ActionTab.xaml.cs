using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Autolabor.PM1.TestTool.MainWindowItems.ActionTab {
    /// <summary>
    /// ActionTab.xaml 的交互逻辑
    /// </summary>
    public partial class ActionTab : UserControl, ITabControl {
        private MainWindowContext _windowContext;

        public ActionTab() => InitializeComponent();

        public void OnEnter() {
            ActionList.Items.Clear();
            ActionEditor.Reset();
            RudderControl.Reset();
            PauseToggle.IsChecked = true;
        }

        public void OnLeave() { }

        private void Tab_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => _windowContext = e.NewValue as MainWindowContext;

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e) {
            var grid = (Grid)sender;

            if (e.NewSize.Height > 640) {
                grid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                grid.RowDefinitions[3].Height = GridLength.Auto;

                grid.ColumnDefinitions[2].Width = GridLength.Auto;

                Grid.SetRow(ActionTitle, 2);
                Grid.SetColumn(ActionTitle, 1);

                Grid.SetRow(ActionGrid, 3);
                Grid.SetColumn(ActionGrid, 1);

                if (e.NewSize.Width > 500) {
                    CanvasBorder.BorderThickness = new Thickness(1);
                    grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[1].Width = new GridLength(240, GridUnitType.Pixel);
                } else {
                    CanvasBorder.BorderThickness = new Thickness(0);
                    grid.ColumnDefinitions[0].Width = GridLength.Auto;
                    grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                }
            } else {
                grid.RowDefinitions[1].Height = GridLength.Auto;
                grid.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);

                Grid.SetRow(ActionTitle, 0);
                Grid.SetColumn(ActionTitle, 2);

                Grid.SetRow(ActionGrid, 1);
                Grid.SetColumn(ActionGrid, 2);

                if (e.NewSize.Width > 600) {
                    CanvasBorder.BorderThickness = new Thickness(1);
                    grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[1].Width = GridLength.Auto;
                    grid.ColumnDefinitions[2].Width = new GridLength(240, GridUnitType.Pixel);
                } else {
                    CanvasBorder.BorderThickness = new Thickness(0);
                    grid.ColumnDefinitions[0].Width = GridLength.Auto;
                    grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                }
            }
        }

        private struct ActionConfig {
            public double v, w, range;
            public bool timeBased;

            public override string ToString()
                => string.Format(
                    CultureInfo.InvariantCulture,
                    "v = {0}m/s | ω = {1}°/s",
                    ToolFunctions.Format("0.##", v),
                    ToolFunctions.Format("0.#", w.ToDegree()));
        }

        private struct RudderControlConfig {
            public double value;

            public override string ToString()
                => string.Format(
                    CultureInfo.InvariantCulture,
                    "调整后轮：{0}°",
                    ToolFunctions.Format("0.#", value.ToDegree()));
        }

        public class ProgressHandler : IProgress<double> {
            private readonly Action<double> _action;

            public ProgressHandler(Action<double> action) => _action = action;

            public void Report(double value) => _action(value);
        }

        private Task task = null;

        private void PauseToggle_Checked(object sender, RoutedEventArgs e) {
            Methods.Paused = true;
            ((ToggleButton)sender).Content = "已暂停";
        }

        private void PauseToggle_Unchecked(object sender, RoutedEventArgs e) {
            Methods.Paused = false;
            ((ToggleButton)sender).Content = "暂停";
        }

        private async Task InvokeActions() {
            try {
                while (!ActionList.Items.IsEmpty) {
                    ActionList.Dispatch(it => it.SelectedIndex = 0);

                    if (ActionList.Items[0] is ActionConfig action)
                        if (action.timeBased)
                            await AsyncMethods.DriveAsync(
                                action.v, action.w,
                                TimeSpan.FromSeconds(action.range),
                                new ProgressHandler(it => _windowContext.Progress = it),
                                (e) => _windowContext.ErrorInfo = e.Message
                            ).ConfigureAwait(true);
                        else
                            await AsyncMethods.DriveAsync(
                                action.v, action.w,
                                action.range,
                                new ProgressHandler(it => _windowContext.Progress = it),
                                (e) => _windowContext.ErrorInfo = e.Message
                            ).ConfigureAwait(true);

                    else if (ActionList.Items[0] is RudderControlConfig rudderControl)
                        await AsyncMethods.AdjustRudderAsync(
                           rudderControl.value,
                           new ProgressHandler(it => _windowContext.Progress = it),
                           (e) => _windowContext.ErrorInfo = e.Message
                       ).ConfigureAwait(true);

                    ActionList.Dispatch(it => {
                        try { it.Items.RemoveAt(0); } catch (ArgumentOutOfRangeException) { }
                    });
                }
            } finally {
                task = null;
            }
        }

        private void ActionEditor_OnCompleted(double v, double w, bool timeBased, double range) {
            ActionList.Items.Add(new ActionConfig { v = v, w = w, range = range, timeBased = timeBased });
            if (task == null) task = Task.Run(InvokeActions);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            ActionList.Items.Clear();
            try {
                Methods.CancelAction();
            } catch (Exception exception) {
                _windowContext.ErrorInfo = exception.Message;
            }
        }

        private void RudderControl_OnCompleted(object sender, double value) {
            ActionList.Items.Add(new RudderControlConfig { value = value });
            if (task == null) task = Task.Run(InvokeActions);
        }
    }
}
