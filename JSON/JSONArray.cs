using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace JSON {
    /// <summary>
    ///     <para>
    ///         A JSONArray is an ordered sequence of values. Its external form is a string
    ///         wrapped in square brackets with commas between the values. The internal form
    ///         is an object having get() and opt() methods for accessing the values by
    ///         index, and put() methods for adding or replacing values. The values can be
    ///         any of these types: Boolean, JSONArray, JSONObject, Number, String, or the
    ///         JSONObject.NULL object.
    ///     </para>
    ///     <para>
    ///         The constructor can convert a JSON external form string into an
    ///         internal form Java object. The toString() method creates an external
    ///         form string.
    ///     </para>
    ///     <para>
    ///         A get() method returns a value if one can be found, and throws an exception
    ///         if one cannot be found. An opt() method returns a default value instead of
    ///         throwing an exception, and so is useful for obtaining optional values.
    ///     </para>
    ///     <para>
    ///         The generic get() and opt() methods return an object which you can cast or
    ///         query for type. There are also typed get() and opt() methods that do typing
    ///         checking and type coersion for you.
    ///     </para>
    ///     <para>
    ///         The texts produced by the toString() methods are very strict.
    ///         The constructors are more forgiving in the texts they will accept.
    ///     </para>
    ///     <para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>An extra comma may appear just before the closing bracket.</description>
    ///             </item>
    ///             <item>
    ///                 <description>Strings may be quoted with single quotes.</description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Strings do not need to be quoted at all if they do not contain leading
    ///                     or trailing spaces, and if they do not contain any of these characters:
    ///                     { } [ ] / \ : ,
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>Numbers may have the 0- (octal) or 0x- (hex) prefix.</description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Public Domain 2002 JSON.org
    ///         @author JSON.org
    ///         @version 0.1
    ///     </para>
    ///     Ported to C# by Are Bjolseth, teleplan.no
    ///     TODO:
    ///     1. Implement Custom exceptions
    ///     2. Add indexer JSONObject[i] = object,     and object = JSONObject[i];
    ///     3. Add indexer JSONObject["key"] = object, and object = JSONObject["key"]
    ///     4. Add unit testing
    ///     5. Add log4net
    ///     6. Make get/put methods private, to force use of indexer instead?
    /// </summary>
    /// <remarks>
    ///     OMITTED:
    ///     public JSONArray put(bool val)
    ///     public JSONArray put(double val)
    ///     public JSONArray put(int val)
    ///     public JSONArray put(int index, boolean value)
    ///     public JSONArray put(int index, double value)
    ///     public JSONArray put(int index, int value)
    /// </remarks>
    public class JSONArray : JSONSerializableValue, IEnumerable<object> {
        /// <summary>
        ///     Construct an empty JSONArray
        /// </summary>
        public JSONArray() {
            _myList = new List<object>();
        }
        /// <summary>
        ///     Construct a JSONArray from a string. Allows for comments like this: {/* comment */}
        ///     Also allows for comments without the curly-braces like this: /* comment */
        /// </summary>
        /// <param name="s">A string that begins with '[' and ends with ']'.</param>
        public JSONArray(string s) {
            for (var commentindex = 0; commentindex < JSONObject.CommentStarters.Length; ++commentindex) {
                var start = JSONObject.CommentStarters[commentindex];
                var end = JSONObject.CommentEnders[commentindex];
                while (s.Contains(start)) {
                    var i = s.IndexOf(start, StringComparison.Ordinal);
                    var i2 = s.IndexOf(end, StringComparison.Ordinal);
                    if (i < 0 || i < 0 || i2 <= i) break;
                    s = s.Remove(i, i2 - i + 3);
                }
            }
            lock (_serializer) {
                _myList = _serializer.Deserialize<List<object>>(s);
            }
            Normalize();
        }
        /// <summary>
        ///     Construct a JSONArray from a Collection.
        /// </summary>
        /// <param name="collection">A Collection.</param>
        public JSONArray(ICollection collection) {
            _myList = new List<object>(collection.Cast<object>());
            Normalize();
        }
        /// <summary>
        ///     Construct a JSONArray from a string array.
        /// </summary>
        public JSONArray(string[] strArray) {
            _myList = strArray.Cast<object>().ToList();
            // no need to normalize.  They're all strings.
        }
        /// <summary>
        ///     Private for internal use.  Doesn't do the normalize step -- assumes the list has already been normalized.
        /// </summary>
        protected JSONArray(IEnumerable<object> list) {
            _myList = list.ToList();
        }
        /// <summary>
        /// An event triggered when something changes in this object, such as a call to put.
        /// Handling this event lets you, for example, update a file every time it changes.
        /// USE WITH CARE.  Improper use will slow down your program if an object changes a lot!
        /// </summary>
        public event EventHandler Altered;
        /// <summary>
        /// Creates a new instance that's a deep copy of this JSONArray with all underlying objects copied at all levels (e.g. JSONObjects and JSONArrays within this JSONObject will be deep-copied).
        /// </summary>
        public JSONArray Clone() {
            var newarray = new JSONArray();
            object[] myarray;
            lock (_myList) {
                myarray = _myList.ToArray();
            }
            foreach (var obj in myarray) {
                if (obj is JSONObject) newarray.put(((JSONObject) obj).Clone());
                else if (obj is JSONArray) newarray.putJSONArray(((JSONArray) obj).Clone());
                else newarray.put(obj);
            }
            return newarray;
        }
        /// <summary>
        ///     Alternate to Java get/put method, by using indexer
        /// </summary>
        public object this[int i] {
            get { return opt(i); }
            set {
                //myArrayList[i] = value;
                put(i, value);
            }
        }
        /// <summary>
        ///     Get the length of the JSONArray.
        ///     Using a propery instead of method
        /// </summary>
        public int Count {
            get {
                lock (_myList) return _myList.Count;
            }
        }
        /// <summary>
        ///     Alternativ to Java, getArrayList, by using propery
        /// </summary>
        public IList List => _myList;
        /// <summary>
        ///     Get the ArrayList which is holding the elements of the JSONArray.
        ///     Use the indexer instead!! Added to be true to the orignal Java src
        /// </summary>
        /// <returns>The ArrayList</returns>
        public IList getArrayList() {
            return _myList;
        }
        /// <summary>
        ///     Get the boolean value associated with an index.
        ///     The string values "true" and "false" are converted to boolean.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>The truth</returns>
        public bool getBoolean(int i) {
            object obj = getValue(i);
            if (obj is bool) return (bool) obj;
            string msg = $"JSONArray[{i}]={obj} not a Boolean";
            throw new Exception(msg);
        }
        /// <summary>
        ///     Get the double value associated with an index.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>A double value</returns>
        public double getDouble(int i) {
            object obj = getValue(i);
            if (obj is double) return (double) obj;
            if (obj is string) return Convert.ToDouble(obj);
            string msg = $"JSONArray[{i}]={obj} not a double";
            throw new Exception(msg);
        }
        public IEnumerator<object> GetEnumerator() {
            lock (_myList) return _myList.GetEnumerator();
        }
        /// <summary>
        ///     Get the int value associated with an index.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>The int value</returns>
        public int getInt(int i) {
            object obj = getValue(i);
            if (obj is int) return (int) obj;
            if (obj is string) return Convert.ToInt32(obj);
            string msg = $"JSONArray[{i}]={obj} not a int";
            throw new Exception(msg);
        }
        /// <summary>
        ///     Get the JSONArray associated with an index.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>A JSONArray value</returns>
        public JSONArray getJSONArray(int i) {
            object obj = getValue(i) as JSONArray;
            if (obj != null) return (JSONArray) obj;
            string msg = $"JSONArray[{i}]={getValue(i)} not a JSONArray";
            throw new Exception(msg);
        }
        /// <summary>
        ///     Get the JSONObject associated with an index.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>A JSONObject value</returns>
        public JSONObject getJSONObject(int i) {
            object obj = getValue(i) as JSONObject;
            if (obj != null) return (JSONObject) obj;
            string msg = $"JSONArray[{i}]={getValue(i)} not a JSONObject";
            throw new Exception(msg);
        }
        /// <summary>
        ///     Get the JSONObject associated with an index, and returns it as a derived class.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>A JSONObject value as a derived class.</returns>
        public T getJSONObject<T>(int i) where T : JSONObject, new() {
            var obj = new T();
            obj.Load(getJSONObject(i));
            return obj;
        }
        /// <summary>
        ///     Get the string associated with an index.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>A string value.</returns>
        public string getString(int i) {
            object obj = getValue(i);
            return obj?.ToString();
            //string msg = string.Format("JSONArray[{0}]={1} not a string", i, getValue(i));
            //throw new Exception(msg);
        }
        /// <summary>
        ///     Get the object value associated with an index.
        ///     Use indexer instead!!! Added to be true to the original Java implementation
        /// </summary>
        /// <param name="i">index subscript. The index must be between 0 and length()-1</param>
        /// <returns>An object value.</returns>
        public object getValue(int i) {
            object obj = opt(i);
            if (obj == null) {
                string msg = $"JSONArray[{i}] not found";
                throw new Exception(msg);
            }
            return obj;
        }
        /// <summary>
        ///     Determine if the value is null.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>true if the value at the index is null, or if there is no value.</returns>
        public bool isNull(int i) {
            object obj = opt(i);
            return (obj == null || obj.Equals(null));
        }
        /// <summary>
        ///     Make a string from the contents of this JSONArray. The separator string
        ///     is inserted between each element.
        ///     Warning: This method assumes that the data structure is acyclical.
        /// </summary>
        /// <param name="separator">separator A string that will be inserted between the elements.</param>
        /// <returns>A string.</returns>
        public string join(string separator, bool format = false, int tab = 0) {
            StringBuilder sb = new StringBuilder();
            lock (_myList) {
                for (int i = 0; i < _myList.Count; i++) {
                    if (i > 0) {
                        sb.Append(separator);
                        if (format) {
                            sb.Append(Environment.NewLine);
                            for (int t = 0; t < tab; ++t) sb.Append("\t");
                        }
                    }
                    var obj = _myList[i];
                    if (obj == null) sb.Append("");
                    else if (obj is string) sb.Append(JSONUtils.Enquote((string) obj));
                    else if (obj is int) sb.Append(((int) obj).ToString());
                    else if (obj is JSONObject) sb.Append(((JSONObject) obj).ToString(format, tab));
                    else if (obj is JSONArray) sb.Append(((JSONArray) obj).ToString(format, tab));
                    else sb.Append(obj);
                }
            }
            return sb.ToString();
        }
        /// <summary>
        ///     Get the length of the JSONArray.
        ///     Added to be true to the original Java implementation
        /// </summary>
        /// <returns>Number of JSONObjects in array</returns>
        public int Length() {
            lock (_myList) return _myList.Count;
        }
        /// <summary>
        ///     Get the optional object value associated with an index.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>object at that index.</returns>
        public object opt(int i) {
            lock (_myList) {
                if (i < 0 || i >= _myList.Count) return null; //throw new ArgumentOutOfRangeException(nameof(i), i, "Index out of bounds!");
                return _myList[i];
            }
        }
        /// <summary>
        ///     Get the optional boolean value associated with an index.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>The truth</returns>
        public bool optBoolean(int i) {
            return optBoolean(i, false);
        }
        /// <summary>
        ///     Get the optional boolean value associated with an index.
        ///     It returns the defaultValue if there is no value at that index or if it is not
        ///     a Boolean or the String "true" or "false".
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <param name="defaultValue"></param>
        /// <returns>The truth.</returns>
        public bool optBoolean(int i, bool defaultValue) {
            object obj = opt(i);
            if (obj != null) return (bool) obj;
            return defaultValue;
        }
        /// <summary>
        ///     Get the optional double value associated with an index.
        ///     NaN is returned if the index is not found,
        ///     or if the value is not a number and cannot be converted to a number.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>The double value object</returns>
        public double optDouble(int i) {
            return optDouble(i, double.NaN);
        }
        /// <summary>
        ///     Get the optional double value associated with an index.
        ///     NaN is returned if the index is not found,
        ///     or if the value is not a number and cannot be converted to a number.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <param name="defaultValue"></param>
        /// <returns>The double value object</returns>
        public double optDouble(int i, double defaultValue) {
            object obj = opt(i);
            if (obj != null) {
                if (obj is double) return (double) obj;
                if (obj is string) return Convert.ToDouble(obj);
                string msg = $"JSONArray[{i}]={obj} not a double";
                throw new Exception(msg);
            }
            return defaultValue;
        }
        /// <summary>
        ///     Get the optional int value associated with an index.
        ///     Zero is returned if the index is not found,
        ///     or if the value is not a number and cannot be converted to a number.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>The int value object</returns>
        public int optInt(int i) {
            return optInt(i, 0);
        }
        /// <summary>
        ///     Get the optional int value associated with an index.
        ///     The defaultValue is returned if the index is not found,
        ///     or if the value is not a number and cannot be converted to a number.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <param name="defaultValue">The default value</param>
        /// <returns>The int value object</returns>
        public int optInt(int i, int defaultValue) {
            object obj = opt(i);
            if (obj != null) {
                if (obj is int) return (int) obj;
                if (obj is string) return Convert.ToInt32(obj);
                string msg = $"JSONArray[{i}]={obj} not a int";
                throw new Exception(msg);
            }
            return defaultValue;
        }
        /// <summary>
        ///     Get the optional JSONArray associated with an index.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>A JSONArray value, or null if the index has no value, or if the value is not a JSONArray.</returns>
        public JSONArray optJSONArray(int i) {
            object obj = opt(i) as JSONArray;
            return (JSONArray) obj;
        }
        /// <summary>
        ///     Get the optional JSONObject associated with an index.
        ///     Null is returned if the key is not found, or null if the index has
        ///     no value, or if the value is not a JSONObject.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>A JSONObject value</returns>
        public JSONObject optJSONObject(int i) {
            object obj = opt(i) as JSONObject;
            return (JSONObject) obj;
        }
        /// <summary>
        ///     Get the optional string value associated with an index. It returns an
        ///     empty string if there is no value at that index. If the value
        ///     is not a string and is not null, then it is coverted to a string.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <returns>A String value</returns>
        public string optString(int i) {
            return optString(i, "");
        }
        /// <summary>
        ///     Get the optional string associated with an index.
        ///     The defaultValue is returned if the key is not found.
        /// </summary>
        /// <param name="i">index subscript</param>
        /// <param name="defaultValue">The default value</param>
        /// <returns>A string value</returns>
        public string optString(int i, string defaultValue) {
            object obj = opt(i);
            if (obj != null) return obj.ToString();
            return defaultValue;
        }
        /// <summary>
        ///     Append an object value.
        /// </summary>
        /// <param name="vals">
        ///     One or more object values.  The value should be a Boolean, Double, Integer, JSONArray, JSObject, or String, or
        ///     the JSONObject.NULL object
        /// </param>
        /// <returns>this (JSONArray)</returns>
        public JSONArray put(params object[] vals) {
            lock (_myList)
                foreach (var val in vals)
                    _myList.Add(val);
            Altered?.Invoke(this, new EventArgs());
            return this;
        }
        /// <summary>
        /// Beware!  If passed a JSONArray (or other enumerable), this method will put the contents of the JSONArray into this JSONArray -- it won't add the array itself to this array as an item.
        /// To do that, call putJSONArray().
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        public JSONArray put(IEnumerable<object> vals) {
            foreach (var val in vals) put(val);
            return this;
        }
        /// <summary>
        /// If you want a JSONArray to contain JSONArrays, use this method, rather than the regular put method, to add them.
        /// The regular put method will add the contents of the JSONArray to this JSONArray.
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        public JSONArray putJSONArray(params JSONArray[] vals) {
            lock (_myList)
                foreach (var val in vals)
                    _myList.Add(val);
            Altered?.Invoke(this, new EventArgs());
            return this;
        }
        /// <summary>
        ///     Put or replace a boolean value in the JSONArray.
        /// </summary>
        /// <param name="i">
        ///     The subscript. If the index is greater than the length of
        ///     the JSONArray, then null elements will be added as necessary to pad it out.
        /// </param>
        /// <param name="val">An object value.</param>
        /// <returns>this (JSONArray)</returns>
        public JSONArray put(int i, object val) {
            if (i < 0) throw new ArgumentOutOfRangeException(nameof(i), i, "Negative indexes illegal");
            if (val == null) throw new ArgumentNullException(nameof(val), "Object cannt be null");
            lock (_myList) {
                if (i < _myList.Count) _myList[i] = val;
                // NOTE! Since i is >= Count, fill null vals before index i, then append new object at i
                else {
                    while (i != _myList.Count) _myList.Add(null);
                    _myList.Add(val);
                }
            }
            Altered?.Invoke(this, new EventArgs());
            return this;
        }
        public JSONArray RemoveAt(int i) {
            lock (_myList) {
                if (i < 0 || i >= Count) throw new ArgumentOutOfRangeException(nameof(i), i, "JSONArray.RemoveAt index must be greater than zero and less than Count.");
                _myList.RemoveAt(i);
            }
            Altered?.Invoke(this, new EventArgs());
            return this;
        }
        public JSONArray RemoveWhere<T>(Func<T, bool> func) {
            lock (_myList) {
                _myList.RemoveAll(o => o is T && func((T) o));
            }
            Altered?.Invoke(this, new EventArgs());
            return this;
        }
        /// <summary>
        ///     Splits this JSONArray into two JSONArrays.
        ///     i specifies the point at which to split the array.  The first output array will contain i records or less, the
        ///     second will contain the remainder of the array or be empty if everything fit into the first one.
        /// </summary>
        public void Split(int i, out JSONArray first, out JSONArray remainder) {
            lock (_myList) {
                first = new JSONArray(_myList.Take(i));
                remainder = new JSONArray(_myList.Skip(i));
            }
        }
        public T[] ToArray<T>(string omitIfMissingField = null) where T : JSONObject, new() {
            var list = new List<T>();
            for (int i = 0; i < Length(); ++i) {
                var obj = getJSONObject(i);
                if (omitIfMissingField != null && !obj.has(omitIfMissingField)) continue;
                T t = new T();
                t.Load(obj);
                list.Add(t);
            }
            return list.ToArray();
        }
        /// <summary>
        ///     Assumes this JSONArray is an array of simple string values, and returns a List of those values.
        /// </summary>
        public string[] ToArray() {
            var list = new List<string>();
            for (int i = 0; i < Length(); ++i) list.Add(getString(i));
            return list.ToArray();
        }
        /// <summary>
        ///     Converts this JSONArray to a Dictionary where the key is an int that is retrieved by the keyname, and the values
        ///     are the JSONObjects converted to type T.
        /// </summary>
        public Dictionary<int, T> ToIntDictionary<T>(string keyname) where T : JSONObject, new() {
            var dict = new Dictionary<int, T>();
            for (int i = 0; i < Length(); ++i) {
                T t = new T();
                t.Load(getJSONObject(i));
                var key = t.getInt(keyname);
                if (!dict.ContainsKey(key)) dict.Add(key, t);
            }
            return dict;
        }
        /// <summary>
        ///     Produce a JSONObject by combining a JSONArray of names with the values
        ///     of this JSONArray.
        /// </summary>
        /// <param name="names">
        ///     A JSONArray containing a list of key strings. These will be paired with the values.
        /// </param>
        /// <returns>A JSONObject, or null if there are no names or if this JSONArray</returns>
        public JSONObject toJSONObject(JSONArray names) {
            if (names == null || names.Length() == 0 || Length() == 0) return null;
            JSONObject jo = new JSONObject();
            for (int i = 0; i < names.Length(); i++) jo.put(names.getString(i), opt(i));
            return jo;
        }
        /// <summary>
        ///     Assumes this JSONArray is an array of simple string values, and returns a List of those values.
        /// </summary>
        public List<string> ToList() {
            var list = new List<string>();
            for (int i = 0; i < Length(); ++i) list.Add(getString(i));
            return list;
        }
        public List<T> ToList<T>() where T : JSONObject, new() {
            var list = new List<T>();
            for (int i = 0; i < Length(); ++i) {
                T t = new T();
                t.Load(getJSONObject(i));
                list.Add(t);
            }
            return list;
        }
        /// <summary>
        ///     Make an JSON external form string of this JSONArray. For compactness, no
        ///     unnecessary whitespace is added.
        /// </summary>
        /// <returns>a printable, displayable, transmittable representation of the array.</returns>
        public override string ToString() {
            return ToString(false, 0);
        }
        public string ToString(bool format, int tab) {
            bool splitThingsUp = false;
            if (format) {
                lock (_myList) splitThingsUp = _myList.Any(o => o is JSONObject || o is JSONArray);
                ++tab;
            }
            return '[' + join(", ", format && splitThingsUp, tab) + ']';
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        /// <summary>
        ///     Replaces any items of type Dictionary&lt;string, object&gt; with a JSONObject.
        /// </summary>
        private void Normalize() {
            bool changed = false;
            lock (_myList) {
                for (int i = 0; i < _myList.Count; ++i) {
                    object o = _myList[i];
                    if (o is Dictionary<string, object>) {
                        _myList[i] = new JSONObject((Dictionary<string, object>) o);
                        changed = true;
                    }
                    else if (o is ArrayList) {
                        _myList[i] = new JSONArray((ArrayList) o);
                        changed = true;
                    }
                    else if (o is Array) {
                        _myList[i] = new JSONArray((Array) o);
                        changed = true;
                    }
                }
            }
            if (changed) Altered?.Invoke(this, new EventArgs());
        }
        /// <summary>
        ///     Creates a JSONArray of strings from an array of strings.
        /// </summary>
        public static JSONArray From(IEnumerable<string> strings) {
            var json = new JSONArray();
            if (strings != null)
                foreach (var str in strings)
                    json.put(str);
            return json;
        }
        /// <summary>The ArrayList where the JSONArray's properties are kept.</summary>
        private readonly List<object> _myList;
        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer {MaxJsonLength = int.MaxValue, RecursionLimit = int.MaxValue};
    }
}