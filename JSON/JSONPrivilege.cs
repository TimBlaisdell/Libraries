namespace IODPUtils.JSON {
    public class JSONPrivilege : JSONObject_WithDirtyFlags {
        public string application {
            get { return get<string>("application"); }
            //set { put("application", value); }
        }
        public string description {
            get { return get<string>("description"); }
            //set { put("description", value); }
        }
        public string keyword {
            get { return get<string>("keyword"); }
            //set { put("keyword", value); }
        }
        public string qualifier {
            get { return get<string>("qualifier"); }
            //set { put("qualifier", value); }
        }
    }
}