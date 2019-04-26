using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Autolabor.PM1.SafeNativeMethods;

namespace Autolabor.PM1 {
    /// <summary>
    /// 提供对底盘参数的访问。
    /// </summary>
    public sealed class Parameter {
        private readonly uint _id;

        internal Parameter(Parameters.IdEnum id) => _id = (uint)id;

        /// <summary>
        /// 获取参数默认值
        /// </summary>
        public double Default => GetDefaultParameter(_id);

        /// <summary>
        /// 获取或设置参数值
        /// </summary>
        public double? Value {
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
    /// 索引所有底盘参数对象。
    /// </summary>
    public sealed class Parameters {
        /// <summary>
        /// 存储枚举与对应的参数对象。
        /// </summary>
        public static readonly IReadOnlyDictionary<IdEnum, Parameter>
            Dictionary = new Dictionary<IdEnum, Parameter>{
                {IdEnum.Width,         new Parameter(IdEnum.Width)},
                {IdEnum.Length,        new Parameter(IdEnum.Length)},
                {IdEnum.WheelRadius,   new Parameter(IdEnum.WheelRadius)},
                {IdEnum.OptimizeWidth, new Parameter(IdEnum.OptimizeWidth)},
                {IdEnum.Acceleration,  new Parameter(IdEnum.Acceleration)},
                {IdEnum.MaxV,          new Parameter(IdEnum.MaxV)},
                {IdEnum.MaxW,          new Parameter(IdEnum.MaxW)}};

        /// <summary>
        /// 指定参数的标识符。
        /// </summary>
        public enum IdEnum : uint {
            /// <summary>
            ///     宽度，两动力轮触地点的间距。
            /// </summary>
            Width,

            /// <summary>
            ///     长度，动力轮轴与转向轮轴转向轴的距离。
            /// </summary>
            Length,

            /// <summary>
            ///     动力轮的半径。
            /// </summary>
            WheelRadius,

            /// <summary>
            ///     优化函数的半宽度。
            /// </summary>
            OptimizeWidth,

            /// <summary>
            ///     动力轮最大角加速度。
            /// </summary>
            Acceleration,

            /// <summary>
            ///     底盘最大线速度。
            /// </summary>
            MaxV,

            /// <summary>
            ///     底盘最大角速度。
            /// </summary>
            MaxW
        }

        internal Parameters() { }

        /// <summary>
        /// 获取枚举项对应的参数对象。
        /// </summary>
        /// <param name="key">枚举项</param>
        /// <returns>
        /// 参数对象。
        /// </returns>
        public Parameter this[IdEnum key] => Dictionary[key];

        /// <summary>
        /// 获取名字对应的参数对象。
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns>
        /// 参数对象。
        /// </returns>
        public Parameter this[string name] {
            get {
                switch (name) {
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
