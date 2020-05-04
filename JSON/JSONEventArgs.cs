using System;

namespace JSON {
    public class JSONEventArgs : EventArgs {
        public JSONEventArgs(JSONObject obj) {
            JSONObject = obj;
        }
        public JSONObject JSONObject { get; set; }
    }
}