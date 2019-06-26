using System;
using System.Runtime.InteropServices;
using Handler = System.UInt32;

namespace Autolabor.PM1 {

    internal static class SafeNativeMethods
    {
        public static readonly bool Win32 = Environment.OSVersion.Platform == PlatformID.Win32NT;
        /// <summary>
        ///     代理底层 API，错误信息转化为日常
        /// </summary>
        /// <param name="handler">任务 id</param>
        public static void OnNative(uint handler)
        {
            var error = Marshal.PtrToStringAnsi(GetErrorInfo(handler));
            if (!string.IsNullOrWhiteSpace(error))
            {
                RemoveErrorInfo(handler);
                throw new Exception(error);
            }
        }

        public static IntPtr GetErrorInfo(Handler handler)
            => Win32 ? SafeNativeMethodsWin32.GetErrorInfo(handler)
                     : SafeNativeMethodsUnix.GetErrorInfo(handler);

        public static void RemoveErrorInfo(Handler handler)
        {
            if (Win32) SafeNativeMethodsWin32.RemoveErrorInfo(handler);
            else SafeNativeMethodsUnix.RemoveErrorInfo(handler);
        }

        public static void ClearErrorInfo()
        {
            if (Win32) SafeNativeMethodsWin32.ClearErrorInfo();
            else SafeNativeMethodsUnix.ClearErrorInfo();
        }

        public static IntPtr GetConnectedPort()
             => Win32 ? SafeNativeMethodsWin32.GetConnectedPort()
                      : SafeNativeMethodsUnix.GetConnectedPort();

        public static Handler Initialize(string port, out double progress)
              => Win32 ? SafeNativeMethodsWin32.Initialize(port, out progress)
                       : SafeNativeMethodsUnix.Initialize(port, out progress);

        public static Handler Shutdown()
              => Win32 ? SafeNativeMethodsWin32.Shutdown()
                       : SafeNativeMethodsUnix.Shutdown();

        public static double GetDefaultParameter(Handler id)
              => Win32 ? SafeNativeMethodsWin32.GetDefaultParameter(id)
                       : SafeNativeMethodsUnix.GetDefaultParameter(id);

        public static Handler GetParameter(Handler id, out double value)
              => Win32 ? SafeNativeMethodsWin32.GetParameter(id, out value)
                       : SafeNativeMethodsUnix.GetParameter(id, out value);

        public static Handler SetParameter(Handler id, double value)
              => Win32 ? SafeNativeMethodsWin32.SetParameter(id, value)
                       : SafeNativeMethodsUnix.SetParameter(id, value);

        public static Handler ResetParameter(Handler id)
              => Win32 ? SafeNativeMethodsWin32.ResetParameter(id)
                       : SafeNativeMethodsUnix.ResetParameter(id);

        public static Handler GetOdometry(
            out double s, out double sa,
            out double x, out double y, out double theta,
            out double vx, out double vy, out double w)
              => Win32 ? SafeNativeMethodsWin32.GetOdometry(
                             out s, out sa,
                             out x, out y, out theta,
                             out vx, out vy, out w)
                       : SafeNativeMethodsUnix.GetOdometry(
                             out s, out sa,
                             out x, out y, out theta,
                             out vx, out vy, out w);

        public static Handler ResetOdometry()
              => Win32 ? SafeNativeMethodsWin32.ResetOdometry()
                       : SafeNativeMethodsUnix.ResetOdometry();

        public static Handler Lock()
              => Win32 ? SafeNativeMethodsWin32.Lock()
                       : SafeNativeMethodsUnix.Lock();

        public static Handler Unlock()
              => Win32 ? SafeNativeMethodsWin32.Unlock()
                       : SafeNativeMethodsUnix.Unlock();

        public static byte CheckState()
              => Win32 ? SafeNativeMethodsWin32.CheckState()
                       : SafeNativeMethodsUnix.CheckState();

        public static Handler DrivePhysical(double speed, double rudder)
              => Win32 ? SafeNativeMethodsWin32.DrivePhysical(speed, rudder)
                       : SafeNativeMethodsUnix.DrivePhysical(speed, rudder);

        public static Handler DriveWheels(double left, double right)
              => Win32 ? SafeNativeMethodsWin32.DriveWheels(left, right)
                       : SafeNativeMethodsUnix.DriveWheels(left, right);

        public static Handler DriveVelocity(double v, double w)
              => Win32 ? SafeNativeMethodsWin32.DriveVelocity(v, w)
                       : SafeNativeMethodsUnix.DriveVelocity(v, w);

        public static double CalculateSpatium(double spatium, double angle, double width)
              => Win32 ? SafeNativeMethodsWin32.CalculateSpatium(spatium, angle, width)
                       : SafeNativeMethodsUnix.CalculateSpatium(spatium, angle, width);

        public static Handler DriveSpatial(
            double v,
            double w,
            double spatium,
            double angle,
            out double progress)
              => Win32 ? SafeNativeMethodsWin32.DriveSpatial(v, w, spatium, angle, out progress)
                       : SafeNativeMethodsUnix.DriveSpatial(v, w, spatium, angle, out progress);

        public static Handler DriveTiming(
            double v,
            double w,
            double time,
            out double progress)
              => Win32 ? SafeNativeMethodsWin32.DriveTiming(v, w, time, out progress)
                       : SafeNativeMethodsUnix.DriveTiming(v, w, time, out progress);

        public static Handler AdjustRudder(double offset, out double progress)
              => Win32 ? SafeNativeMethodsWin32.AdjustRudder(offset, out progress)
                       : SafeNativeMethodsUnix.AdjustRudder(offset, out progress);

        public static void SetPaused(bool paused)
        {
            if (Win32) SafeNativeMethodsWin32.SetPaused(paused);
            else SafeNativeMethodsUnix.SetPaused(paused);
        }

        public static bool IsPaused()
              => Win32 ? SafeNativeMethodsWin32.IsPaused()
                       : SafeNativeMethodsUnix.IsPaused();

        public static void CancelAction()
        {
            if (Win32) SafeNativeMethodsWin32.CancelAction();
            else SafeNativeMethodsUnix.CancelAction();
        }

        private static class SafeNativeMethodsWin32
        {
            public const string LIBRARY = "pm1_sdk_native.dll";

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
            public static extern double CalculateSpatium(double spatium, double angle, double width);

            [DllImport(LIBRARY, EntryPoint = "drive_spatial")]
            public static extern Handler DriveSpatial(
                double v,
                double w,
                double spatium,
                double angle,
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

        private static class SafeNativeMethodsUnix
        {
            public const string LIBRARY = "libpm1_sdk_native.so";

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
            public static extern double CalculateSpatium(double spatium, double angle, double width);

            [DllImport(LIBRARY, EntryPoint = "drive_spatial")]
            public static extern Handler DriveSpatial(
                double v,
                double w,
                double spatium,
                double angle,
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
}
