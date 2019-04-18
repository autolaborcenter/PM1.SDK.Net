using System;
using System.Runtime.InteropServices;
using static Autolabor.PM1.SafeNativeMethods;

namespace Autolabor.PM1 {
    /// <summary>
    ///     底盘参数结构体
    /// </summary>
    public struct Config {
        /// <summary>
        ///     轮间距
        /// </summary>
        public double Width;

        /// <summary>
        ///     轴间距
        /// </summary>
        public double Length;

        /// <summary>
        ///     轮半径
        /// </summary>
        public double WheelRadius;

        /// <summary>
        ///     优化宽度
        /// </summary>
        public double OptimizeWidth;

        /// <summary>
        ///     最大加速度
        /// </summary>
        public double Acceleration;

        /// <summary>
        ///     最大线速度
        /// </summary>
        public double MaxV;

        /// <summary>
        ///     最大角速度
        /// </summary>
        public double MaxW;
    }

    /// <summary>
    ///     底盘工作状态
    /// </summary>
    public enum StateEnum {
        /// <summary>
        ///     所有节点离线
        /// </summary>
        Offline = 0,

        /// <summary>
        ///     底盘已解锁
        /// </summary>
        Unlocked = 1,

        /// <summary>
        ///     异常状态，尝试锁定底盘以恢复
        /// </summary>
        Error = 0x7f,

        /// <summary>
        ///     底盘已锁定
        /// </summary>
        Locked = 0xff
    }

    /// <summary>
    ///     底盘控制类
    /// </summary>
    public static class Methods {
        /// <summary>
        ///     代理底层 API，错误信息转化为日常
        /// </summary>
        /// <param name="handler">任务 id</param>
        private static void OnNative(uint handler) {
            var error = Marshal.PtrToStringAnsi(GetErrorInfo(handler));
            if (!string.IsNullOrWhiteSpace(error)) {
                RemoveErrorInfo(handler);
                throw new Exception(error);
            }
        }

        /// <summary>
        ///     获取默认底盘参数
        /// </summary>
        public static Config
            DefaultConfig {
            get {
                var result = new Config();
                GetDefaultChassisConfig(
                       out result.Width,
                       out result.Length,
                       out result.WheelRadius,
                       out result.OptimizeWidth,
                       out result.Acceleration,
                       out result.MaxV,
                       out result.MaxW);
                return result;
            }
        }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="port">端口名</param>
        /// <param name="config">配置</param>
        /// <param name="progress">进度</param>
        /// <returns>已连接的端口</returns>
        public static string Initialize(
            string port,
            Config? config,
            out double progress) {
            var configNotNull = config ?? new Config {
                Width = double.NaN,
                Length = double.NaN,
                WheelRadius = double.NaN,
                OptimizeWidth = double.NaN,
                Acceleration = double.NaN,
                MaxV = double.NaN,
                MaxW = double.NaN
            };
            OnNative(SafeNativeMethods.Initialize(
                port,
                configNotNull.Width,
                configNotNull.Length,
                configNotNull.WheelRadius,
                configNotNull.OptimizeWidth,
                configNotNull.Acceleration,
                configNotNull.MaxV,
                configNotNull.MaxW,
                out progress));
            return Marshal.PtrToStringAnsi(GetConnectedPort());
        }

        /// <summary>
        ///     关闭连接
        /// </summary>
        public static void Shutdown() => OnNative(SafeNativeMethods.Shutdown());

        /// <summary>
        ///     读取里程计
        /// </summary>
        public static (double s, double sa,
            double x, double y, double theta,
            double vx, double vy, double w)
            Odometry {
            get {
                OnNative(GetOdometry(out var s, out var sa,
                                      out var x, out var y, out var theta,
                                      out var vx, out var vy, out var w));
                return (s, sa, x, y, theta, vx, vy, w);
            }
        }

        /// <summary>
        ///     重置里程计
        /// </summary>
        public static void ResetOdometry() => OnNative(SafeNativeMethods.ResetOdometry());

        /// <summary>
        ///     锁定或解锁底盘
        /// </summary>
        public static StateEnum State {
            get => (StateEnum)CheckState();

            set {
                switch (value) {
                    case StateEnum.Offline:
                        OnNative(SafeNativeMethods.Shutdown());
                        break;
                    case StateEnum.Unlocked:
                        OnNative(Unlock());
                        break;
                    case StateEnum.Locked:
                        OnNative(Lock());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException
                            (nameof(value), value, "Illegal State");
                }
            }
        }

        /// <summary>
        ///     设定物理模型目标
        /// </summary>
        /// <param name="speed">轮速</param>
        /// <param name="rudder">舵</param>
        public static void SetPhysicalTarget(double speed, double rudder) 
            => OnNative(DrivePhysical(speed, rudder));

        /// <summary>
        ///     发布速度指令
        /// </summary>
        public static (double v, double w) Velocity {
            set => OnNative(Drive(value.v, value.w));
            get {
                OnNative(GetOdometry(out _, out _, out _, out _, out _,
                                     out var vx, out var vy, out var w));
                return (Math.Sqrt(vx * vx + vy * vy), w);
            }
        }

        /// <summary>
        ///     计算空间尺度
        /// </summary>
        /// <param name="spatium">轨迹弧长</param>
        /// <param name="angle">轨迹夹角</param>
        /// <returns>尺度</returns>
        public static double SpatiumCalculate(double spatium, double angle)
            => SafeNativeMethods.SpatiumCalculate(spatium, angle);

        /// <summary>
        ///     按空间驱动
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <param name="meters"></param>
        /// <param name="progress"></param>
        public static void DriveSpatial(double v, double w, double meters, out double progress) 
            => OnNative(SafeNativeMethods.DriveSpatial(v, w, meters, out progress));

        /// <summary>
        ///     按时间驱动
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <param name="seconds"></param>
        /// <param name="progress"></param>
        public static void DriveTiming(double v, double w, double seconds, out double progress)
            => OnNative(SafeNativeMethods.DriveTiming(v, w, seconds, out progress));

        /// <summary>
        ///     前进
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="meters">路程（米）</param>
        /// <param name="progress">进度</param>
        public static void GoStraight(double speed, double meters, out double progress) {
            if (speed == 0) {
                if (meters == 0) {
                    progress = 1;
                    return;
                }
                throw new ArgumentException("action never complete");
            }
            if (meters <= 0)
                throw new ArgumentException("invalid target");

            OnNative(SafeNativeMethods.DriveSpatial(speed, 0, meters, out progress));
        }

        /// <summary>
        ///     前进
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="time">时间（秒）</param>
        /// <param name="progress">进度</param>
        public static void GoStraight(double speed, TimeSpan time, out double progress)
            => OnNative(SafeNativeMethods.DriveTiming(speed, 0, time.TotalSeconds, out progress));

        /// <summary>
        ///     原地转
        /// </summary>
        /// <param name="speed">角速度</param>
        /// <param name="rad">弧度</param>
        /// <param name="progress">进度</param>
        public static void TurnAround(double speed, double rad, out double progress) {
            if (speed == 0) {
                if (rad == 0) {
                    progress = 1;
                    return;
                }
                throw new ArgumentException("action never complete");
            }
            if (rad <= 0)
                throw new ArgumentException("invalid target");

            OnNative(SafeNativeMethods.DriveSpatial(0, speed, SpatiumCalculate(0, rad), out progress));
        }

        /// <summary>
        ///     原地转
        /// </summary>
        /// <param name="speed">角速度</param>
        /// <param name="time">时间</param>
        /// <param name="progress">进度</param>
        public static void TurnAround(double speed, TimeSpan time, out double progress)
            => OnNative(SafeNativeMethods.DriveTiming(0, speed, time.TotalSeconds, out progress));

        /// <summary>
        ///     走圆弧
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="r">半径（米）</param>
        /// <param name="rad">弧度</param>
        /// <param name="progress">进度</param>
        public static void GoArc(double speed, double r, double rad, out double progress) {
            if (Math.Abs(r) < 0.05)
                throw new ArgumentException("radius is too little, use turn_around instead");
            if (speed == 0) {
                if (rad == 0) {
                    progress = 1;
                    return;
                }
                throw new ArgumentException("action never complete");
            }
            if (rad <= 0)
                throw new ArgumentException("invalid target");

            OnNative(SafeNativeMethods.DriveSpatial(speed, speed / r, SpatiumCalculate(r * rad, rad), out progress));
        }

        /// <summary>
        ///     走圆弧
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="r">半径</param>
        /// <param name="time">时间</param>
        /// <param name="progress">进度</param>
        public static void GoArc(double speed, double r, TimeSpan time, out double progress) {
            if (Math.Abs(r) < 0.05)
                throw new ArgumentException("radius is too little, use turn_around instead");

            OnNative(SafeNativeMethods.DriveTiming(speed, speed / r, time.TotalSeconds, out progress));
        }

        /// <summary>
        ///     调整后轮零位
        /// </summary>
        /// <param name="offset">偏置</param>
        /// <param name="progress">进度</param>
        public static void AdjustRudder(double offset, out double progress) {
            if (Math.Abs(offset) >= Math.PI / 2)
                throw new ArgumentException("more than +-90 degree is not supported yet");

            OnNative(SafeNativeMethods.AdjustRudder(offset, out progress));
        }

        /// <summary>
        ///     暂停或恢复
        /// </summary>
        public static bool Paused {
            get => IsPaused();
            set {
                if (value) Pause();
                else Resume();
            }
        }

        /// <summary>
        ///     取消任务
        /// </summary>
        public static void CancelTask() => CancelAll();
    }
}
