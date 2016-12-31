using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace IODPUtils.JSON {
    public class JSONObject_WithDirtyFlags : JSONObject {
        public JSONObject_WithDirtyFlags() {
        }
        public JSONObject_WithDirtyFlags(string jsonstr) : base(jsonstr) {
        }
        /// <summary>
        ///     Returns an array of all dirty properties.
        /// </summary>
        [XmlIgnore] public string[] DirtyProperties {
            get { return _dirtyprops.ToArray(); }
        }
        /// <summary>
        ///     If propname is null, clears all dirty flags.  Otherwise, clears only the specified dirty flag.
        /// </summary>
        /// <param name="propname">The property for which you want to set the dirty flag, or null to clear all dirty flags.</param>
        public void ClearDirty(string propname = null) {
            if (string.IsNullOrEmpty(propname)) _dirtyprops.Clear();
            else _dirtyprops.Remove(propname);
        }
        public T get<T>(string propname) {
            if (has(propname)) {
                object o = opt(propname);
                if (o == null || o is JSONNull) return typeof (T) == typeof (string) ? (T) (object) "" : default(T);
                if (typeof (T) == typeof (double)) return (T) (object) double.Parse(o.ToString());
                if (typeof (T) == typeof (int) || typeof (T) == typeof (short) || typeof (T) == typeof (long)) return (T) (object) int.Parse(o.ToString());
                return (T) o;
            }
            if (typeof (T) == typeof (string)) return (T) (object) "";
            return default(T);
        }
        /// <summary>
        /// Gets a boolean value, but uses string values "T" and "F" in the JSONObject.
        /// </summary>
        public override bool getBool(string propname) {
            if (!has(propname)) return false;
            string s = opt(propname) as string;
            if (string.IsNullOrEmpty(s)) return false;
            return s.ToLower().StartsWith("t");
        }
        /// <summary>
        ///     If propname is null, returns true if any properties are dirty. Otherwise returns true if the specified property is
        ///     dirty.
        /// </summary>
        public bool isDirty(string propname = null) {
            if (string.IsNullOrEmpty(propname)) return _dirtyprops.Any();
            return _dirtyprops.Contains(propname);
        }
        /// <summary>
        /// Sets a value in the JSONObject, and sets the property's dirty flag.
        /// Also, stores boolean values as "T" or "F" in the JSONObject.
        /// </summary>
        /// <param name="propname"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override JSONObject put(string propname, object value) {
            SetDirty(propname);
            if (value is bool) return base.put(propname, (bool) value ? "T" : "F");
            return base.put(propname, value);
        }
        /// <summary>
        ///     Sets the dirty flag for the specified property.
        /// </summary>
        /// <param name="propname">The property for which you want to set the dirty flag.</param>
        public void SetDirty(string propname) {
            if (string.IsNullOrEmpty(propname)) return;
            if (!_dirtyprops.Contains(propname)) _dirtyprops.Add(propname);
        }
        private readonly List<string> _dirtyprops = new List<string>();
    }
}