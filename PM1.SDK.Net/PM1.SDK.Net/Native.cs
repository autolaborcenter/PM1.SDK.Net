using System;
using System.Runtime.InteropServices;
using Handler = System.UInt32;

namespace Autolabor.PM1 {
    internal static class SafeNativeMethods {
        public static readonly bool Win32 = Environment.OSVersion.Platform == PlatformID.Win32NT;

        /// <summary>
        ///     代理底层 API，错误信息转化为异常
        /// </summary>
        /// <param name="handler">任务 id</param>
        public static void OnNative(uint handler) {
            var error = Marshal.PtrToStringAnsi(GetErrorInfo(handler));
            if (!string.IsNullOrWhiteSpace(error)) {
                RemoveErrorInfo(handler);
                throw new Exception(error);
            }
        }

        /// <summary>
        /// 从错误管理器获取错误信息
        /// </summary>
        /// <param name="handler">操作序号</param>
        /// <returns>错误信息（C 风格字符串）</returns>
        public static IntPtr GetErrorInfo(Handler handler)
            => Win32
                   ? SafeNativeMethodsWin32.GetErrorInfo(handler)
                   : SafeNativeMethodsUnix.GetErrorInfo(handler);

        /// <summary>
        /// 从错误管理器移除错误信息
        /// </summary>
        /// <param name="handler">操作序号</param>
        public static void RemoveErrorInfo(Handler handler) {
            if (Win32)
                SafeNativeMethodsWin32.RemoveErrorInfo(handler);
            else
                SafeNativeMethodsUnix.RemoveErrorInfo(handler);
        }

        /// <summary>
        /// 从错误管理器清除所有错误信息
        /// </summary>
        public static void ClearErrorInfo() {
            if (Win32)
                SafeNativeMethodsWin32.ClearErrorInfo();
            else
                SafeNativeMethodsUnix.ClearErrorInfo();
        }

        /// <summary>
        /// 获取当前打开的串口名字
        /// </summary>
        /// <returns>串口名字（C 风格字符串）</returns>
        public static IntPtr GetConnectedPort()
            => Win32
                   ? SafeNativeMethodsWin32.GetConnectedPort()
                   : SafeNativeMethodsUnix.GetConnectedPort();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="port">目标串口</param>
        /// <param name="progress">进度</param>
        /// <returns>操作序号</returns>
        public static Handler Initialize(string port, out double progress)
            => Win32
                   ? SafeNativeMethodsWin32.Initialize(port, out progress)
                   : SafeNativeMethodsUnix.Initialize(port, out progress);

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns>操作序号</returns>
        public static Handler Shutdown()
            => Win32
                   ? SafeNativeMethodsWin32.Shutdown()
                   : SafeNativeMethodsUnix.Shutdown();

        /// <summary>
        /// 获取参数默认值
        /// </summary>
        /// <param name="id">参数类型 id</param>
        /// <returns>参数默认值</returns>
        public static double GetDefaultParameter(Handler id)
            => Win32
                   ? SafeNativeMethodsWin32.GetDefaultParameter(id)
                   : SafeNativeMethodsUnix.GetDefaultParameter(id);

        /// <summary>
        /// 获取参数当前值
        /// </summary>
        /// <param name="id">参数类型 id</param>
        /// <param name="value">参数当前值</param>
        /// <returns>操作序号</returns>
        public static Handler GetParameter(Handler id, out double value)
            => Win32
                   ? SafeNativeMethodsWin32.GetParameter(id, out value)
                   : SafeNativeMethodsUnix.GetParameter(id, out value);

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="id">参数类型 id</param>
        /// <param name="value">参数值</param>
        /// <returns>操作序号</returns>
        public static Handler SetParameter(Handler id, double value)
            => Win32
                   ? SafeNativeMethodsWin32.SetParameter(id, value)
                   : SafeNativeMethodsUnix.SetParameter(id, value);

        /// <summary>
        /// 重置参数值到默认值
        /// </summary>
        /// <param name="id">参数类型 id</param>
        /// <returns>操作序号</returns>
        public static Handler ResetParameter(Handler id)
            => Win32
                   ? SafeNativeMethodsWin32.ResetParameter(id)
                   : SafeNativeMethodsUnix.ResetParameter(id);

        /// <summary>
        /// 获取电池电量百分比
        /// </summary>
        /// <param name="value">电量百分比值</param>
        /// <returns>操作序号</returns>
        public static Handler GetBatteryPercent(out double value)
            => Win32
                   ? SafeNativeMethodsWin32.GetBatteryPercent(out value)
                   : SafeNativeMethodsUnix.GetBatteryPercent(out value);

        /// <summary>
        /// 获取后轮方向角
        /// </summary>
        /// <param name="value">后轮方向角（弧度）</param>
        /// <returns>操作序号</returns>
        public static Handler GetRudder(out double value)
            => Win32
                   ? SafeNativeMethodsWin32.GetRudder(out value)
                   : SafeNativeMethodsUnix.GetRudder(out value);

        public static Handler GetOdometry(
            out double stamp,
            out double s, out double sa,
            out double x, out double y, out double theta)
            => Win32
                   ? SafeNativeMethodsWin32.GetOdometry(
                       out stamp,
                       out s, out sa,
                       out x, out y, out theta)
                   : SafeNativeMethodsUnix.GetOdometry(
                       out stamp,
                       out s, out sa,
                       out x, out y, out theta);

        public static Handler ResetOdometry()
            => Win32
                   ? SafeNativeMethodsWin32.ResetOdometry()
                   : SafeNativeMethodsUnix.ResetOdometry();

        public static Handler SetCommandEnabled(bool value)
            => Win32
                   ? SafeNativeMethodsWin32.SetCommandEnabled(value)
                   : SafeNativeMethodsUnix.SetCommandEnabled(value);

        public static Handler SetEnabled(bool value)
            => Win32
                   ? SafeNativeMethodsWin32.SetEnabled(value)
                   : SafeNativeMethodsUnix.SetEnabled(value);

        public static byte CheckState()
            => Win32
                   ? SafeNativeMethodsWin32.CheckState()
                   : SafeNativeMethodsUnix.CheckState();

        public static Handler DrivePhysical(double speed, double rudder)
            => Win32
                   ? SafeNativeMethodsWin32.DrivePhysical(speed, rudder)
                   : SafeNativeMethodsUnix.DrivePhysical(speed, rudder);

        public static Handler DriveWheels(double left, double right)
            => Win32
                   ? SafeNativeMethodsWin32.DriveWheels(left, right)
                   : SafeNativeMethodsUnix.DriveWheels(left, right);

        public static Handler DriveVelocity(double v, double w)
            => Win32
                   ? SafeNativeMethodsWin32.DriveVelocity(v, w)
                   : SafeNativeMethodsUnix.DriveVelocity(v, w);

        public static double CalculateSpatium(double spatium, double angle, double width)
            => Win32
                   ? SafeNativeMethodsWin32.CalculateSpatium(spatium, angle, width)
                   : SafeNativeMethodsUnix.CalculateSpatium(spatium, angle, width);

        public static Handler DriveSpatial(
            double     v,
            double     w,
            double     spatium,
            double     angle,
            out double progress)
            => Win32
                   ? SafeNativeMethodsWin32.DriveSpatial(v, w, spatium, angle, out progress)
                   : SafeNativeMethodsUnix.DriveSpatial(v, w, spatium, angle, out progress);

        public static Handler DriveTiming(
            double     v,
            double     w,
            double     time,
            out double progress)
            => Win32
                   ? SafeNativeMethodsWin32.DriveTiming(v, w, time, out progress)
                   : SafeNativeMethodsUnix.DriveTiming(v, w, time, out progress);

        public static Handler AdjustRudder(double offset, out double progress)
            => Win32
                   ? SafeNativeMethodsWin32.AdjustRudder(offset, out progress)
                   : SafeNativeMethodsUnix.AdjustRudder(offset, out progress);

        public static void SetPaused(bool paused) {
            if (Win32)
                SafeNativeMethodsWin32.SetPaused(paused);
            else
                SafeNativeMethodsUnix.SetPaused(paused);
        }

        public static bool IsPaused()
            => Win32
                   ? SafeNativeMethodsWin32.IsPaused()
                   : SafeNativeMethodsUnix.IsPaused();

        public static void CancelAction() {
            if (Win32)
                SafeNativeMethodsWin32.CancelAction();
            else
                SafeNativeMethodsUnix.CancelAction();
        }

        private static class SafeNativeMethodsWin32 {
            public const string LIBRARY = "pm1_sdk_native.dll";

            [DllImport(LIBRARY, EntryPoint = "get_error_info")]
            public static extern IntPtr GetErrorInfo(Handler handler);

            [DllImport(LIBRARY, EntryPoint = "remove_error_info")]
            public static extern void RemoveErrorInfo(Handler handler);

            [DllImport(LIBRARY, EntryPoint = "clear_error_info")]
            public static extern void ClearErrorInfo();

            [DllImport(LIBRARY, EntryPoint = "get_connected_port")]
            public static extern IntPtr GetConnectedPort();

            [DllImport(LIBRARY, EntryPoint = "initialize", CharSet = CharSet.Ansi)]
            public static extern Handler Initialize(string port, out double progress);

            [DllImport(LIBRARY, EntryPoint = "shutdown")]
            public static extern Handler Shutdown();

            [DllImport(LIBRARY, EntryPoint = "get_default_parameter")]
            public static extern double GetDefaultParameter(Handler id);

            [DllImport(LIBRARY, EntryPoint = "get_parameter")]
            public static extern Handler GetParameter(Handler id, out double value);

            [DllImport(LIBRARY, EntryPoint = "set_parameter")]
            public static extern Handler SetParameter(Handler id, double value);

            [DllImport(LIBRARY, EntryPoint = "reset_parameter")]
            public static extern Handler ResetParameter(Handler id);

            [DllImport(LIBRARY, EntryPoint = "get_battery_percent")]
            public static extern Handler GetBatteryPercent(out double value);

            [DllImport(LIBRARY, EntryPoint = "get_rudder")]
            public static extern Handler GetRudder(out double value);

            [DllImport(LIBRARY, EntryPoint = "get_odometry")]
            public static extern Handler GetOdometry(
                out double stamp,
                out double s, out double sa,
                out double x, out double y, out double theta);

            [DllImport(LIBRARY, EntryPoint = "reset_odometry")]
            public static extern Handler ResetOdometry();

            [DllImport(LIBRARY, EntryPoint = "set_command_enabled")]
            public static extern Handler SetCommandEnabled(bool value);

            [DllImport(LIBRARY, EntryPoint = "set_enabled")]
            public static extern Handler SetEnabled(bool value);

            [DllImport(LIBRARY, EntryPoint = "check_state")]
            public static extern byte CheckState();

            [DllImport(LIBRARY, EntryPoint = "drive_physical")]
            public static extern Handler DrivePhysical(double speed, double rudder);

            [DllImport(LIBRARY, EntryPoint = "drive_wheels")]
            public static extern Handler DriveWheels(double left, double right);

            [DllImport(LIBRARY, EntryPoint = "drive_velocity")]
            public static extern Handler DriveVelocity(double v, double w);

            [DllImport(LIBRARY, EntryPoint = "calculate_spatium")]
            public static extern double CalculateSpatium(double spatium, double angle, double width);

            [DllImport(LIBRARY, EntryPoint = "drive_spatial")]
            public static extern Handler DriveSpatial(
                double     v,
                double     w,
                double     spatium,
                double     angle,
                out double progress);

            [DllImport(LIBRARY, EntryPoint = "drive_timing")]
            public static extern Handler DriveTiming(
                double     v,
                double     w,
                double     time,
                out double progress);

            [DllImport(LIBRARY, EntryPoint = "adjust_rudder")]
            public static extern Handler AdjustRudder(
                double     offset,
                out double progress);

            [DllImport(LIBRARY, EntryPoint = "set_paused")]
            public static extern void SetPaused(bool paused);

            [DllImport(LIBRARY, EntryPoint = "is_paused")]
            public static extern bool IsPaused();

            [DllImport(LIBRARY, EntryPoint = "cancel_action")]
            public static extern void CancelAction();
        }

        private static class SafeNativeMethodsUnix {
            public const string LIBRARY = "libpm1_sdk_native.so";

            [DllImport(LIBRARY, EntryPoint = "get_error_info")]
            public static extern IntPtr GetErrorInfo(Handler handler);

            [DllImport(LIBRARY, EntryPoint = "remove_error_info")]
            public static extern void RemoveErrorInfo(Handler handler);

            [DllImport(LIBRARY, EntryPoint = "clear_error_info")]
            public static extern void ClearErrorInfo();

            [DllImport(LIBRARY, EntryPoint = "get_connected_port")]
            public static extern IntPtr GetConnectedPort();

            [DllImport(LIBRARY, EntryPoint = "initialize", CharSet = CharSet.Ansi)]
            public static extern Handler Initialize(string port, out double progress);

            [DllImport(LIBRARY, EntryPoint = "shutdown")]
            public static extern Handler Shutdown();

            [DllImport(LIBRARY, EntryPoint = "get_default_parameter")]
            public static extern double GetDefaultParameter(Handler id);

            [DllImport(LIBRARY, EntryPoint = "get_parameter")]
            public static extern Handler GetParameter(Handler id, out double value);

            [DllImport(LIBRARY, EntryPoint = "set_parameter")]
            public static extern Handler SetParameter(Handler id, double value);

            [DllImport(LIBRARY, EntryPoint = "reset_parameter")]
            public static extern Handler ResetParameter(Handler id);

            [DllImport(LIBRARY, EntryPoint = "get_battery_percent")]
            public static extern Handler GetBatteryPercent(out double value);

            [DllImport(LIBRARY, EntryPoint = "get_rudder")]
            public static extern Handler GetRudder(out double value);

            [DllImport(LIBRARY, EntryPoint = "get_odometry")]
            public static extern Handler GetOdometry(
                out double stamp,
                out double s, out double sa,
                out double x, out double y, out double theta);

            [DllImport(LIBRARY, EntryPoint = "reset_odometry")]
            public static extern Handler ResetOdometry();

            [DllImport(LIBRARY, EntryPoint = "set_command_enabled")]
            public static extern Handler SetCommandEnabled(bool value);

            [DllImport(LIBRARY, EntryPoint = "set_enabled")]
            public static extern Handler SetEnabled(bool value);

            [DllImport(LIBRARY, EntryPoint = "check_state")]
            public static extern byte CheckState();

            [DllImport(LIBRARY, EntryPoint = "drive_physical")]
            public static extern Handler DrivePhysical(double speed, double rudder);

            [DllImport(LIBRARY, EntryPoint = "drive_wheels")]
            public static extern Handler DriveWheels(double left, double right);

            [DllImport(LIBRARY, EntryPoint = "drive_velocity")]
            public static extern Handler DriveVelocity(double v, double w);

            [DllImport(LIBRARY, EntryPoint = "calculate_spatium")]
            public static extern double CalculateSpatium(double spatium, double angle, double width);

            [DllImport(LIBRARY, EntryPoint = "drive_spatial")]
            public static extern Handler DriveSpatial(
                double     v,
                double     w,
                double     spatium,
                double     angle,
                out double progress);

            [DllImport(LIBRARY, EntryPoint = "drive_timing")]
            public static extern Handler DriveTiming(
                double     v,
                double     w,
                double     time,
                out double progress);

            [DllImport(LIBRARY, EntryPoint = "adjust_rudder")]
            public static extern Handler AdjustRudder(
                double     offset,
                out double progress);

            [DllImport(LIBRARY, EntryPoint = "set_paused")]
            public static extern void SetPaused(bool paused);

            [DllImport(LIBRARY, EntryPoint = "is_paused")]
            public static extern bool IsPaused();

            [DllImport(LIBRARY, EntryPoint = "cancel_action")]
            public static extern void CancelAction();
        }
    }
}