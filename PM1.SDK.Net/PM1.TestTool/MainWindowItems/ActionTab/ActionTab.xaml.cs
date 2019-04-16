using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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

            if (PauseToggle.IsChecked == true)
                Pause();
            else
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

                Grid.SetRow(ActionEditor, 3);
                Grid.SetColumn(ActionEditor, 1);

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

                Grid.SetRow(ActionEditor, 1);
                Grid.SetColumn(ActionEditor, 2);

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

        private Task task = null;

        private void Pause() {
            while (!Methods.Paused) {
                Methods.Paused = true;
                Thread.Sleep(10);
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e) {
            var toggle = (ToggleButton)sender;
            if (toggle.IsChecked == true) {
                Pause();
                toggle.Content = "已暂停";
            } else {
                Methods.Paused = false;
                toggle.Content = "暂停";
            }
        }

        private void ActionEditor_OnCompleted(double v, double w, bool timeBased, double range) {

            ActionList.Items.Add(new ActionConfig { v = v, w = w, range = range, timeBased = timeBased });

            if (task == null) {
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
                task = Task.Run(() => {
                    try {
                        while (ActionList.Items.Count > 0) {
                            if (ActionList.Items[0] is ActionConfig action) {
                                ActionList.Dispatch(it => it.SelectedIndex = 0);
                                if (action.timeBased)
                                    Methods.DriveTiming(action.v, action.w, action.range, out progress);
                                else
                                    Methods.DriveSpatial(action.v, action.w, action.range, out progress);
                            }
                            ActionList.Dispatch(it => it.Items.RemoveAt(0));
                        }
                    } catch (Exception exception) {
                        _windowContext.ErrorInfo = exception.Message;
                    } finally {
                        task = null;
                        flag = false;
                    }
                });
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            ActionList.Items.Clear();
            try {
                Methods.CancelTask();
            } catch (Exception exception) {
                _windowContext.ErrorInfo = exception.Message;
            }
        }
    }
}
