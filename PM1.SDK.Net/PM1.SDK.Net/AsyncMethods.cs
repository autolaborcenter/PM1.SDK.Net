using System;
using System.Threading.Tasks;
using static Autolabor.PM1.Methods;

namespace Autolabor.PM1 {
    public static class AsyncMethods {
        private delegate void ChassisAction(out double progress);

        /// <summary>
        ///     异常处理器
        /// </summary>
        /// <param name="exception">异常对象</param>
        public delegate void ExceptionHandler(Exception exception);

        /// <summary>
        ///     阻塞转异步
        /// </summary>
        /// <param name="action">动作</param>
        /// <param name="progress">进度报告</param>
        /// <param name="progressUpdatePeriod">进度更新周期</param>
        /// <param name="handler">异常处理器</param>
        /// <returns>任务对象</returns>
        private static async Task RunActionAsync(
            ChassisAction action,
            IProgress<double> progress,
            TimeSpan progressUpdatePeriod,
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
                        await Task.Delay(progressUpdatePeriod).ConfigureAwait(false);
                    }
                    progress.Report(_progress);
                }).ConfigureAwait(true);
        }

        /// <summary>
        ///     异步初始化
        /// </summary>
        /// <param name="port">串口名字</param>
        /// <param name="config">初始配置参数</param>
        /// <param name="progress">进度报告</param>
        /// <param name="handler">异常处理器</param>
        /// <returns>任务对象</returns>
        public static async Task InitializeAsync(
            string port,
            IProgress<double> progress,
            ExceptionHandler handler
        ) => await RunActionAsync(
                 (out double _progress) => Initialize(port, out _progress),
                 progress, TimeSpan.FromMilliseconds(50), handler
             ).ConfigureAwait(true);

        /// <summary>
        ///     空间约束的异步行驶
        /// </summary>
        /// <param name="v">线速度</param>
        /// <param name="w">角速度</param>
        /// <param name="spatium">空间尺度</param>
        /// <param name="progress">进度报告</param>
        /// <param name="handler">异常处理器</param>
        /// <returns>任务对象</returns>
        public static async Task DriveAsync(
            double v, double w, double spatium,
            IProgress<double> progress,
            ExceptionHandler handler
        ) => await RunActionAsync(
                 (out double _progress) => DriveSpatial(v, w, spatium, out _progress),
                 progress, TimeSpan.FromMilliseconds(50), handler
             ).ConfigureAwait(true);

        /// <summary>
        ///     时间约束的异步行驶
        /// </summary>
        /// <param name="v">线速度</param>
        /// <param name="w">角速度</param>
        /// <param name="time">时间尺度</param>
        /// <param name="progress">进度报告</param>
        /// <param name="handler">异常处理器</param>
        /// <returns>任务对象</returns>
        public static async Task DriveAsync(
            double v, double w, TimeSpan time,
            IProgress<double> progress,
            ExceptionHandler handler
        ) => await RunActionAsync(
                (out double _progress) => DriveTiming(v, w, time.TotalSeconds, out _progress),
                progress, TimeSpan.FromMilliseconds(50), handler
             ).ConfigureAwait(true);

        /// <summary>
        ///     异步调整后轮
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="progress"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static async Task AdjustRudderAsync(
            double offset,
             IProgress<double> progress,
            ExceptionHandler handler
        ) => await RunActionAsync(
                (out double _progress) => AdjustRudder(offset, out _progress),
                progress, TimeSpan.FromMilliseconds(50), handler
             ).ConfigureAwait(true);
    }
}
