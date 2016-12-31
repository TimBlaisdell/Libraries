using System;
using System.Reflection;

namespace IODPUtils.JSON {
    public abstract class JSONSerializableValue : JSONValue {
        public static T Create<T>(string s) where T : JSONSerializableValue {
            return (T) Activator.CreateInstance(typeof (T), BindingFlags.CreateInstance, null, s);
        }
        protected JSONSerializableValue() {
        }
        public JSONSerializableValue(string s) {
            throw new Exception("This should never be called. Deriving classes must define their own constructor that takes a string.");
        }
        public override string ToString() {
            throw new Exception("This should never be called. Deriving classes must define their own ToString override.");
        }
        public override string PrettyPrint() {
            return ToString();
        }
    }
}