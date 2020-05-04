using System;
using System.Collections;

namespace JSON {
    /// <summary>
    ///     Generic IEnumerator code taken from MSDN, modified slightly to fit this implementation.
    /// </summary>
    public class JSONObjectEnumerator : IEnumerator {
        /// <summary>
        ///     Creates an enumerator for a JSONObject.
        /// </summary>
        public JSONObjectEnumerator(JSONObject jobj) {
            _jsonObject = jobj;
        }
        public object Current {
            get {
                try {
                    return _jsonObject[_position];
                }
                catch (IndexOutOfRangeException) {
                    throw new InvalidOperationException();
                }
            }
        }
        public bool MoveNext() {
            ++_position;
            return (_position < _jsonObject.Count);
        }
        public void Reset() {
            _position = -1;
        }
        private readonly JSONObject _jsonObject;
        private int _position = -1;
    }
}