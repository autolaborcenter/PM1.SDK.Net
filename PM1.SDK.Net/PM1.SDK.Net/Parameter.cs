using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Autolabor.PM1.SafeNativeMethods;

namespace Autolabor.PM1 {
    /// <summary>
    ///     参数
    /// </summary>
    public class Parameter {
        private readonly uint _id;

        internal Parameter(Parameters.IdEnum id) => _id = (uint)id;

        public double Default => GetDefaultParameter(_id);

        public double? Current {
            get {
                var handler = GetParameter(_id, out var value);
                var error = Marshal.PtrToStringAnsi(GetErrorInfo(handler));
                if (!string.IsNullOrWhiteSpace(error)) {
                    RemoveErrorInfo(handler);
                    return null;
                }
                return value;
            }
            set => OnNative(value.HasValue
                            ? SetParameter(_id, value.Value)
                            : ResetParameter(_id));
        }
    }

    /// <summary>
    ///     参数容器
    /// </summary>
    public class Parameters {
        public static readonly IDictionary<IdEnum, Parameter>
            Dictionary = new Dictionary<IdEnum, Parameter>{
                {IdEnum.Width,         new Parameter(IdEnum.Width)},
                {IdEnum.Length,        new Parameter(IdEnum.Length)},
                {IdEnum.WheelRadius,   new Parameter(IdEnum.WheelRadius)},
                {IdEnum.OptimizeWidth, new Parameter(IdEnum.OptimizeWidth)},
                {IdEnum.Acceleration,  new Parameter(IdEnum.Acceleration)},
                {IdEnum.MaxV,          new Parameter(IdEnum.MaxV)},
                {IdEnum.MaxW,          new Parameter(IdEnum.MaxW)}};

        public enum IdEnum : uint {
            /// <summary>
            ///     轮间距
            /// </summary>
            Width,

            /// <summary>
            ///     轴间距
            /// </summary>
            Length,

            /// <summary>
            ///     轮半径
            /// </summary>
            WheelRadius,

            /// <summary>
            ///     优化宽度
            /// </summary>
            OptimizeWidth,

            /// <summary>
            ///     最大加速度
            /// </summary>
            Acceleration,

            /// <summary>
            ///     最大线速度
            /// </summary>
            MaxV,

            /// <summary>
            ///     最大角速度
            /// </summary>
            MaxW
        }

        internal Parameters() { }

        public Parameter this[IdEnum key] => Dictionary[key];

        public Parameter this[string key] {
            get {
                switch (key) {
                    case nameof(IdEnum.Width): return Dictionary[IdEnum.Width];
                    case nameof(IdEnum.Length): return Dictionary[IdEnum.Length];
                    case nameof(IdEnum.WheelRadius): return Dictionary[IdEnum.WheelRadius];
                    case nameof(IdEnum.OptimizeWidth): return Dictionary[IdEnum.OptimizeWidth];
                    case nameof(IdEnum.Acceleration): return Dictionary[IdEnum.Acceleration];
                    case nameof(IdEnum.MaxV): return Dictionary[IdEnum.MaxV];
                    case nameof(IdEnum.MaxW): return Dictionary[IdEnum.MaxW];
                    default: return null;
                }
            }
        }
    }
}
