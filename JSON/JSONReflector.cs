using System;
using System.Collections.Generic;
using System.Reflection;

namespace JSON {
    /// <summary>
    ///     JSONReflector provides a convenient way to convert value and reference type objects
    ///     to JSON format through reflection.
    ///     This implementation build JSON around reflected public properties of type int, double,
    ///     double, decimal, byte, string, bool, enum or array.  (Generics and other types may be
    ///     supported at a later time.)
    /// </summary>
    public class JSONReflector : JSONValue {
        /// <summary>
        ///     Public constructor that accepts any object
        /// </summary>
        /// <param name="objValue">object to be reflected/evaluated for JSON conversion</param>
        public JSONReflector(object objValue) {
            Dictionary<JSONStringValue, JSONValue> jsonNameValuePairs = new Dictionary<JSONStringValue, JSONValue>();

            Type type = objValue.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo pi in properties) {
                JSONStringValue jsonParameterName = new JSONStringValue(pi.Name);
                JSONValue jsonParameterValue = GetJSONValue(pi.GetValue(objValue, null));
                if (jsonParameterValue != null) {
                    jsonNameValuePairs.Add(jsonParameterName, jsonParameterValue);
                }
            }

            _jsonObjectCollection = new JSONObjectCollection(jsonNameValuePairs);
        }
        /// <summary>
        ///     Required override of the PrettyPrint() method.
        /// </summary>
        /// <returns>returns the internal JSONObjectCollection PrettyPrint() method</returns>
        public override string PrettyPrint() {
            return _jsonObjectCollection.PrettyPrint();
        }
        /// <summary>
        ///     Required override of the ToString() method.
        /// </summary>
        /// <returns>returns the internal JSONObjectCollection ToString() method</returns>
        public override string ToString() {
            return _jsonObjectCollection.ToString();
        }
        private JSONValue GetJSONValue(object objValue) {
            Type thisType = objValue.GetType();
            JSONValue jsonValue = null;

            if (thisType == typeof(Int32)) {
                jsonValue = new JSONNumberValue(Convert.ToInt32(objValue));
            }
            else if (thisType == typeof(Single)) {
                jsonValue = new JSONNumberValue(Convert.ToSingle(objValue));
            }
            else if (thisType == typeof(Double)) {
                jsonValue = new JSONNumberValue(Convert.ToDouble(objValue));
            }
            else if (thisType == typeof(Decimal)) {
                jsonValue = new JSONNumberValue(Convert.ToDecimal(objValue));
            }
            else if (thisType == typeof(Byte)) {
                jsonValue = new JSONNumberValue(Convert.ToByte(objValue));
            }
            else if (thisType == typeof(String)) {
                jsonValue = new JSONStringValue(Convert.ToString(objValue));
            }
            else if (thisType == typeof(Boolean)) {
                jsonValue = new JSONBoolValue(Convert.ToBoolean(objValue));
            }
            else if (thisType.BaseType == typeof(Enum)) {
                jsonValue = new JSONStringValue(Enum.GetName(thisType, objValue));
            }
            else if (thisType.IsArray) {
                List<JSONValue> jsonValues = new List<JSONValue>();
                Array arrValue = (Array) objValue;
                for (int x = 0; x < arrValue.Length; x++) {
                    JSONValue jsValue = GetJSONValue(arrValue.GetValue(x));
                    jsonValues.Add(jsValue);
                }
                jsonValue = new JSONArrayCollection(jsonValues);
            }
            return jsonValue;
        }
        private readonly JSONObjectCollection _jsonObjectCollection;
    }
}