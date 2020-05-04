using System;
using System.Reflection;

namespace JSON {
    [Serializable] public abstract class JSONSerializableValue : JSONValue {
        protected JSONSerializableValue() {
        }
        protected JSONSerializableValue(string s) {
            throw new Exception("This should never be called. Deriving classes must define their own constructor that takes a string.");
        }
        public override string PrettyPrint() {
            return ToString();
        }
        public override string ToString() {
            throw new Exception("This should never be called. Deriving classes must define their own ToString override.");
        }
        public static T Create<T>(string s) where T : JSONSerializableValue {
            return (T) Activator.CreateInstance(typeof(T), BindingFlags.CreateInstance, null, s);
        }
    }
}