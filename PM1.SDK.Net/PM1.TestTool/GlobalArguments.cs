using System;
using System.Windows;

namespace Autolabor.PM1.TestTool {
    public class GlobalParameter<T> where T:struct {
        public readonly string Name;

        public GlobalParameter(string name) => Name = name;

        public T? Value {
            get {
                try {
                    return (T?)Application.Current.Properties[Name];
                } catch (InvalidCastException) {
                    return null;
                }
            }
            set {
                if (value.HasValue)
                    Application.Current.Properties[Name] = value.Value;
                else
                    Application.Current.Properties.Remove(Name);
            }
        }
    }
}
