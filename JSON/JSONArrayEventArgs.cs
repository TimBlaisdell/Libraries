using System;

namespace JSON {
    public class JSONArrayEventArgs : EventArgs {
        public JSONArrayEventArgs(JSONArray array) {
            JSONArray = array;
        }
        public JSONArray JSONArray { get; set; }
    }
}