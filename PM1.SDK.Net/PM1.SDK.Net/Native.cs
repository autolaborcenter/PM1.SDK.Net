using System;
using System.Runtime.InteropServices;
using Handler = System.UInt32;

namespace Autolabor.PM1 {
    public static class SafeNativeMethods {
#if DEBUG
        const string LIBRARY = "pm1_sdk_native_injection_debug.dll";
#else
        const string LIBRARY = "pm1_sdk_native.dll";
#endif

        [DllImport(LIBRARY, EntryPoint = "get_error_info")]
        public static extern IntPtr GetErrorInfo(Handler handler);

        [DllImport(LIBRARY, EntryPoint = "remove_error_info")]
        public static extern void RemoveErrorInfo(Handler handler);

        [DllImport(LIBRARY, EntryPoint = "clear_error_info")]
        public static extern void ClearErrorInfo();

        [DllImport(LIBRARY, EntryPoint = "get_current_port")]
        public static extern IntPtr GetConnectedPort();

        [DllImport(LIBRARY, EntryPoint = "get_default_chassis_config")]
        public static extern void GetDefaultChassisConfig(
            out double width,
            out double length,
            out double wheelRadius,
            out double optimizeWidth,
            out double acceleration);

        [DllImport(LIBRARY, EntryPoint = "initialize", CharSet = CharSet.Ansi)]
        public static extern Handler Initialize(
            string port,
            double width,
            double length,
            double wheelRadius,
            double optimizeWidth,
            double acceleration,
            out double progress);

        [DllImport(LIBRARY, EntryPoint = "shutdown")]
        public static extern Handler Shutdown();

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

        [DllImport(LIBRARY, EntryPoint = "drive")]
        public static extern Handler Drive(double v, double w);

        [DllImport(LIBRARY, EntryPoint = "spatium_calculate")]
        public static extern double SpatiumCalculate(double spatium, double angle);

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

        [DllImport(LIBRARY, EntryPoint = "pause")]
        public static extern void Pause();

        [DllImport(LIBRARY, EntryPoint = "resume")]
        public static extern void Resume();

        [DllImport(LIBRARY, EntryPoint = "is_paused")]
        public static extern bool IsPaused();

        [DllImport(LIBRARY, EntryPoint = "cancel_all")]
        public static extern void CancelAll();
    }
}
