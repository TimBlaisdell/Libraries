namespace IODPUtils.JSON {
    /// <summary>
    ///     JSONBoolValue represents a boolean value in JSONSharp.
    /// </summary>
    public class JSONBoolValue : JSONSerializableValue {
        public JSONBoolValue(string s) {
            bool b;
            int i;
            if (string.IsNullOrEmpty(s)) _value = false;
            else if (bool.TryParse(s, out b)) _value = b;
            else if (int.TryParse(s, out i)) _value = i != 0;
            else if (s.ToUpper().StartsWith("T")) _value = true;
            else _value = false;
        }
        /// <summary>
        ///     Simple public instance constructor that accepts a boolean.
        /// </summary>
        /// <param name="value">boolean value for this instance</param>
        public JSONBoolValue(bool value) {
            _value = value;
        }
        /// <summary>
        ///     Required override of the ToString() method.
        /// </summary>
        /// <returns>boolean value for this instance, as text and lower-cased (either "true" or "false", without quotation marks)</returns>
        public override string ToString() {
            return _value.ToString().ToLower();
        }
        private readonly bool _value;
    }
}