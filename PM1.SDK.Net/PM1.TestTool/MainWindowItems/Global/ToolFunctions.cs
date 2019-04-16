using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Autolabor.PM1.TestTool {
    internal static partial class ToolFunctions {
        public static void Dispatch<T>(this T control, Action<T> action)
          where T : Control => control.Dispatcher.Invoke(action, control);

        public static string Format(string format, double value)
               => value.ToString(format, CultureInfo.InvariantCulture);

        public static double ToRad(this double degree) => degree * Math.PI / 180;

        public static double ToDegree(this double rad) => rad * 180 / Math.PI;

        public static async Task Handle(
            MainWindowContext context,
            CancellationTokenSource connecting
        ) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try {
                while (!connecting.IsCancellationRequested) {
                    context.ConnectedTime = Format("0.0", stopwatch.ElapsedMilliseconds / 1000.0);
                    try {
                        context.State = Methods.State;
                        var (_, _, x, y, theta, _, _, _) = Methods.Odometry;
                        context.Odometry = string.Format(
                            CultureInfo.InvariantCulture,
                            "{0}, {1}, {2}°",
                            Format("0.##", x),
                            Format("0.##", y),
                            Format("0.#", theta.ToDegree()));
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
