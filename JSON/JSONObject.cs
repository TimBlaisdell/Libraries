using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

/*
 * A JSONObject is an unordered collection of name/value pairs. Its
 * external form is a string wrapped in curly braces with colons between the
 * names and values, and commas between the values and names. The internal form
 * is an object having get() and opt() methods for accessing the values by name,
 * and put() methods for adding or replacing values by name. The values can be
 * any of these types: Boolean, JSONArray, JSONObject, Number, String, or the
 * JSONObject.NULL object.
 * <p>
 * The constructor can convert an external form string into an internal form
 * Java object. The toString() method creates an external form string.
 * <p>
 * A get() method returns a value if one can be found, and throws an exception
 * if one cannot be found. An opt() method returns a default value instead of
 * throwing an exception, and so is useful for obtaining optional values.
 * <p>
 * The generic get() and opt() methods return an object, which you can cast or
 * query for type. There are also typed get() and opt() methods that do typing
 * checking and type coersion for you.
 * <p>
 * The texts produced by the toString() methods are very strict.
 * The constructors are more forgiving in the texts they will accept.
 * <ul>
 * <li>An extra comma may appear just before the closing brace.</li>
 * <li>Strings may be quoted with single quotes.</li>
 * <li>Strings do not need to be quoted at all if they do not contain leading
 *     or trailing spaces, and if they do not contain any of these characters:
 *     { } [ ] / \ : , </li>
 * <li>Numbers may have the 0- (octal) or 0x- (hex) prefix.</li>
 * </ul>
 * <p>
 * Public Domain 2002 JSON.org
 * @author JSON.org
 * @version 0.1
 * <p>
 * Ported to C# by Are Bjolseth, teleplan.no
 * TODO:
 * 1. Implement Custom exceptions
 * 2. Add indexer JSONObject[i] = object,     and object = JSONObject[i];
 * 3. Add indexer JSONObject["key"] = object, and object = JSONObject["key"]
 * 4. Add unit testing
 * 5. Add log4net
 */

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
    [Serializable] public class JSONObject : JSONSerializableValue, IEnumerable {
        /// <summary>
        ///     Construct an empty JSONObject.
        /// </summary>
        public JSONObject() {
            _myDict = new ConcurrentDictionary<string, object>();
            _myKeyList = new List<string>();
        }
        public JSONObject(IDictionary<string, object> dict) {
            _myDict = dict == null ? new ConcurrentDictionary<string, object>() : new ConcurrentDictionary<string, object>(dict);
            _myKeyList = dict == null ? new List<string>() : dict.Keys.ToList();
            Normalize();
        }
        //public JSONObject(ConcurrentDictionary<string, object> dict) {
        //    _myDict = dict == null ? new ConcurrentDictionary<string, object>() : new ConcurrentDictionary<string, object>(dict);
        //    _myKeyList = _myDict.Keys.ToList();
        //    Normalize();
        //}
        /// <summary>
        /// Creates a new instance that's a copy of the specified JSONObject, without actually copying the data (underlying dictionary is the same instance).
        /// </summary>
        public JSONObject(JSONObject original) : this(original?._myDict == null ? new ConcurrentDictionary<string, object>() : new ConcurrentDictionary<string, object>(original._myDict)) {
            _myKeyList.Clear();
            lock (original?._myKeyList ?? new List<string>()) {
                if (original?._myKeyList != null) _myKeyList.AddRange(original._myKeyList);
            }
        }
        /// <summary>
        /// Creates a new instance that's a deep copy of this JSONObject with all underlying objects copied at all levels (e.g. JSONObjects and JSONArrays within this JSONObject will be deep-copied).
        /// </summary>
        public JSONObject Clone() {
            var newobj = new JSONObject();
            string[] keyarray;
            lock (_myKeyList) {
                keyarray = _myKeyList.ToArray();
            }
            foreach (var key in keyarray) {
                var val = _myDict[key];
                if (val is JSONObject) newobj.put(key, ((JSONObject) val).Clone());
                else if (val is JSONArray) newobj.put(key, ((JSONArray) val).Clone());
                else newobj.put(key, val);
            }
            return newobj;
        }
        /// <summary>
        ///     Construct a JSONObject from a string. Allows for comments like this: {/* comment */}
        ///     Also allows for comments without the curly-braces like this: /* comment */
        /// </summary>
        /// <param name="sJSON">A string beginning with '{' and ending with '}'.</param>
        public JSONObject(string sJSON) {
            if (string.IsNullOrEmpty(sJSON)) {
                _myDict = new ConcurrentDictionary<string, object>();
                _myKeyList = new List<string>();
                return;
            }
            for (var commentindex = 0; commentindex < CommentStarters.Length; ++commentindex) {
                var start = CommentStarters[commentindex];
                var end = CommentEnders[commentindex];
                while (sJSON.Contains(start)) {
                    var i = sJSON.IndexOf(start, StringComparison.Ordinal);
                    var i2 = sJSON.IndexOf(end, StringComparison.Ordinal);
                    if (i < 0 || i < 0 || i2 <= i) break;
                    sJSON = sJSON.Remove(i, i2 - i + 2);
                }
            }
            int ihack = 0;
            while ((ihack = sJSON.IndexOf("\\>", ihack, StringComparison.Ordinal)) >= 0) {
                sJSON = sJSON.Remove(ihack, 1);
            }
            lock (_serializer) {
                try {
                    var dict = _serializer.Deserialize<Dictionary<string, object>>(sJSON);
                    _myDict = new ConcurrentDictionary<string, object>(dict);
                    _myKeyList = dict.Keys.ToList();
                }
                catch {
                    throw;
                }
            }
            Normalize();
        }
        /// <summary>
        /// An event triggered when something changes in this object, such as a call to put.
        /// Handling this event lets you, for example, update a file every time it changes.
        /// USE WITH CARE.  Improper use will slow down your program if an object changes a lot!
        /// </summary>
        public event EventHandler Altered;
        public void TriggerAltered() {
            Altered?.Invoke(this, new EventArgs());
        }
        /// <summary>
        ///     Return the key for the associated index
        /// </summary>
        [XmlIgnore] public string this[int i] {
            get {
                lock (_myKeyList) return _myKeyList[i];
            }
        }
        /// <summary>
        ///     Get/Add an object with the associated key
        /// </summary>
        [XmlIgnore] public object this[string key] {
            get { return getValue(key); }
            set { put(key, value); }
        }
        /// <summary>
        ///     Return the number of JSON items in hashtable
        /// </summary>
        [XmlIgnore] public int Count => _myDict.Count;
        public virtual string[] Names {
            get {
                lock (_myKeyList) return _myKeyList.ToArray();
            }
        }
        public static string[] CommentEnders { get; } = {"*/}", "*/"};
        public static string[] CommentStarters { get; } = {"{/*", "/*"};
        /// <summary>
        ///     Accumulate values under a key. It is similar to the put method except
        ///     that if there is already an object stored under the key then a
        ///     JSONArray is stored under the key to hold all of the accumulated values.
        ///     If there is already a JSONArray, then the new value is appended to it.
        ///     In contrast, the put method replaces the previous value.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <param name="val">An object to be accumulated under the key.</param>
        /// <returns>this</returns>
        public JSONObject accumulate(string key, object val) {
            JSONArray a;
            var obj = opt(key);
            if (obj == null) {
                put(key, val);
            }
            else if (obj is JSONArray) {
                a = (JSONArray) obj;
                a.put(val);
            }
            else {
                a = new JSONArray();
                a.put(obj);
                a.put(val);
                put(key, a);
            }
            return this;
        }
        /// <summary>
        ///     <para>This method, along with the IEnumerable interface, enables inline initializers, like:</para>
        ///     <para>JSONObject jobj = new JSONObject { {"key", "value"}, ... };</para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public JSONObject Add(string key, object value) {
            put(key, value);
            return this;
        }
        /// <summary>
        ///     This version of Add allows you to include a boolean in an initializer to specify that the field should not be added
        ///     if it's null or an empty string.
        /// </summary>
        /// <param name="key">The key to add</param>
        /// <param name="value">The value to add</param>
        /// <param name="omitIfEmpty">If true, the key/value will not be added if the value is null or an empty string.</param>
        public JSONObject Add(string key, object value, bool omitIfEmpty) {
            if (omitIfEmpty && (value == null || value is string && string.IsNullOrEmpty((string) value))) return this;
            return put(key, value);
        }
        /// <summary>
        ///     This version of Add allows you to include a default value to add to the JSONObject if the value passed is null or
        ///     an empty string.
        /// </summary>
        /// <param name="key">The key to add</param>
        /// <param name="value">The value to add</param>
        /// <param name="defaultValue">The default value to add if value is null or an empty string.</param>
        public JSONObject Add(string key, object value, object defaultValue) {
            if (value == null || value is string && string.IsNullOrEmpty((string) value))
                return put(key, defaultValue);
            return put(key, value);
        }
        /// <summary>
        ///     Returns true if all items at the top level within this JSONObject satisfy a condition.
        /// </summary>
        public bool All(Func<object, bool> func) {
            return _myDict.Values.All(func);
        }
        public T Cast<T>() where T : JSONObject {
            return (T) this;
        }
        public virtual T get<T>(string key) {
            var obj = opt(key);
            if (obj == null) throw new Exception("No such element");
            return (T) obj;
        }
        /// <summary>
        ///     Get the boolean value associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>The truth.</returns>
        public virtual bool getBool(string key) {
            var o = getValue(key);
            if (o is bool) {
                var b = (bool) o;
                return b;
            }
            throw new Exception($"JSONObject[{JSONUtils.Enquote(key)}] is not a Boolean");
        }
        /// <summary>
        ///     C# convenience method
        /// </summary>
        /// <returns>The Hashtable</returns>
        public IDictionary<string, object> getDictionary() {
            return _myDict;
        }
        /// <summary>
        ///     Get the double value associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>The double value</returns>
        public double getDouble(string key) {
            var o = getValue(key);
            if (o is double) return (double) o;
            if (o is string) return (string) o == "" ? 0 : Convert.ToDouble(o);
            if (o is int) return (int) o;
            if (o is decimal) return decimal.ToDouble((decimal) o);
            var msg = $"JSONObject[{JSONUtils.Enquote(key)}] is not a double";
            throw new Exception(msg);
        }
        public IEnumerator GetEnumerator() {
            return new JSONObjectEnumerator(this);
        }
        /// <summary>
        ///     Get the int value associated with a key.
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns> The integer value.</returns>
        public int getInt(string key) {
            var o = getValue(key);
            if (o is int) return (int) o;
            if (o is string) return Convert.ToInt32(o);
            if (o is decimal) return decimal.ToInt32((decimal) o);
            if (o is double) return (int) Math.Round((double) o);
            var msg = $"JSONObject[{JSONUtils.Enquote(key)}] is not a int";
            throw new Exception(msg);
        }
        /// <summary>
        ///     Get the JSONArray value associated with a key.
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns>A JSONArray which is the value</returns>
        public JSONArray getJSONArray(string key) {
            var o = getValue(key);
            if (o is JSONArray) return (JSONArray) o;
            var msg = $"JSONObject[{JSONUtils.Enquote(key)}]={o} is not a JSONArray";
            throw new Exception(msg);
        }
        /// <summary>
        ///     Get the JSONObject value associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>A JSONObject which is the value.</returns>
        public JSONObject getJSONObject(string key) {
            var o = getValue(key);
            if (o is JSONObject) return (JSONObject) o;
            var msg = $"JSONObject[{JSONUtils.Enquote(key)}]={o} is not a JSONArray";
            throw new Exception(msg);
        }
        /// <summary>
        ///     Get the string associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>A string which is the value.</returns>
        public string getString(string key) {
            return getValue(key).ToString();
        }
        /// <summary>
        ///     Alias to Java get method
        ///     Get the value object associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>The object associated with the key.</returns>
        public virtual object getValue(string key) {
            var obj = opt(key);
            if (obj == null) throw new Exception("No such element");
            return obj;
        }
        /// <summary>
        ///     Determine if the JSONObject contains a specific key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>true if the key exists in the JSONObject.</returns>
        public virtual bool has(string key) {
            return _myDict.ContainsKey(key);
        }
        /// <summary>
        ///     Determine if the value associated with the key is null or if there is no value.
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns>true if there is no value associated with the key or if the valus is the JSONObject.NULL object</returns>
        public bool isNull(string key) {
            return NULL.Equals(opt(key));
        }
        /// <summary>
        ///     Get an enumeration of the keys of the JSONObject.
        ///     Added to be true to orginal Java implementation
        ///     Indexers are easier to use
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator keys() {
            lock (_myKeyList) return _myKeyList.GetEnumerator();
        }
        /// <summary>
        ///     Get the number of keys stored in the JSONObject.
        /// </summary>
        /// <returns>The number of keys in the JSONObject.</returns>
        public virtual int Length() {
            return _myDict.Count;
        }
        /// <summary>
        ///     Makes this JSONObject a copy of the specified one, performing a complete copy of the underlying dictionary.
        ///     To make a duplicate object without actually copying data, use the constructor that takes a JSONObject.
        /// </summary>
        public virtual JSONObject Load(JSONObject obj) {
            lock (_myKeyList) {
                lock (obj._myKeyList) {
                    _myDict = obj._myDict; //new ConcurrentDictionary<string, object>(obj._myDict);
                    _myKeyList = obj._myKeyList;
                    //_myKeyList.Clear();
                    //_myKeyList.AddRange(obj._myKeyList);
                }
            }
            Altered?.Invoke(this, new EventArgs());
            return this;
        }
        /// <summary>
        ///     Produce a JSONArray containing the names of the elements of this JSONObject
        /// </summary>
        /// <returns>A JSONArray containing the key strings, or null if the JSONObject</returns>
        public virtual JSONArray names() {
            lock (_myKeyList) return new JSONArray(_myKeyList);
            //foreach (var key in _myDict.Keys) ja.put(key);
            //if (ja.Length() == 0) return null;
            //return ja;
        }
        public T opt<T>(string key) {
            return (T) opt(key);
        }
        /// <summary>
        ///     Get an optional value associated with a key.
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns>An object which is the value, or null if there is no value.</returns>
        public virtual object opt(string key) {
            if (key == null) throw new ArgumentNullException(nameof(key), "Null key");
            return _myDict.ContainsKey(key) ? _myDict[key] : null;
        }
        /// <summary>
        ///     Get an optional value associated with a key.
        ///     It returns false if there is no such key, or if the value is not
        ///     Boolean.TRUE or the String "true".
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <param name="defaultValue">The preferred return value if conversion fails</param>
        /// <returns>bool value object</returns>
        public bool optBoolean(string key, bool defaultValue = false) {
            var obj = opt(key);
            if (obj != null) {
                if (obj is bool) return (bool) obj;
                if (obj is string) return Convert.ToBoolean(obj);
            }
            return defaultValue;
        }
        /// <summary>
        ///     Get an optional double associated with a key,
        ///     or NaN if there is no such key or if its value is not a number.
        ///     If the value is a string, an attempt will be made to evaluate it as
        ///     a number.
        /// </summary>
        /// <param name="key">A string which is the key.</param>
        /// <param name="defaultValue">The default</param>
        /// <returns>A double value object</returns>
        public double optDouble(string key, double defaultValue = double.NaN) {
            var obj = opt(key);
            if (obj != null) {
                if (obj is double) return (double) obj;
                if (obj is string) return (string) obj == "" ? 0 : Convert.ToDouble(obj);
                if (obj is int) return (int) obj;
                if (obj is decimal) return decimal.ToDouble((decimal) obj);
            }
            return defaultValue;
        }
        /// <summary>
        ///     Get an optional double associated with a key, or the
        ///     defaultValue if there is no such key or if its value is not a number.
        ///     If the value is a string, an attempt will be made to evaluate it as
        ///     number.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <param name="defaultValue">The default value</param>
        /// <returns>An int object value</returns>
        public int optInt(string key, int defaultValue = 0) {
            var obj = opt(key);
            if (obj != null) {
                if (obj is int) return (int) obj;
                if (obj is string) return Convert.ToInt32(obj);
                if (obj is decimal) return decimal.ToInt32((decimal) obj);
                if (obj is double) return (int) Math.Round((double) obj);
            }
            return defaultValue;
        }
        /// <summary>
        ///     Get an optional JSONArray associated with a key.
        ///     It returns null if there is no such key, or if its value is not a JSONArray
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns>A JSONArray which is the value</returns>
        public JSONArray optJSONArray(string key) {
            var obj = opt(key);
            return obj as JSONArray;
        }
        /// <summary>
        ///     Get an optional JSONObject associated with a key.
        ///     It returns null if there is no such key, or if its value is not a JSONObject.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>A JSONObject which is the value</returns>
        public JSONObject optJSONObject(string key) {
            var obj = opt(key);
            return obj as JSONObject;
        }
        /// <summary>
        ///     Get an optional string associated with a key.
        ///     It returns the defaultValue if there is no such key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <param name="defaultValue">The default</param>
        /// <returns>A string which is the value.</returns>
        public string optString(string key, string defaultValue = "") {
            var obj = opt(key);
            return obj?.ToString() ?? defaultValue;
        }
        // OMITTED - all put methods can be replaced by a indexer in C#
        //         - ===================================================
        // public JSONObject put(String key, boolean value)
        // public JSONObject put(String key, double value)
        // public JSONObject put(String key, int value)
        /// <summary>
        ///     Put a key/value pair in the JSONObject. If the value is null,
        ///     then the key will be removed from the JSONObject if it is present.
        /// </summary>
        /// <param name="key"> A key string.</param>
        /// <param name="val">
        ///     An object which is the value. It should be of one of these
        ///     types: Boolean, Double, Integer, JSONArray, JSONObject, String, or the
        ///     JSONObject.NULL object.
        /// </param>
        /// <returns>JSONObject</returns>
        public virtual JSONObject put(string key, object val) {
            if (key == null) throw new ArgumentNullException(nameof(key), "key cannot be null");
            if (val != null) {
                if (!_myDict.ContainsKey(key)) {
                    lock (_myKeyList) _myKeyList.Add(key);
                }
                _myDict.AddOrUpdate(key, val, (s, o) => val);
            }
            else remove(key);
            Altered?.Invoke(this, new EventArgs());
            return this;
        }
        /// <summary>
        ///     Add a key value pair, but only if the value is not null.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public JSONObject putOpt(string key, object val) {
            if (val != null) put(key, val);
            return this;
        }
        /// <summary>
        ///     Remove a object assosiateted with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object remove(string key) {
            if (_myDict.ContainsKey(key)) {
                object obj;
                _myDict.TryRemove(key, out obj);
                lock (_myKeyList) _myKeyList.Remove(key);
                Altered?.Invoke(this, new EventArgs());
                return obj;
            }
            return null;
        }
        /// <summary>
        ///     Returns a JSONArray containing the values in this JSONObject. If names is specified, returns only the values of the
        ///     named fields, in the same order than they appear in names array.
        /// </summary>
        public JSONArray toJSONArray(JSONArray names = null) {
            var ja = new JSONArray();
            if (names == null || names.Length() == 0) {
                lock (_myKeyList) return new JSONArray(_myKeyList.Select(k => _myDict[k]).ToList());
            }
            foreach (string name in names) ja.put(opt(name));
            return ja;
        }
        public virtual string ToJSONString() {
            return ToString();
        }
        /// <summary>
        ///     Overridden to return a JSON formatted object as a string
        /// </summary>
        /// <returns>JSON object as formatted string</returns>
        public override string ToString() {
            return ToString(false, 0);
        }
        public string ToString(bool format, int tab = 0) {
            if (_myDict.Count == 0) return "{}"; // short-circuit.
            object obj = null;
            string colon = ":" + (format ? " " : "");
            var sb = new StringBuilder();
            sb.Append("{" + (format ? Environment.NewLine : ""));
            ++tab;
            string[] keyarray;
            lock (_myKeyList) keyarray = _myKeyList.ToArray();
            foreach (var key in keyarray) {
                if (obj != null) sb.Append("," + (format ? Environment.NewLine : ""));
                obj = _myDict[key];
                if (obj != null) {
                    if (format)
                        for (int i = 0; i < tab; ++i)
                            sb.Append('\t');
                    sb.Append(JSONUtils.Enquote(key));
                    sb.Append(colon);
                    if (obj is string) sb.Append(JSONUtils.Enquote((string) obj));
                    else if (obj is double) sb.Append(numberToString(obj));
                    else if (obj is bool) sb.Append(obj.ToString().ToLower());
                    else if (obj is JSONObject) sb.Append(((JSONObject) obj).ToString(format, tab));
                    else if (obj is JSONArray) sb.Append(((JSONArray) obj).ToString(format, tab));
                    else sb.Append(obj);
                }
            }
            if (format) {
                sb.Append(Environment.NewLine);
                for (int i = 0; i < tab - 1; ++i) sb.Append('\t');
            }
            sb.Append('}');
            return sb.ToString();
        }
        public virtual IEnumerable<object> Where(Func<object, bool> func) {
            lock (_myKeyList) return _myKeyList.Select(k => _myDict[k]).Where(func);
        }
        private void Normalize() {
            bool changed = false;
            string[] keyarray;
            lock (_myKeyList) keyarray = _myKeyList.ToArray();
            foreach (var key in keyarray) {
                var o = _myDict[key];
                if (o is ArrayList) {
                    _myDict[key] = new JSONArray((ArrayList) o);
                    changed = true;
                }
                if (o is Array) {
                    _myDict[key] = new JSONArray((Array) o);
                    changed = true;
                }
                else if (o is Dictionary<string, object>) {
                    _myDict[key] = new JSONObject((Dictionary<string, object>) o);
                    changed = true;
                }
            }
            if (changed) Altered?.Invoke(this, new EventArgs());
        }
        /// <summary>
        ///     Produce a string from a number.
        /// </summary>
        /// <param name="number">Number value type object</param>
        /// <returns>String representation of the number</returns>
        public static string numberToString(object number) {
            if (number is double && double.IsNaN((double) number)) throw new ArgumentException("object must be a valid number", nameof(number));
            if (number is double && double.IsNaN((double) number)) throw new ArgumentException("object must be a valid number", nameof(number));
            // Shave off trailing zeros and decimal point, if possible
            var s = ((double) number).ToString(NumberFormatInfo.InvariantInfo).ToLower();
            if (s.IndexOf('e') < 0 && s.IndexOf('.') > 0) {
                while (s.EndsWith("0")) s = s.Substring(0, s.Length - 1);
                if (s.EndsWith(".")) s = s.Substring(0, s.Length - 1);
            }
            return s;
        }
        public static T XMLDeserialize<T>(XmlNode propertymap, Dictionary<int, string> stringmap, string serializedData) where T : JSONObject, new() {
            var xmldoc = new XmlDocument();
            try {
                // Build the property map into a Dictionary.
                var propertyMapNodes = propertymap.SelectNodes("prop");
                if (propertyMapNodes == null || propertyMapNodes.Count == 0) return null;
                var map = new Dictionary<int, string>();
                foreach (XmlNode mapnode in propertyMapNodes) map.Add(int.Parse(mapnode.Attributes?["tag"]?.Value ?? "X"), mapnode.Attributes?["name"].Value);
                // Build the LIMSObject from the data.
                var r = new T();
                xmldoc.LoadXml(serializedData);
                var datanode = xmldoc.SelectSingleNode("//x_s_d");
                var pinfos = typeof(T).GetProperties().ToDictionary(pi => pi.Name);
                // Initialize all values in the LIMSObject to default values.
                // Default values are: false for bool, 0 for int/decimal, "" for strings, null for everything else.
                foreach (var pi in pinfos.Values) {
                    if (pi.IsDefined(typeof(XmlIgnoreAttribute), false) || !pi.CanWrite) continue;
                    if (pi.PropertyType.IsValueType) pi.SetValue(r, Activator.CreateInstance(pi.PropertyType), null);
                    else if (pi.PropertyType == typeof(string)) pi.SetValue(r, string.Empty, null);
                    else if (pi.PropertyType == typeof(DateTime)) pi.SetValue(r, DateTime.Parse("1/1/0001 12:00:00 AM"), null);
                    else pi.SetValue(r, null, null);
                }
                var data = datanode?.InnerText.Split(new[] {'|'}, StringSplitOptions.None);
                if (data == null || data.Length == 0 || data.Length % 2 != 0) return null; // bad data.
                for (var i = 0; i < data.Length; i += 2) {
                    var propname = map[int.Parse(data[i])];
                    if (propname != null && pinfos[propname].CanWrite && !pinfos[propname].IsDefined(typeof(XmlIgnoreAttribute), false)) {
                        int key;
                        string val;
                        if (data[i + 1].StartsWith("_!_") && int.TryParse(data[i + 1].Substring(3), out key) && stringmap.ContainsKey(key))
                            val = stringmap[key];
                        else val = data[i + 1];
                        if (pinfos[propname].PropertyType == typeof(bool)) pinfos[propname].SetValue(r, val == "T", null);
                        else if (pinfos[propname].PropertyType == typeof(int)) pinfos[propname].SetValue(r, int.Parse(val), null);
                        else if (pinfos[propname].PropertyType == typeof(string)) pinfos[propname].SetValue(r, val, null);
                        else if (pinfos[propname].PropertyType == typeof(decimal)) pinfos[propname].SetValue(r, decimal.Parse(val), null);
                        else if (pinfos[propname].PropertyType == typeof(DateTime)) pinfos[propname].SetValue(r, DateTime.Parse(val), null);
                    }
                }
                return r;
            }
            catch {
                return null;
            }
        }
        public static StringBuilder XMLSerialize<T>(T o, Dictionary<string, int> strings) where T : JSONObject, new() {
            var sb = new StringBuilder();
            var pinfos = typeof(T).GetProperties();
            var t = 0;
            var divider = "";
            foreach (var pi in pinfos) {
                if (pi.IsDefined(typeof(XmlIgnoreAttribute), false)) continue;
                var val = pi.GetValue(o, null);
                if (val != null && val.ToString() != "" &&
                    !(val is bool? && (bool) val == false) &&
                    !(val is int? && (int) val == 0) &&
                    !(val is DateTime && val.ToString() == "1/1/0001 12:00:00 AM") &&
                    !string.IsNullOrEmpty(strslz(val))) {
                    // strings longer than 6 chars go into the string map, and the value placed in the data is "_!_key"
                    // also, any string containing the "|" character will go ni the map, so that we keep them out of the data, 
                    // which uses that character as a delimiter.
                    if (val is string && ((val as string).Length > StringMapThreshold || (val as string).Contains("|")))
                        if (strings.ContainsKey(val as string)) {
                            val = "_!_" + strings[val as string];
                        }
                        else {
                            strings.Add(val as string, strings.Count);
                            val = "_!_" + (strings.Count - 1);
                        }
                    sb.Append(divider + t + "|" + strslz(val));
                    divider = "|";
                }
                ++t;
            }
            var sb2 = new StringBuilder();
            var xwsettings = new XmlWriterSettings {OmitXmlDeclaration = true, Indent = false};
            using (var xw = XmlWriter.Create(sb2, xwsettings)) {
                xw.WriteElementString("x_s_d", sb.ToString());
            }
            return sb2;
        }
        public static string XMLSerializeMap<T>() where T : JSONObject {
            var sb = new StringBuilder();
            var xwsettings = new XmlWriterSettings {OmitXmlDeclaration = true, Indent = false};
            using (var xw = XmlWriter.Create(sb, xwsettings)) {
                xw.WriteStartElement("x_s_m");
                var pinfos = typeof(T).GetProperties();
                var t = 0;
                foreach (var pi in pinfos) {
                    if (!pi.IsDefined(typeof(XmlIgnoreAttribute), false)) {
                        xw.WriteStartElement("prop");
                        xw.WriteAttributeString("name", pi.Name);
                        xw.WriteAttributeString("tag", t.ToString());
                        xw.WriteEndElement();
                    }
                    ++t;
                }
                xw.WriteEndElement(); // </x_s_m>
            }
            return sb.ToString();
        }
        private static string strslz(object o) {
            if (o is bool?) return (bool) o ? "T" : "F";
            if (o is IDictionary) return null;
            return o?.ToString();
        }
        /// <summary>The hash map where the JSONObject's properties are kept.</summary>
        private ConcurrentDictionary<string, object> _myDict;
        protected void SetMyDict(IDictionary<string, object> dict, List<string> keys) {
            _myDict = new ConcurrentDictionary<string, object>(dict);
            lock (_myKeyList) {
                _myKeyList.Clear();
                _myKeyList.AddRange(keys);
            }
        }
        /// <summary>A shadow list of keys to enable access by sequence of insertion</summary>
        private List<string> _myKeyList = new List<string>();
        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer {MaxJsonLength = int.MaxValue, RecursionLimit = int.MaxValue};
        /// <summary>
        ///     It is sometimes more convenient and less ambiguous to have a NULL
        ///     object than to use C#'s null value.
        ///     JSONObject.NULL.toString() returns "null".
        /// </summary>
        public static readonly JSONNull NULL = new JSONNull();
        public const int StringMapThreshold = 6;
        /// <summary>
        ///     Make a Null object
        ///     JSONObject.NULL is equivalent to the value that JavaScript calls null,
        ///     whilst C#'s null is equivalent to the value that JavaScript calls undefined.
        /// </summary>
        public struct JSONNull {
            /// <summary>
            ///     Overriden to return "null"
            /// </summary>
            /// <returns>null</returns>
            public override string ToString() {
                //return base.ToString ();
                return "null";
            }
        }
    }
}