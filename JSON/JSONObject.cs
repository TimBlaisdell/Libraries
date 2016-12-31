using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

namespace IODPUtils.JSON {
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
    public class JSONObject : JSONSerializableValue, IEnumerable {
        /// <summary>
        ///     Construct an empty JSONObject.
        /// </summary>
        public JSONObject() {
            _myDict = new Dictionary<string, object>();
            _myKeyList = _myDict.Keys.ToList();
        }
        public JSONObject(Dictionary<string, object> dict) {
            _myDict = dict;
            _myKeyList = _myDict.Keys.ToList();
            Normalize();
        }
        /// <summary>
        ///     Construct a JSONObject from a string.
        /// </summary>
        /// <param name="sJSON">A string beginning with '{' and ending with '}'.</param>
        public JSONObject(string sJSON) {
            lock (_serializer) {
                _myDict = _serializer.Deserialize<Dictionary<string, object>>(sJSON);
            }
            _myKeyList = _myDict.Keys.ToList();
            Normalize();
        }
        private void Normalize() {
            foreach (string key in _myDict.Keys.ToArray()) {
                object o = _myDict[key];
                if (o is ArrayList) _myDict[key] = new JSONArray((ArrayList) o);
                if (o is Array) _myDict[key] = new JSONArray((Array) o);
                else if (o is Dictionary<string, object>) _myDict[key] = new JSONObject((Dictionary<string, object>) o);
            }
        }
        // public JSONObject(Hashtable map)
        // By changing to arg to interface, all classes that implements IDictionary can be used
        // public interface IDictionary : ICollection, IEnumerable
        // Classes that implements IDictionary
        // 1. BaseChannelObjectWithProperties - Provides a base implementation of a channel object that wants to provide a dictionary interface to its properties.
        // 2. DictionaryBase - Provides the abstract (MustInherit in Visual Basic) base class for a strongly typed collection of key-and-value pairs.
        // 3. Hashtable - Represents a collection of key-and-value pairs that are organized based on the hash code of the key.
        // 4. HybridDictionary - Implements IDictionary by using a ListDictionary while the collection is small, and then switching to a Hashtable when the collection gets large.
        // 5. ListDictionary - Implements IDictionary using a singly linked list. Recommended for collections that typically contain 10 items or less.
        // 6. PropertyCollection - Contains the properties of a DirectoryEntry.
        // 7. PropertyDescriptorCollection - Represents a collection of PropertyDescriptor objects.
        // 8. SortedList - Represents a collection of key-and-value pairs that are sorted by the keys and are accessible by key and by index.
        // 9. StateBag - Manages the view state of ASP.NET server controls, including pages. This class cannot be inherited.
        // See ms-help://MS.VSCC.2003/MS.MSDNQTR.2003FEB.1033/cpref/html/frlrfsystemcollectionsidictionaryclasstopic.htm
        /// <summary>
        ///     Return the key for the associated index
        /// </summary>
        [XmlIgnore] public string this[int i] {
            get { return _myKeyList[i]; }
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
        [XmlIgnore] public int Count {
            get { return _myDict.Count; }
        }
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
            object obj = opt(key);
            if (obj == null) put(key, val);
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
            if (omitIfEmpty && (value == null || ((value is string) && string.IsNullOrEmpty((string) value)))) return this;
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
            if (value == null || ((value is string) && string.IsNullOrEmpty((string) value)))
                return put(key, defaultValue);
            return put(key, value);
        }
        /// <summary>
        ///     Get the boolean value associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>The truth.</returns>
        public virtual bool getBool(string key) {
            object o = getValue(key);
            if (o is bool) {
                bool b = (bool) o;
                return b;
            }
            string msg = string.Format("JSONObject[{0}] is not a Boolean", JSONUtils.Enquote(key));
            throw new Exception(msg);
        }
        /// <summary>
        ///     C# convenience method
        /// </summary>
        /// <returns>The Hashtable</returns>
        public IDictionary getDictionary() {
            return _myDict;
        }
        /// <summary>
        ///     Get the double value associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>The double value</returns>
        public double getDouble(string key) {
            object o = getValue(key);
            if (o is double) return (double) o;
            if (o is string) return Convert.ToDouble(o);
            string msg = string.Format("JSONObject[{0}] is not a double", JSONUtils.Enquote(key));
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
            object o = getValue(key);
            if (o is int) return (int) o;
            if (o is string) return Convert.ToInt32(o);
            string msg = string.Format("JSONObject[{0}] is not a int", JSONUtils.Enquote(key));
            throw new Exception(msg);
        }
        /// <summary>
        ///     Get the JSONArray value associated with a key.
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns>A JSONArray which is the value</returns>
        public JSONArray getJSONArray(string key) {
            object o = getValue(key);
            if (o is JSONArray) return (JSONArray) o;
            string msg = string.Format("JSONObject[{0}]={1} is not a JSONArray", JSONUtils.Enquote(key), o);
            throw new Exception(msg);
        }
        /// <summary>
        ///     Get the JSONObject value associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>A JSONObject which is the value.</returns>
        public JSONObject getJSONObject(string key) {
            object o = getValue(key);
            if (o is JSONObject) return (JSONObject) o;
            string msg = string.Format("JSONObject[{0}]={1} is not a JSONArray", JSONUtils.Enquote(key), o);
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
        public object getValue(string key) {
            object obj = opt(key);
            if (obj == null) throw new Exception("No such element");
            return obj;
        }
        /// <summary>
        ///     Determine if the JSONObject contains a specific key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>true if the key exists in the JSONObject.</returns>
        public bool has(string key) {
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
        public IEnumerator keys() {
            return _myDict.Keys.GetEnumerator();
        }
        /// <summary>
        ///     Get the number of keys stored in the JSONObject.
        /// </summary>
        /// <returns>The number of keys in the JSONObject.</returns>
        public int Length() {
            return _myDict.Count;
        }
        public void Load(JSONObject obj) {
            _myDict = new Dictionary<string, object>(obj._myDict);
            _myKeyList = new List<string>(obj._myKeyList);
        }
        /// <summary>
        ///     Produce a JSONArray containing the names of the elements of this JSONObject
        /// </summary>
        /// <returns>A JSONArray containing the key strings, or null if the JSONObject</returns>
        public JSONArray names() {
            JSONArray ja = new JSONArray();
            //NOTE!! I choose to use keys stored in the ArrayList, to maintain sequence order
            foreach (string key in _myKeyList) ja.put(key);
            if (ja.Length() == 0) return null;
            return ja;
        }
        /// <summary>
        ///     Produce a string from a number.
        /// </summary>
        /// <param name="number">Number value type object</param>
        /// <returns>String representation of the number</returns>
        public string numberToString(object number) {
            if (number is double && double.IsNaN(((double) number))) {
                //string msg = string.Format("");
                throw new ArgumentException("object must be a valid number", "number");
            }
            if (number is double && double.IsNaN(((double) number))) {
                //string msg = string.Format("");
                throw new ArgumentException("object must be a valid number", "number");
            }
            // Shave off trailing zeros and decimal point, if possible
            string s = ((double) number).ToString(NumberFormatInfo.InvariantInfo).ToLower();
            if (s.IndexOf('e') < 0 && s.IndexOf('.') > 0) {
                while (s.EndsWith("0")) {
                    s = s.Substring(0, s.Length - 1);
                }
                if (s.EndsWith(".")) {
                    s = s.Substring(0, s.Length - 1);
                }
            }
            return s;
        }
        /// <summary>
        ///     Get an optional value associated with a key.
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns>An object which is the value, or null if there is no value.</returns>
        public object opt(string key) {
            if (key == null) throw new ArgumentNullException("key", "Null key");
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
            object obj = opt(key);
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
        /// <returns>A double value object</returns>
        public double optDouble(string key) {
            return optDouble(key, double.NaN);
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
        public double optDouble(string key, double defaultValue) {
            object obj = opt(key);
            if (obj != null) {
                if (obj is double) return (double) obj;
                if (obj is string) return Convert.ToDouble(obj);
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
        /// <returns>An int object value</returns>
        public int optInt(string key) {
            return optInt(key, 0);
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
        public int optInt(string key, int defaultValue) {
            object obj = opt(key);
            if (obj != null) {
                if (obj is int) return (int) obj;
                if (obj is string) return Convert.ToInt32(obj);
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
            object obj = opt(key);
            return obj as JSONArray;
        }
        /// <summary>
        ///     Get an optional JSONObject associated with a key.
        ///     It returns null if there is no such key, or if its value is not a JSONObject.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>A JSONObject which is the value</returns>
        public JSONObject optJSONObject(string key) {
            object obj = opt(key);
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
            object obj = opt(key);
            return obj != null ? obj.ToString() : defaultValue;
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
            if (key == null) throw new ArgumentNullException("key", "key cannot be null");
            if (val != null) {
                if (!_myDict.ContainsKey(key)) {
                    //string test = key;
                    _myDict.Add(key, val);
                    _myKeyList.Add(key);
                }
                else _myDict[key] = val;
            }
            else remove(key);
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
        public object remove(string key) {
            if (_myDict.ContainsKey(key)) {
                object obj = _myDict[key];
                _myDict.Remove(key);
                _myKeyList.Remove(key);
                return obj;
            }
            return null;
        }
        /// <summary>
        ///     Returns a JSONArray containing the values in this JSONObject. If names is specified, returns only the values of the
        ///     named fields, in the same order than they appear in names array.
        /// </summary>
        public JSONArray toJSONArray(JSONArray names = null) {
            JSONArray ja = new JSONArray();
            if (names == null || names.Length() == 0) return new JSONArray(_myDict.Values.ToArray());
            foreach (string name in names) ja.put(opt(name));
            return ja;
        }
        /// <summary>
        ///     Overridden to return a JSON formatted object as a string
        /// </summary>
        /// <returns>JSON object as formatted string</returns>
        public override string ToString() {
            object obj = null;
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            foreach (string key in _myDict.Keys) {
                if (obj != null) sb.Append(',');
                obj = _myDict[key];
                if (obj != null) {
                    sb.Append(JSONUtils.Enquote(key));
                    sb.Append(':');
                    if (obj is string) sb.Append(JSONUtils.Enquote((string) obj));
                    else if (obj is double) sb.Append(numberToString(obj));
                    else if (obj is bool) sb.Append(obj.ToString().ToLower());
                    else sb.Append(obj);
                }
            }
            sb.Append('}');
            return sb.ToString();
        }
        public static T XMLDeserialize<T>(XmlNode propertymap, Dictionary<int, string> stringmap, string serializedData) where T : JSONObject, new() {
            XmlDocument xmldoc = new XmlDocument();
            try {
                // Build the property map into a Dictionary.
                XmlNodeList propertyMapNodes = propertymap.SelectNodes("prop");
                if (propertyMapNodes == null || propertyMapNodes.Count == 0) return null;
                Dictionary<int, string> map = new Dictionary<int, string>();
                foreach (XmlNode mapnode in propertyMapNodes) {
                    map.Add(int.Parse(mapnode.Attributes["tag"].Value), mapnode.Attributes["name"].Value);
                }
                // Build the LIMSObject from the data.
                T r = new T();
                xmldoc.LoadXml(serializedData);
                XmlNode datanode = xmldoc.SelectSingleNode("//x_s_d");
                Dictionary<string, PropertyInfo> pinfos = typeof (T).GetProperties().ToDictionary(pi => pi.Name);
                // Initialize all values in the LIMSObject to default values.
                // Default values are: false for bool, 0 for int/decimal, "" for strings, null for everything else.
                foreach (PropertyInfo pi in pinfos.Values) {
                    if (pi.IsDefined(typeof (XmlIgnoreAttribute), false) || !pi.CanWrite) continue;
                    if (pi.PropertyType.IsValueType) {
                        pi.SetValue(r, Activator.CreateInstance(pi.PropertyType), null);
                    }
                    else if (pi.PropertyType == typeof (string)) {
                        pi.SetValue(r, String.Empty, null);
                    }
                    else if (pi.PropertyType == typeof (DateTime)) {
                        pi.SetValue(r, DateTime.Parse("1/1/0001 12:00:00 AM"), null);
                    }
                    else pi.SetValue(r, null, null);
                }
                string[] data = datanode.InnerText.Split(new[] {'|'}, StringSplitOptions.None);
                if (data.Length == 0 || data.Length % 2 != 0) return null; // bad data.
                for (int i = 0; i < data.Length; i += 2) {
                    string propname = map[int.Parse(data[i])];
                    if (propname != null && pinfos[propname].CanWrite && !pinfos[propname].IsDefined(typeof (XmlIgnoreAttribute), false)) {
                        int key;
                        string val;
                        if (data[i + 1].StartsWith("_!_") && int.TryParse(data[i + 1].Substring(3), out key) && stringmap.ContainsKey(key))
                            val = stringmap[key];
                        else val = data[i + 1];
                        if (pinfos[propname].PropertyType == typeof (bool)) pinfos[propname].SetValue(r, val == "T", null);
                        else if (pinfos[propname].PropertyType == typeof (int)) pinfos[propname].SetValue(r, int.Parse(val), null);
                        else if (pinfos[propname].PropertyType == typeof (string)) pinfos[propname].SetValue(r, val, null);
                        else if (pinfos[propname].PropertyType == typeof (decimal)) pinfos[propname].SetValue(r, decimal.Parse(val), null);
                        else if (pinfos[propname].PropertyType == typeof (DateTime)) pinfos[propname].SetValue(r, DateTime.Parse(val), null);
                    }
                }
                return r;
            }
            catch {
                return null;
            }
        }
        public static StringBuilder XMLSerialize<T>(T o, Dictionary<string, int> strings) where T : JSONObject, new() {
            StringBuilder sb = new StringBuilder();
            PropertyInfo[] pinfos = typeof (T).GetProperties();
            int t = 0;
            string divider = "";
            foreach (PropertyInfo pi in pinfos) {
                if (pi.IsDefined(typeof (XmlIgnoreAttribute), false)) continue;
                object val = pi.GetValue(o, null);
                if (val != null && val.ToString() != "" &&
                    !(val is bool? && (bool) val == false) &&
                    !(val is int? && (int) val == 0) &&
                    !(val is DateTime && val.ToString() == "1/1/0001 12:00:00 AM") &&
                    !string.IsNullOrEmpty(strslz(val))) {
                    // strings longer than 6 chars go into the string map, and the value placed in the data is "_!_key"
                    // also, any string containing the "|" character will go ni the map, so that we keep them out of the data, 
                    // which uses that character as a delimiter.
                    if (val is string && ((val as string).Length > StringMapThreshold || (val as string).Contains("|"))) {
                        if (strings.ContainsKey(val as string)) val = "_!_" + strings[val as string];
                        else {
                            strings.Add(val as string, strings.Count);
                            val = "_!_" + (strings.Count - 1);
                        }
                    }
                    sb.Append(divider + t + "|" + strslz(val));
                    divider = "|";
                }
                ++t;
            }
            StringBuilder sb2 = new StringBuilder();
            XmlWriterSettings xwsettings = new XmlWriterSettings {OmitXmlDeclaration = true, Indent = false};
            using (XmlWriter xw = XmlWriter.Create(sb2, xwsettings)) xw.WriteElementString("x_s_d", sb.ToString());
            return sb2;
        }
        public static string XMLSerializeMap<T>() where T : JSONObject {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings xwsettings = new XmlWriterSettings {OmitXmlDeclaration = true, Indent = false};
            using (XmlWriter xw = XmlWriter.Create(sb, xwsettings)) {
                xw.WriteStartElement("x_s_m");
                PropertyInfo[] pinfos = typeof (T).GetProperties();
                int t = 0;
                foreach (PropertyInfo pi in pinfos) {
                    if (!pi.IsDefined(typeof (XmlIgnoreAttribute), false)) {
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
            return o == null ? null : o.ToString();
        }
        /// <summary>The hash map where the JSONObject's properties are kept.</summary>
        private Dictionary<string, object> _myDict;
        /// <summary>A shadow list of keys to enable access by sequence of insertion</summary>
        private List<string> _myKeyList;
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