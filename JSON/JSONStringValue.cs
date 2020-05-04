using System.Collections.Generic;

namespace JSON {
    /// <summary>
    ///     JSONStringValue is a collection of zero or more Unicode characters, wrapped in double quotes,
    ///     using backslash escapes. A character is represented as a single character string. A string
    ///     is very much like a C# string.
    /// </summary>
    public class JSONStringValue : JSONSerializableValue {
        /// <summary>
        ///     Public constructor that accepts a value of type string
        /// </summary>
        /// <param name="value">string value</param>
        public JSONStringValue(string value) {
            _value = value;
        }
        /// <summary>
        ///     Required override of the PrettyPrint() method.
        /// </summary>
        /// <returns>this.ToString()</returns>
        public override string PrettyPrint() {
            return ToString();
        }
        public string ToRawString() {
            return _value;
        }
        /// <summary>
        ///     Required override of the ToString() method.
        /// </summary>
        /// <returns>contained string in JSON-compliant form</returns>
        public override string ToString() {
            return ToJSONString(_value);
        }
        /// <summary>
        ///     Evaluates all characters in a string and returns a new string,
        ///     properly formatted for JSON compliance and bounded by double-quotes.
        /// </summary>
        /// <param name="text">string to be evaluated</param>
        /// <returns>new string, in JSON-compliant form</returns>
        public static string ToJSONString(string text) {
            char[] charArray = text.ToCharArray();
            List<string> output = new List<string>();
            foreach (char c in charArray) {
                if (c == 8) output.Add("\\b"); //Backspace
                else if (c == 9) output.Add("\\t"); //Horizontal tab
                else if (c == 10 || c == 13) output.Add("\\n"); //Newline or CR
                else if (c == 12) output.Add("\\f"); //Formfeed
                else if (c == 34 || c == 44 || c == 47 || c == 92) output.Add("\\" + c); //Double-quotes ("), comma (,), solidus (/), reverse solidus (\)
                else if (c > 31) output.Add(c.ToString());
                //TODO: add support for hexadecimal
            }
            return "\"" + string.Join("", output.ToArray()) + "\"";
        }
        private readonly string _value;
    }
}