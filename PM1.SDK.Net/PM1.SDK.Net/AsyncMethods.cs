using System;
using System.Threading.Tasks;
using static Autolabor.PM1.Methods;

namespace Autolabor.PM1 {
    public static class AsyncMethods {
        private delegate void ChassisAction(out double progress);
        private delegate T ChassisAction<T>(out double progress);

        private static async Task RunActionAsync(
            ChassisAction action,
            Action<double> progress,
            TimeSpan progressUpdatePeriod,
            Action<Exception> handler
        ) {
            var _progress = .0;
            var _running = true;
            _ = Task.Run(
                async () => {
                    while (_running) {
                        progress(_progress);
                        await Task.Delay(progressUpdatePeriod).ConfigureAwait(false);
                    }
                    progress(_progress);
                });
            await Task.Run(() => {
                try {
                    action(out _progress);
                } catch (Exception e) {
                    handler?.Invoke(e);
                } finally {
                    _running = false;
                }
            }).ConfigureAwait(true);
        }

        private static async Task<T> RunActionAsync<T>(
            ChassisAction<T> action,
            Action<double> progress,
            TimeSpan progressUpdatePeriod,
            Action<Exception> handler
        ) {
            var _progress = .0;
            var _running = true;
            _ = Task.Run(
                async () => {
                    while (_running) {
                        progress(_progress);
                        await Task.Delay(progressUpdatePeriod).ConfigureAwait(false);
                    }
                    progress(_progress);
                });
            return await Task.Run(() => {
                try {
                    return action(out _progress);
                } catch (Exception e) {
                    handler?.Invoke(e);
                    return default;
                } finally {
                    _running = false;
                }
            }).ConfigureAwait(true);
        }

        /// <summary>
        /// 异步初始化。
        /// </summary>
        /// <param name="port">串口名字</param>
        /// <param name="progress">进度报告回调</param>
        /// <param name="handler">异常处理回调</param>
        /// <returns>
        /// 实际连接的串口名字。
        /// </returns>
        public static async Task<string> InitializeAsync(
            string port,
            Action<double> progress,
            Action<Exception> handler
        ) => await RunActionAsync(
                 (out double _progress) => Initialize(port, out _progress),
                 progress, TimeSpan.FromMilliseconds(50), handler
             ).ConfigureAwait(false);

        /// <summary>
        /// 控制机器人异步执行受空间约束的指定动作。
        /// </summary>
        /// <param name="v">线速度（米/秒）</param>
        /// <param name="w">角速度（弧度/秒）</param>
        /// <param name="spatium">空间约束</param>
        /// <param name="progress">进度报告回调</param>
        /// <param name="handler">异常处理回调</param>
        public static async Task DriveAsync(
            double v, double w,
            double spatium, double angle,
            Action<double> progress,
            Action<Exception> handler
        ) => await RunActionAsync(
                 (out double _progress) => Drive(v, w, spatium, angle, out _progress),
                 progress, TimeSpan.FromMilliseconds(50), handler
             ).ConfigureAwait(false);

        /// <summary>
        /// 控制机器人异步执行受时间约束的指定动作。
        /// </summary>
        /// <param name="v">线速度（米/秒）</param>
        /// <param name="w">角速度（弧度/秒）</param>
        /// <param name="time">时间约束</param>
        /// <param name="progress">进度报告回调</param>
        /// <param name="handler">异常处理回调</param>
        public static async Task DriveAsync(
            double v, double w, TimeSpan time,
            Action<double> progress,
            Action<Exception> handler
        ) => await RunActionAsync(
                (out double _progress) => Drive(v, w, time, out _progress),
                progress, TimeSpan.FromMilliseconds(50), handler
             ).ConfigureAwait(false);

        /// <summary>
        /// 异步调整后轮零位。
        /// </summary>
        /// <param name="offset">调整量（弧度）</param>
        /// <param name="progress">进度报告回调</param>
        /// <param name="handler">异常处理回调</param>
        public static async Task AdjustRudderAsync(
            double offset,
            Action<double> progress,
            Action<Exception> handler
        ) => await RunActionAsync(
                (out double _progress) => AdjustRudder(offset, out _progress),
                progress, TimeSpan.FromMilliseconds(50), handler
             ).ConfigureAwait(false);
    }
}
