using System;
using System.Collections;
using System.Collections.Generic;

namespace JSON {
    /// <summary>
    ///     JSONArrayCollection is an ordered collection of values. An array begins with
    ///     "[" (left bracket) and ends with "]" (right bracket). Array elements are
    ///     separated by "," (comma).
    /// </summary>
    public class JSONArrayCollection : JSONValueCollection {
        /// <summary>
        ///     Public constructor that accepts a generic list of JSONValue objects.
        /// </summary>
        /// <param name="values">Generic list of JSONValue objects.</param>
        public JSONArrayCollection(List<JSONValue> values) {
            _values = values;
        }
        /// <summary>
        ///     Empty public constructor. Use this method in conjunction with
        ///     the Add method to populate the internal array of elements.
        /// </summary>
        public JSONArrayCollection() {
            _values = new List<JSONValue>();
        }
        /// <summary>
        ///     Required override of the BeginMarker property
        /// </summary>
        protected override string BeginMarker => "[";
        /// <summary>
        ///     Required override of the EndMarker property
        /// </summary>
        protected override string EndMarker => "]";
        /// <summary>
        ///     Adds a JSONValue to the internal object array.  Values are checked to
        ///     ensure no duplication occurs in the internal array.
        /// </summary>
        /// <param name="value">JSONValue to add to the internal array</param>
        public void Add(JSONValue value) {
            if (!_values.Contains(value)) _values.Add(value);
        }
        public override IEnumerator GetEnumerator() {
            return _values.GetEnumerator();
        }
        /// <summary>
        ///     Required override of the CollectionToPrettyPrint() method.
        /// </summary>
        /// <returns>the entire collection as a string in JSON-compliant format, with indentation for readability</returns>
        protected override string CollectionToPrettyPrint() {
            CURRENT_INDENT++;
            List<string> output = new List<string>();
            List<string> nvps = new List<string>();
            foreach (JSONValue jv in _values) nvps.Add("".PadLeft(CURRENT_INDENT, Convert.ToChar(HORIZONTAL_TAB)) + jv.PrettyPrint());
            output.Add(string.Join(JSONVALUE_SEPARATOR + Environment.NewLine, nvps.ToArray()));
            CURRENT_INDENT--;
            return string.Join("", output.ToArray());
        }
        /// <summary>
        ///     Required override of the CollectionToString() method.
        /// </summary>
        /// <returns>the entire collection as a string in JSON-compliant format</returns>
        protected override string CollectionToString() {
            List<string> output = new List<string>();
            List<string> nvps = new List<string>();
            foreach (JSONValue jv in _values) nvps.Add(jv.ToString());
            output.Add(string.Join(JSONVALUE_SEPARATOR, nvps.ToArray()));
            return string.Join("", output.ToArray());
        }
        /// <summary>
        ///     Internal generic list of JSONValue objects that comprise the elements
        ///     of the JSONArrayCollection.
        /// </summary>
        protected List<JSONValue> _values;
    }
}