using System;
using System.Globalization;

namespace IODPUtils.JSON {
    /// <summary>
    ///     JSONNumberValue is very much like a C# number, except that octal and hexadecimal formats
    ///     are not used.
    /// </summary>
    public class JSONNumberValue : JSONSerializableValue {
        static JSONNumberValue() {
            JavaScriptNumberFormatInfo = new NumberFormatInfo {NumberDecimalSeparator = "."};
        }
        internal JSONNumberValue(string value) {
            _value = value;
        }
        /// <summary>
        ///     Public constructor that accepts a value of type int
        /// </summary>
        /// <param name="value">int (System.Int32) value</param>
        public JSONNumberValue(int value) : this(value.ToString()) {
        }
        /// <summary>
        ///     Public constructor that accepts a value of type double
        /// </summary>
        /// <param name="value">double (System.Double) value</param>
        public JSONNumberValue(double value) : this(value.ToString(JavaScriptNumberFormatInfo)) {
        }
        /// <summary>
        ///     Public constructor that accepts a value of type decimal
        /// </summary>
        /// <param name="value">decimal (System.Decimal) value</param>
        public JSONNumberValue(decimal value) : this(value.ToString(JavaScriptNumberFormatInfo)) {
        }
        /// <summary>
        ///     Public constructor that accepts a value of type single
        /// </summary>
        /// <param name="value">single (System.Single) value</param>
        public JSONNumberValue(Single value) : this(value.ToString("E", JavaScriptNumberFormatInfo)) {
        }
        /// <summary>
        ///     Public constructor that accepts a value of type byte
        /// </summary>
        /// <param name="value">byte (System.Byte) value</param>
        public JSONNumberValue(byte value) : this(value.ToString()) {
        }
        /// <summary>
        ///     Required override of ToString() method.
        /// </summary>
        /// <returns>contained numeric value, rendered as a string</returns>
        public override string ToString() {
            return _value;
        }
        private readonly string _value;
        /// <summary>
        ///     Number formatting object for handling globalization differences with decimal point separators
        /// </summary>
        protected static NumberFormatInfo JavaScriptNumberFormatInfo;
    }
}