using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Autolabor.PM1 {
    public static class AsyncMethods {
        private delegate void ChassisAction(out double progress);
        public delegate void ExceptionHandler(Exception exception);

        private static async Task RunActionAsync(
            ChassisAction action,
            IProgress<double> progress,
            ExceptionHandler handler
        ) {
            var _progress = .0;
            var task = Task.Run(() => {
                try { action(out _progress); } catch (Exception e) { handler?.Invoke(e); }
            });
            await Task.Run(
                async () => {
                    while (!task.GetAwaiter().IsCompleted) {
                        progress.Report(_progress);
                        await Task.Delay(50).ConfigureAwait(false);
                    }
                    progress.Report(_progress);
                }).ConfigureAwait(true);
        }

        public static async Task DriveSpatialAsync(
            double v, double w, double spatium,
            IProgress<double> progress,
            ExceptionHandler handler
        ) => await RunActionAsync(
                 (out double _progress) => Methods.DriveSpatial(v, w, spatium, out _progress),
                 progress, handler
             ).ConfigureAwait(true);
    }
}
