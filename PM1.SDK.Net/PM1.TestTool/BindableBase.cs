using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Autolabor.PM1.TestTool {
    /// <inheritdoc />
    /// <summary>
    ///     可绑定对象
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        ///     发布属性变化通知
        /// </summary>
        /// <param name="propertyName">属性名字</param>
        protected void Notify(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        ///     设置属性
        /// </summary>
        /// <param name="field">后台字段</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称</param>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>是否发生更新</returns>
        protected bool SetProperty<T>(ref T field,
                                      T value,
                                      [CallerMemberName] string propertyName = null) {
            if (Equals(field, value)) return false;
            field = value;
            Notify(propertyName);
            return true;
        }

        protected bool SetProperty<T>(T field,
                                      T value,
                                      Action<T> setter,
                                      [CallerMemberName] string propertyName = null) {
            if (Equals(field, value)) return false;
            setter(value);
            Notify(propertyName);
            return true;
        }
    }
}
