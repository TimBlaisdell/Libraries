using System;
using System.Collections;
using System.Collections.Generic;

namespace IODPUtils.JSON {
    /// <summary>
    ///     JSONObjectCollection is an unordered set of name/value pairs. An object begins
    ///     with "{" (left brace) and ends with "}" (right brace). Each name is followed
    ///     by ":" (colon) and the name/value pairs are separated by "," (comma).
    /// </summary>
    public class JSONObjectCollection : JSONValueCollection {
        /// <summary>
        ///     Public constructor that accepts a Dictionary of name/value pairs.
        /// </summary>
        /// <param name="namevaluepairs">Dictionary collection of name/value pairs (JSONStringValue=name,JSONValue=pair)</param>
        public JSONObjectCollection(Dictionary<JSONStringValue, JSONValue> namevaluepairs) {
            _namevaluepairs = namevaluepairs;
        }
        /// <summary>
        ///     Empty public constructor. Use this method in conjunction with
        ///     the Add method to populate the internal dictionary of name/value pairs.
        /// </summary>
        public JSONObjectCollection() {
            _namevaluepairs = new Dictionary<JSONStringValue, JSONValue>();
        }
        /// <summary>
        ///     Required override of the BeginMarker property
        /// </summary>
        protected override string BeginMarker {
            get { return "{"; }
        }
        /// <summary>
        ///     Required override of the EndMarker property
        /// </summary>
        protected override string EndMarker {
            get { return "}"; }
        }
        /// <summary>
        ///     Adds a JSONStringValue as the "name" and a JSONValue as the "value" to the
        ///     internal Dictionary.  Values are checked to ensure no duplication occurs
        ///     in the internal Dictionary.
        /// </summary>
        /// <param name="name">JSONStringValue "name" to add to the internal dictionary</param>
        /// <param name="value">JSONValue "value" to add to the internal dictionary</param>
        public void Add(JSONStringValue name, JSONValue value) {
            if (!_namevaluepairs.ContainsKey(name)) _namevaluepairs.Add(name, value);
        }
        /// <summary>
        ///     This version of Add allows you to specify third boolean value in an initializer to specify that the entry should be
        ///     omitted if null.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="omitIfNull">if set to <c>true</c> [omit if null].</param>
        public void Add(JSONStringValue name, JSONValue value, bool omitIfNull) {
            if (omitIfNull && value == null) return;
            Add(name, value);
        }
        public override IEnumerator GetEnumerator() {
            return _namevaluepairs.GetEnumerator();
        }
        /// <summary>
        ///     Required override of the CollectionToPrettyPrint() method.
        /// </summary>
        /// <returns>the entire dictionary as a string in JSON-compliant format, with indentation for readability</returns>
        protected override string CollectionToPrettyPrint() {
            CURRENT_INDENT++;
            List<string> output = new List<string>();
            List<string> nvps = new List<string>();
            foreach (KeyValuePair<JSONStringValue, JSONValue> kvp in _namevaluepairs)
                nvps.Add("".PadLeft(CURRENT_INDENT, Convert.ToChar(HORIZONTAL_TAB)) + kvp.Key.PrettyPrint() + NAMEVALUEPAIR_SEPARATOR + kvp.Value.PrettyPrint());
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
            foreach (KeyValuePair<JSONStringValue, JSONValue> kvp in _namevaluepairs)
                nvps.Add(kvp.Key + NAMEVALUEPAIR_SEPARATOR + kvp.Value);
            output.Add(string.Join(JSONVALUE_SEPARATOR, nvps.ToArray()));
            return string.Join("", output.ToArray());
        }
        private readonly Dictionary<JSONStringValue, JSONValue> _namevaluepairs;
        private const string NAMEVALUEPAIR_SEPARATOR = ":";
    }
}