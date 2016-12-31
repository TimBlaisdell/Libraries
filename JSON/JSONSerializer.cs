using System;
using System.Collections.Generic;

namespace IODPUtils.JSON {
    public static class JSONSerializer {
        public static JSONObject Serialize(object obj) {
            JSONObject json = new JSONObject();
            foreach (var prop in obj.GetType().GetProperties()) {
                json.put(prop.Name.ToLower(), prop.GetValue(obj, null));
            }
            return json;
        }
        public static JSONArray Serialize(IEnumerable<object> objs) {
            JSONArray json = new JSONArray();
            foreach (var obj in objs) {
                json.put(Serialize(obj));
            }
            return json;
        }
        public static T Deserialize<T>(JSONObject json) where T : new() {
            T obj = new T();
            foreach (var prop in obj.GetType().GetProperties()) {
                if (!prop.CanWrite) continue;
                if (json.has(prop.Name.ToLower())) {
                    if (prop.PropertyType == typeof (DateTime) || prop.PropertyType == typeof(DateTime?)) prop.SetValue(obj, DateTime.Parse(json.getString(prop.Name.ToLower()), null), null);
                    else if (prop.PropertyType == typeof (int) || prop.PropertyType == typeof (int?)) prop.SetValue(obj, json.getInt(prop.Name.ToLower()), null);
                    else if (prop.PropertyType == typeof (bool) || prop.PropertyType == typeof (bool?)) {
                        string s = json.getString(prop.Name.ToLower()).ToUpper();
                        prop.SetValue(obj, s.StartsWith("T") || s == "1", null);
                    }
                    else prop.SetValue(obj, json.opt(prop.Name.ToLower()), null);
                }
            }
            return obj;
        }
        public static List<T> Deserialize<T>(JSONArray json) where T : new() {
            var list = new List<T>();
            for (int i = 0; i < json.Length(); ++i) {
                list.Add(Deserialize<T>(json.getJSONObject(i)));
            }
            return list;
        } 
    }
}