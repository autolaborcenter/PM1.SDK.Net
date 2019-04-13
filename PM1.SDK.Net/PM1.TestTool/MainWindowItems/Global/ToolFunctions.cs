using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Autolabor.PM1.TestTool {
    internal static partial class ToolFunctions {
        public static void Dispatch<T>(this T control, Action<T> action)
          where T : Control => control.Dispatcher.Invoke(action, control);

        public static async Task Handle(
            MainWindowContext context,
            CancellationTokenSource connecting
        ) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try {
                while (!connecting.IsCancellationRequested) {
                    context.ConnectedTime = (stopwatch.ElapsedMilliseconds / 1000.0).ToString("0.0");
                    try {
                        context.State = Methods.State;
                        var (_, _, x, y, theta, _, _, _) = Methods.Odometry;
                        context.Odometry = string.Format("{0}, {1}, {2}°",
                                                    x.ToString("0.0"),
                                                    y.ToString("0.0"),
                                                    theta.ToString("0.0"));
                    } catch (Exception exception) {
                        context.ErrorInfo = exception.Message;
                    }
                    await Task.Delay(99, connecting.Token).ConfigureAwait(false);
                }
            } catch (TaskCanceledException) {
                try {
                    Methods.Shutdown();
                } catch (Exception exception) {
                    context.ErrorInfo = exception.Message;
                }
            } catch (Exception exception) {
                context.ErrorInfo = exception.Message;
            } finally {
                context.Connected = false;
            }
        }
    }
}
