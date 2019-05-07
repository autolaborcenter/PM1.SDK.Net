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
        /// 初始化。
        /// </summary>
        /// <param name="port">串口名字</param>
        /// <param name="config">配置</param>
        /// <param name="progress">进度</param>
        /// <returns>
        /// 已连接的端口。
        /// </returns>
        public static string Initialize(string port, out double progress) {
            OnNative(SafeNativeMethods.Initialize(port, out progress));
            return Marshal.PtrToStringAnsi(GetConnectedPort());
        }

        /// <summary>
        /// 关闭。
        /// </summary>
        public static void Shutdown() => OnNative(SafeNativeMethods.Shutdown());

        /// <summary>
        /// 安全关闭。
        /// </summary>
        /// <returns>
        /// 关闭是否成功。
        /// </returns>
        public static bool ShutdownSafety() 
            => string.IsNullOrWhiteSpace(Marshal.PtrToStringAnsi(GetErrorInfo(SafeNativeMethods.Shutdown())));

        /// <summary>
        /// 获取参数索引器。
        /// </summary>
        public static readonly Parameters Parameters = new Parameters();

        /// <summary>
        /// 获取里程计读数。
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
        /// 清除里程计累计值。
        /// </summary>
        public static void ResetOdometry() => OnNative(SafeNativeMethods.ResetOdometry());

        /// <summary>
        /// 获取或设置底盘工作状态。
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
        /// 设置物理模型下的目标运动状态。
        /// </summary>
        public static (double speed, double rudder) PhysicalTarget {
            set => OnNative(DrivePhysical(value.speed, value.rudder));
        }

        /// <summary>
        /// 设置差动模型下的目标运动状态。
        /// </summary>
        public static (double left, double right) WheelsTarget {
            set => OnNative(DriveWheels(value.left, value.right));
        }

        /// <summary>
        /// 设置运动模型下的目标运动状态。
        /// </summary>
        public static (double v, double w) VelocityTarget {
            set => OnNative(DriveVelocity(value.v, value.w));
        }

        /// <summary>
        /// 动作：直线行驶。
        /// </summary>
        /// <param name="speed">线速度（米/秒）</param>
        /// <param name="meters">路程约束（米）</param>
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
        /// 动作：直线行驶。
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="time">时间（秒）</param>
        /// <param name="progress">进度</param>
        public static void GoStraight(double speed, TimeSpan time, out double progress)
            => OnNative(SafeNativeMethods.DriveTiming(speed, 0, time.TotalSeconds, out progress));

        /// <summary>
        /// 动作：原地转身。
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

            OnNative(SafeNativeMethods.DriveSpatial(0, speed, CalculateSpatium(0, rad), out progress));
        }

        /// <summary>
        /// 动作：原地转身。
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
        /// 动作：按圆弧行驶。
        /// </summary>
        /// <param name="r">半径（米）</param>
        /// <param name="progress">进度</param>
        public static void GoArcVS(double v, double r, double s, out double progress) {
            if (CheckArguments(v, r, s)) { progress = 1; return; }
            DriveSpatial(v, v / r, CalculateSpatium(s, s / r), out progress);
        }

        /// <summary>
        /// 动作：按圆弧行驶。
        /// </summary>
        /// <param name="r">半径（米）</param>
        /// <param name="progress">进度</param>
        public static void GoArcVA(double v, double r, double a, out double progress) {
            if (CheckArguments(v, r, a)) { progress = 1; return; }
            DriveSpatial(v, v / r, CalculateSpatium(a * r, a), out progress);
        }

        /// <summary>
        /// 动作：按圆弧行驶。
        /// </summary>
        /// <param name="speed">速度</param>
        /// <param name="r">半径（米）</param>
        /// <param name="rad">弧度</param>
        /// <param name="progress">进度</param>
        public static void GoArcWS(double w, double r, double s, out double progress) {
            if (CheckArguments(w, r, s)) { progress = 1; return; }
            DriveSpatial(w * r, w, CalculateSpatium(s, s / r), out progress);
        }

        /// <summary>
        /// 动作：按圆弧行驶。
        /// </summary>
        /// <param name="r">半径（米）</param>
        /// <param name="progress">进度</param>
        public static void GoArcWA(double w, double r, double a, out double progress) {
            if (CheckArguments(w, r, a)) { progress = 1; return; }
            DriveSpatial(w * r, w, CalculateSpatium(a * r, a), out progress);
        }

        /// <summary>
        /// 动作：按圆弧行驶。
        /// </summary>
        /// <param name="r">半径（米）</param>
        /// <param name="progress">进度</param>
        public static void GoArcVT(double v, double r, double t, out double progress) {
            if (CheckArguments(double.NaN, r, t)) { progress = 1; return; }
            DriveTiming(v, v / r, t, out progress);
        }

        /// <summary>
        /// 动作：按圆弧行驶。
        /// </summary>
        /// <param name="r">半径（米）</param>
        /// <param name="progress">进度</param>
        public static void GoArcWT(double w, double r, double t, out double progress) {
            if (CheckArguments(double.NaN, r, t)) { progress = 1; return; }
            DriveTiming(w * r, w, t, out progress);
        }

        /// <summary>
        /// 计算空间尺度。
        /// </summary>
        /// <param name="spatium">轨迹弧长</param>
        /// <param name="angle">轨迹圆心角</param>
        /// <returns>
        /// 描述运动约束的尺度值。
        /// </returns>
        public static double CalculateSpatium(double spatium, double angle)
            => SafeNativeMethods.CalculateSpatium(spatium, angle);

        /// <summary>
        /// 控制机器人执行受空间约束的指定动作。
        /// </summary>
        /// <param name="v">线速度（米/秒）</param>
        /// <param name="w">角速度（弧度/秒）</param>
        /// <param name="spatium">空间约束</param>
        /// <param name="progress">进度</param>
        public static void Drive(double v, double w, double spatium, out double progress)
            => OnNative(DriveSpatial(v, w, spatium, out progress));

        /// <summary>
        /// 控制机器人执行受时间约束的指定动作。
        /// </summary>
        /// <param name="v">线速度（米/秒）</param>
        /// <param name="w">角速度（弧度/秒）</param>
        /// <param name="time">时间约束（秒）</param>
        /// <param name="progress">进度</param>
        public static void Drive(double v, double w, TimeSpan time, out double progress)
            => OnNative(DriveTiming(v, w, time.TotalSeconds, out progress));

        /// <summary>
        /// 动作：调整后轮零位。
        /// </summary>
        /// <param name="offset">偏置</param>
        /// <param name="progress">进度</param>
        public static void AdjustRudder(double offset, out double progress) {
            if (Math.Abs(offset) >= Math.PI / 2)
                throw new ArgumentException("more than +-90 degree is not supported yet");

            OnNative(SafeNativeMethods.AdjustRudder(offset, out progress));
        }

        /// <summary>
        /// 查询、设置或解除暂停。
        /// </summary>
        public static bool Paused {
            get => IsPaused();
            set => SetPaused(value);
        }

        /// <summary>
        /// 取消动作。
        /// </summary>
        public static void CancelAction() => SafeNativeMethods.CancelAction();
    }
}
