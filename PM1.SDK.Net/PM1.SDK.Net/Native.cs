using System;
using System.Runtime.InteropServices;
using Handler = System.UInt32;

namespace Autolabor.PM1 {
    internal static class SafeNativeMethods {
#if DEBUG
        public const string LIBRARY = "pm1_sdk_native_debug.dll";
#else
        public const string LIBRARY = "pm1_sdk_native.dll";
#endif

        /// <summary>
        ///     代理底层 API，错误信息转化为日常
        /// </summary>
        /// <param name="handler">任务 id</param>
        public static void OnNative(uint handler) {
            var error = Marshal.PtrToStringAnsi(GetErrorInfo(handler));
            if (!string.IsNullOrWhiteSpace(error)) {
                RemoveErrorInfo(handler);
                throw new Exception(error);
            }
        }

        [DllImport(LIBRARY, EntryPoint = "get_error_info")]
        public static extern IntPtr GetErrorInfo(Handler handler);

        [DllImport(LIBRARY, EntryPoint = "remove_error_info")]
        public static extern void RemoveErrorInfo(Handler handler);

        [DllImport(LIBRARY, EntryPoint = "clear_error_info")]
        public static extern void ClearErrorInfo();

        [DllImport(LIBRARY, EntryPoint = "get_current_port")]
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

        [DllImport(LIBRARY, EntryPoint = "get_odometry")]
        public static extern Handler GetOdometry(
            out double s, out double sa,
            out double x, out double y, out double theta,
            out double vx, out double vy, out double w);

        [DllImport(LIBRARY, EntryPoint = "reset_odometry")]
        public static extern Handler ResetOdometry();

        [DllImport(LIBRARY, EntryPoint = "lock")]
        public static extern Handler Lock();

        [DllImport(LIBRARY, EntryPoint = "unlock")]
        public static extern Handler Unlock();

        [DllImport(LIBRARY, EntryPoint = "check_state")]
        public static extern byte CheckState();

        [DllImport(LIBRARY, EntryPoint = "drive_physical")]
        public static extern Handler DrivePhysical(double speed, double rudder);

        [DllImport(LIBRARY, EntryPoint = "drive_wheels")]
        public static extern Handler DriveWheels(double left, double right);

        [DllImport(LIBRARY, EntryPoint = "drive_velocity")]
        public static extern Handler DriveVelocity(double v, double w);

        [DllImport(LIBRARY, EntryPoint = "calculate_spatium")]
        public static extern double CalculateSpatium(double spatium, double angle);

        [DllImport(LIBRARY, EntryPoint = "drive_spatial")]
        public static extern Handler DriveSpatial(
            double v,
            double w,
            double spatium,
            out double progress);

        [DllImport(LIBRARY, EntryPoint = "drive_timing")]
        public static extern Handler DriveTiming(
            double v,
            double w,
            double time,
            out double progress);

        [DllImport(LIBRARY, EntryPoint = "adjust_rudder")]
        public static extern Handler AdjustRudder(
            double offset,
            out double progress);

        [DllImport(LIBRARY, EntryPoint = "set_paused")]
        public static extern void SetPaused(bool paused);

        [DllImport(LIBRARY, EntryPoint = "is_paused")]
        public static extern bool IsPaused();

        [DllImport(LIBRARY, EntryPoint = "cancel_action")]
        public static extern void CancelAction();
    }
}
