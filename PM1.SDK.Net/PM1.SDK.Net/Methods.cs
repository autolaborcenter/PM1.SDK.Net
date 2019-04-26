using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Autolabor.PM1.SafeNativeMethods;

namespace Autolabor.PM1 {
    /// <summary>
    /// 可能的底盘工作状态。
    /// </summary>
    public enum StateEnum {
        /// <summary>
        /// 所有节点离线。
        /// </summary>
        Offline = 0,

        /// <summary>
        /// 底盘已解锁。
        /// </summary>
        Unlocked = 1,

        /// <summary>
        /// 异常状态，尝试锁定底盘以恢复
        /// </summary>
        Error = 0x7f,

        /// <summary>
        /// 底盘已锁定。
        /// </summary>
        Locked = 0xff
    }

    /// <summary>
    ///     底盘控制类
    /// </summary>
    public static class Methods {

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="port">端口名</param>
        /// <param name="config">配置</param>
        /// <param name="progress">进度</param>
        /// <returns>已连接的端口</returns>
        public static string Initialize(string port, out double progress) {
            OnNative(SafeNativeMethods.Initialize(port, out progress));
            return Marshal.PtrToStringAnsi(GetConnectedPort());
        }

        /// <summary>
        ///     关闭连接
        /// </summary>
        public static void Shutdown() => OnNative(SafeNativeMethods.Shutdown());

        /// <summary>
        ///     安全关闭
        /// </summary>
        /// <returns>是否成功关闭</returns>
        public static bool ShutdownSafety() 
            => string.IsNullOrWhiteSpace(Marshal.PtrToStringAnsi(GetErrorInfo(SafeNativeMethods.Shutdown())));

        /// <summary>
        ///     参数索引
        /// </summary>
        public static readonly Parameters Parameters = new Parameters();

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
        ///     控制机器人运行
        /// </summary>
        public static (double speed, double rudder) PhysicalTarget {
            set => OnNative(DrivePhysical(value.speed, value.rudder));
        }

        /// <summary>
        ///     控制机器人运行
        /// </summary>
        public static (double left, double right) WheelsTarget {
            set => OnNative(DriveWheels(value.left, value.right));
        }

        /// <summary>
        ///     控制机器人运行
        /// </summary>
        public static (double v, double w) VelocityTarget {
            set => OnNative(DriveVelocity(value.v, value.w));
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
        /// <param name="spatium"></param>
        /// <param name="progress"></param>
        public static void DriveSpatial(double v, double w, double spatium, out double progress)
            => OnNative(SafeNativeMethods.DriveSpatial(v, w, spatium, out progress));

        /// <summary>
        ///     按时间驱动
        /// </summary>
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <param name="time"></param>
        /// <param name="progress"></param>
        public static void DriveTiming(double v, double w, double time, out double progress)
            => OnNative(SafeNativeMethods.DriveTiming(v, w, time, out progress));

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

        private const string
            IllegalArgument = "illegal action argument",
            InfiniteAction = "action never complete",
            NegativeTarget = "action target argument must be positive";

        private static bool CheckArguments(double speed, double radius, double range) {
            if (radius == 0) throw new ArgumentException(IllegalArgument);
            if (range == 0) return true;
            if (speed == 0) throw new ArgumentException(InfiniteAction);
            if (range < 0) throw new ArgumentException(NegativeTarget);
            return false;
        }

        /// <summary>
        ///     走圆弧
        /// </summary>
        /// <param name="r">半径（米）</param>
        /// <param name="progress">进度</param>
        public static void GoArcVS(double v, double r, double s, out double progress) {
            if (CheckArguments(v, r, s)) { progress = 1; return; }
            DriveSpatial(v, v / r, SpatiumCalculate(s, s / r), out progress);
        }

        /// <summary>
        ///     走圆弧
        /// </summary>
        /// <param name="r">半径（米）</param>
        /// <param name="progress">进度</param>
        public static void GoArcVA(double v, double r, double a, out double progress) {
            if (CheckArguments(v, r, a)) { progress = 1; return; }
            DriveSpatial(v, v / r, SpatiumCalculate(a * r, a), out progress);
        }

        /// <summary>
        ///     走圆弧
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="r">半径（米）</param>
        /// <param name="rad">弧度</param>
        /// <param name="progress">进度</param>
        public static void GoArcWS(double w, double r, double s, out double progress) {
            if (CheckArguments(w, r, s)) { progress = 1; return; }
            DriveSpatial(w * r, w, SpatiumCalculate(s, s / r), out progress);
        }

        /// <summary>
        ///     走圆弧
        /// </summary>
        /// <param name="r">半径（米）</param>
        /// <param name="progress">进度</param>
        public static void GoArcWA(double w, double r, double a, out double progress) {
            if (CheckArguments(w, r, a)) { progress = 1; return; }
            DriveSpatial(w * r, w, SpatiumCalculate(a * r, a), out progress);
        }

        /// <summary>
        ///     走圆弧
        /// </summary>
        /// <param name="r">半径（米）</param>
        /// <param name="progress">进度</param>
        public static void GoArcVT(double v, double r, double t, out double progress) {
            if (CheckArguments(double.NaN, r, t)) { progress = 1; return; }
            DriveTiming(v, v / r, t, out progress);
        }

        /// <summary>
        ///     走圆弧
        /// </summary>
        /// <param name="r">半径（米）</param>
        /// <param name="progress">进度</param>
        public static void GoArcWT(double w, double r, double t, out double progress) {
            if (CheckArguments(double.NaN, r, t)) { progress = 1; return; }
            DriveTiming(w * r, w, t, out progress);
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
        public static void CancelAction() => SafeNativeMethods.CancelAction();
    }
}
