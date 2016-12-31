using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace Utilities {
    public partial class Settings {
        /*----------------------*/
        /* Constructor          */
        /*----------------------*/
        public Settings(string xmlfile, string primarytag, params string[] intermediatetags) {
            _xmlfile = xmlfile;
            _tags = new string[1 + (intermediatetags?.Length ?? 0)];
            _tags[0] = primarytag;
            if (intermediatetags != null) for (int i = 0; i < intermediatetags.Length; ++i) _tags[i + 1] = intermediatetags[i];
            LoadSettings();
        }
        /*----------------------*/
        /* Indexer              */
        /*----------------------*/
        public object this[params string[] names] {
            get {
                if (names == null || names.Length == 0) return null;
                if (names.Length > _tags.Length) throw new Exception("Attempt to pass more names than there are tag levels to a settings object.");
                lock (_lockobj) return Get(names);
            }
            set {
                lock (_lockobj) {
                    if (value == null) Clear(names);
                    Set(names, value);
                    //if (value is int || value is bool || value is string || value.GetType().IsEnum || value is INIValue) Set(names, value);
                    //else if (value is string) Set(names, value as string);
                    //else if (value is IEnumerable<int>) Set(names, value as IEnumerable<int>);
                    //else if (value is IEnumerable<string>) Set(names, value as IEnumerable<string>);
                    //else if (value is Enum) Set(names, value as Enum);
                    //else if (value is bool) Set(names, (bool) value);
                    //else if (value is INIValue) Set(names, (INIValue) value);
                    //else if (value is IEnumerable<INIValue>) Set(names, value as IEnumerable<INIValue>);
                }
            }
        }
        /*----------------------*/
        /* Methods              */
        /*----------------------*/
        public int Count(params string[] names) {
            string xpath = names == null || names.Length == 0 ? "//" + _tags[0] : MakeXPath(names) + "/" + _tags[names.Length];
            XmlNodeList nodes = _settings.SelectNodes(xpath);
            return nodes?.Count ?? 0;
        }
        public TEnum GetEnum<TEnum>(params string[] names) where TEnum : struct {
            object val = Get(names);
            if (val is string) {
                if (string.IsNullOrEmpty(val as string)) return default(TEnum);
                TEnum t;
                if (Enum.TryParse(val as string, true, out t)) return t;
                return default(TEnum);
            }
            if (val is TEnum) return (TEnum) val;
            return default(TEnum);
        }
        /// <summary>
        ///    <para>Returns an array containing the names of the elements under a specific element.</para>
        ///    <para>
        ///       For example, to get the names of all the top-level elements, call GetNames().  To get the names of all the
        ///       settings for a display, call GetNames("NameOfDisplay").
        ///    </para>
        ///    <para>This method will never return null.</para>
        /// </summary>
        public string[] GetNames(params string[] names) {
            string xpath = names == null || names.Length == 0 ? "//" + _tags[0] : MakeXPath(names) + "/" + _tags[names.Length];
            XmlNodeList nodes = _settings.SelectNodes(xpath);
            return nodes == null ? new string[0] : (from XmlNode node in nodes where node?.Attributes != null select node.Attributes["name"].Value).ToArray();
        }
        public void GetWithPropertyInfo(object obj, PropertyInfo pi, params string[] names) {
            if (pi.PropertyType == typeof(int)) pi.SetValue(obj, (int) Get(names), null);
            else if (pi.PropertyType == typeof(bool)) pi.SetValue(obj, (bool) Get(names), null);
            else if (pi.PropertyType == typeof(string)) pi.SetValue(obj, Get(names) as string, null);
            //else if (pi.PropertyType == typeof (Enum)) {
            //   //Enum t = pi.PropertyType as Enum;
            //   //pi.SetValue(obj, GetEnum<t>(names) as pi.PropertyType, null); //Set(names, pi.GetValue(obj, null) as Enum);
            //}
            else if (pi.PropertyType == typeof(IEnumerable<int>)) Set(names, pi.GetValue(obj, null) as IEnumerable<int>);
            else if (pi.PropertyType == typeof(IEnumerable<string>)) Set(names, pi.GetValue(obj, null) as IEnumerable<string>);
            //else if (pi.PropertyType == typeof (INIValue)) Set(names, pi.GetValue(obj, null) as INIValue);
            //else if (pi.PropertyType == typeof (IEnumerable<INIValue>)) Set(names, pi.GetValue(obj, null) as IEnumerable<INIValue>);
        }
        public void Rename(params string[] names) {
            Assert.False(names.Length < 2);
            string[] oldnames = names.Take(names.Length - 1).ToArray();
            string newname = names[names.Length - 1];
            XmlNode node = GetNode(oldnames);
            if (node?.Attributes != null) {
                node.Attributes["name"].Value = newname;
                Save();
            }
        }
        public void SetWithPropertyInfo(object obj, PropertyInfo pi, params string[] names) {
            Set(names, pi.GetValue(obj, null));
            //if (pi.PropertyType == typeof (int)) Set(names, (int) pi.GetValue(obj, null));
            //else if (pi.PropertyType == typeof (bool)) Set(names, (bool) pi.GetValue(obj, null));
            //else if (pi.PropertyType == typeof (string)) Set(names, pi.GetValue(obj, null) as string);
            //else if (pi.PropertyType == typeof (IEnumerable<int>)) Set(names, pi.GetValue(obj, null) as IEnumerable<int>);
            //else if (pi.PropertyType == typeof (IEnumerable<string>)) Set(names, pi.GetValue(obj, null) as IEnumerable<string>);
            //else if (pi.PropertyType == typeof (INIValue)) Set(names, pi.GetValue(obj, null) as INIValue);
            //else if (pi.PropertyType == typeof (IEnumerable<INIValue>)) Set(names, pi.GetValue(obj, null) as IEnumerable<INIValue>);
            //else if (pi.PropertyType.IsEnum) Set(names, pi.GetValue(obj, null) as Enum);
        }
        private void Clear(IEnumerable<string> names) {
            string xpath = MakeXPath(names);
            XmlNode node = _settings.SelectSingleNode(xpath);
            if (node == null) return;
            node.ParentNode?.RemoveChild(node);
            Save();
        }
        private object Get(IEnumerable<string> names) {
            string xpath = MakeXPath(names);
            XmlNode node = _settings.SelectSingleNode(xpath);
            string strval = node?.Attributes?["value"].Value;
            if (strval == null) return null;
            //string[] strvals;
            string typestr = ConvertOldType(node.Attributes["type"].Value) ?? node.Attributes["type"].Value;
            Type t = Type.GetType(typestr);
            if (t == null) {
                Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly asm in asms) {
                    if (typestr.StartsWith(asm.GetName().Name + ".")) {
                        string typename = typestr.Replace(asm.GetName().Name + ".", "");
                        t = asm.GetExportedTypes().FirstOrDefault(type => type.Name == typename);
                        if (t != null) break;
                    }
                }
            }
            if (t == null) return null;
            object o = _getSwitcher.Switch(t, strval);
            if (t.IsEnum && o is string) {
                try {
                    return Enum.Parse(t, o as string);
                }
                catch {
                    return Enum.GetValues(t).Cast<object>().ToArray()[0]; // Just return the zero value for this enum.
                }
            }
            Type ct = Switcher<object>.GetContentTypeOfEnumerableType(t);
            if (ct != null && ct.IsEnum && o is string[]) {
                var enums = new Enum[(o as string[]).Length];
                for (int i = 0; i < (o as string[]).Length; ++i) {
                    try {
                        enums[i] = (Enum) Enum.Parse(ct, (o as string[])[i]);
                    }
                    catch {
                        enums[i] = (Enum) Enum.GetValues(t).Cast<object>().ToArray()[0];
                    }
                }
                return enums;
            }
            return o;
            //int ii;
            //switch (node.Attributes["type"].Value) {
            //   case "int":
            //      return int.TryParse(strval, out ii) ? ii : 0;
            //   case "bool":
            //      bool b;
            //      if (bool.TryParse(strval, out b)) return b;
            //      return false;
            //   case "enum": // Get treats enums as strings.  To get an enum, use GetEnum<Type>().
            //   case "string":
            //      return strval;
            //   case "enumerable-int":
            //      strvals = strval.Split(',');
            //      int[] ivals = new int[strvals.Length];
            //      for (ii = 0; ii < strvals.Length; ++ii) {
            //         ivals[ii] = 0;
            //         int.TryParse(strvals[ii], out ivals[ii]);
            //      }
            //      return ivals;
            //   case "enumerable-string":
            //      strvals = strval.Split(',');
            //      for (ii = 0; ii < strvals.Length; ++ii) {
            //         strvals[ii] = strvals[ii].Replace("!@c0mM4@!", ",");
            //      }
            //      return strvals;
            //   case "daypart":
            //      return Display.DayPart.FromINIString(strval);
            //   case "enumerable-daypart":
            //      strvals = strval.Split(',');
            //      Display.DayPart[] dps = new Display.DayPart[strvals.Length];
            //      for (ii = 0; ii < strvals.Length; ++ii) {
            //         dps[ii] = Display.DayPart.FromINIString(strvals[ii]);
            //      }
            //      return dps;
            //}
            //return null;
        }
        private XmlNode GetNode(IEnumerable<string> names) {
            if (names == null) return null;
            string[] namesa = names as string[] ?? names.ToArray();
            string xpath = MakeXPath(namesa);
            if (string.IsNullOrEmpty(xpath)) return null;
            XmlNode node = _settings.SelectSingleNode(xpath);
            if (node != null) return node;
            // if we get here, the setting doesn't exist, so we have to create it.
            XmlElement elem;
            if (namesa.Length == 1) {
                elem = _settings.CreateElement(_tags[0]);
                elem.SetAttribute("name", namesa[0]);
                elem.SetAttribute("type", "unknown");
                elem.SetAttribute("value", "");
                if (_settings.DocumentElement == null) throw new Exception("DocumentElement is null in Settings.GetNode");
                _settings.DocumentElement.AppendChild(elem);
            }
            else {
                XmlNode parent = GetNode(namesa.Take(namesa.Length - 1));
                elem = _settings.CreateElement(_tags[namesa.Length - 1]);
                elem.SetAttribute("name", namesa[namesa.Length - 1]);
                elem.SetAttribute("type", "unknown");
                elem.SetAttribute("value", "");
                parent.AppendChild(elem);
            }
            return _settings.SelectSingleNode(xpath);
        }
        private void LoadSettings() {
            if (_settings == null) {
                _settings = new XmlDocument();
                if (File.Exists(_xmlfile)) _settings.Load(_xmlfile);
                else {
                    _settings.LoadXml("<settings></settings>");
                    SaveXmlWithRetries(_settings, _xmlfile);
                }
            }
        }
        private string MakeXPath(IEnumerable<string> names) {
            if (names == null) return null;
            string[] namesa = names as string[] ?? names.ToArray();
            string xpath = "//" + _tags[0] + "[@name='" + namesa[0] + "']";
            for (int i = 1; i < namesa.Length; ++i) xpath += "/" + _tags[i] + "[@name='" + namesa[i] + "']";
            return xpath;
        }
        private void Save() {
            SaveXmlWithRetries(_settings, _xmlfile);
        }
        private void Set(IEnumerable<string> names, object value) {
            if (value == null) {
                Clear(names);
                return;
            }
            XmlNode node = GetNode(names);
            if (node?.Attributes == null) return;
            node.Attributes["type"].Value = value.GetType().FullName;
            //--------------------------------------------------------------
            // All the crap between this comment and the next is to convert an array or List of some enumerated type to an IEnumerable<Enum> 
            // that won't blow up when you actually try to iterate through it.
            Type t = Switcher<object>.GetContentTypeOfEnumerableType(value.GetType());
            IEnumerable<Enum> b = null;
            if (t != null && t.IsEnum) {
                var a = value as Array; // if value is an array of an enums, then this will work, and Array a will not be null.
                if (a != null) b = a.Cast<Enum>(); // This gets us an IEnumerable<Enum> from an array of enums.
                else if (typeof(List<>).MakeGenericType(t) == value.GetType()) {
                    // this will be true if value is a List of enums.
                    var z = value as IList;
                    if (z != null) b = z.Cast<Enum>(); // This gets us an IEnumerable<Enum> from a List of enums.
                }
            }
            // At this point, if b is not null, then it is an IEnumerable<Enum>.
            //--------------------------------------------------------------
            node.Attributes["value"].Value = _setSwitcher.Switch(b ?? value);
            Save();
        }
        private static string ConvertOldType(string s) {
            switch (s) {
                case "int":
                    return typeof(int).FullName;
                case "bool":
                    return typeof(bool).FullName;
                case "enum":
                    return typeof(Enum).FullName;
                case "string":
                    return typeof(string).FullName;
                case "enumerable-int":
                    return typeof(IEnumerable<int>).FullName;
                case "enumerable-string":
                    return typeof(IEnumerable<string>).FullName;
            }
            return null;
        }
        public static void SaveXmlWithRetries(XmlDocument xml, string file, bool retry = true, int timeoutsecs = 5) {
            if (!retry) {
                xml.Save(file);
                return;
            }
            DateTime start = DateTime.Now;
            bool success;
            int tries = 0;
            do {
                ++tries;
                try {
                    success = true;
                    xml.Save(file);
                }
                catch (Exception ex) {
                    Trace.WriteLine("Failed to save \"" + file + "\". Exception follows:" + Environment.NewLine + ex);
                    success = false;
                    Thread.Sleep(50);
                }
            } while (!success && (DateTime.Now - start).TotalSeconds < timeoutsecs);
            if (success) {
                // Only log success if we had to retry.
                if (tries > 1) Trace.WriteLine("After " + tries + " tries, successfully saved file \"" + file + "\".");
            }
            else Trace.WriteLine("Failed to save file \"" + file + "\".");
        }
        /*----------------------*/
        /* Data                 */
        /*----------------------*/
        private readonly Switcher<object> _getSwitcher = //
            new Switcher<object>().Case<int, string>(s => {
                                                         int i;
                                                         return int.TryParse(s, out i) ? i : 0;
                                                     }) //
                                  .Case<Int64, string>(s => {
                                                           Int64 i;
                                                           return Int64.TryParse(s, out i) ? i : 0;
                                                       }) //
                                  .Case<bool, string>(s => {
                                                          bool b;
                                                          return bool.TryParse(s, out b) && b;
                                                      }) //
                                  .Case<string, string>(s => s) //
                                  .Case<Enum, string>(s => s) //
                                  .Case<Color, string>(s => {
                                                           if (string.IsNullOrEmpty(s)) return null;
                                                           string[] strvals = s.Split(',');
                                                           if (strvals.Length != 4) return null;
                                                           return Color.FromArgb(int.Parse(strvals[0]), int.Parse(strvals[1]), int.Parse(strvals[2]), int.Parse(strvals[3]));
                                                       })
                                  //.Case<INIValue, string>(INIValue.FromINIString) //
                                  .Case<IEnumerable<int>, string>(s => {
                                                                      string[] strvals = s.Split(',');
                                                                      var ivals = new int[strvals.Length];
                                                                      for (int i = 0; i < strvals.Length; ++i) {
                                                                          ivals[i] = 0;
                                                                          int.TryParse(strvals[i], out ivals[i]);
                                                                      }
                                                                      return ivals;
                                                                  }) //
                                  .Case<IEnumerable<Enum>, string>(s => {
                                                                       string[] strvals = s.Split(',');
                                                                       return strvals;
                                                                   }) //
                                  .Case<IEnumerable<bool>, string>(s => {
                                                                       string[] strvals = s.Split(',');
                                                                       var bvals = new bool[strvals.Length];
                                                                       for (int i = 0; i < strvals.Length; ++i) {
                                                                           bvals[i] = false;
                                                                           bool.TryParse(strvals[i], out bvals[i]);
                                                                       }
                                                                       return bvals;
                                                                   }) //
                                  .Case<IEnumerable<string>, string>(s => {
                                                                         string[] strvals = s.Split(',');
                                                                         for (int i = 0; i < strvals.Length; ++i) strvals[i] = strvals[i].Replace("!@c0mM4@!", ",");
                                                                         return strvals;
                                                                     }) //
                                  //.Case<IEnumerable<INIValue>, string>(s => {
                                  //                                        string[] strvals = s.Split(',');
                                  //                                        var ivs = new INIValue[strvals.Length];
                                  //                                        for (int i = 0; i < strvals.Length; ++i) ivs[i] = INIValue.FromINIString(strvals[i]);
                                  //                                        return ivs;
                                  //                                     }) //
                                  .Case<Size, string>(s => {
                                                          string[] strvals = s.Split(',');
                                                          int w, h;
                                                          if (strvals.Length == 2 && int.TryParse(strvals[0], out w) && int.TryParse(strvals[1], out h))
                                                              return new Size(w, h);
                                                          return new Size(0, 0);
                                                      }) //
                                  .Case<Point, string>(s => {
                                                           string[] strvals = s.Split(',');
                                                           int x, y;
                                                           if (strvals.Length == 2 && int.TryParse(strvals[0], out x) && int.TryParse(strvals[1], out y))
                                                               return new Point(x, y);
                                                           return new Point(0, 0);
                                                       });
        private readonly object _lockobj = new object();
        private readonly Switcher<string> _setSwitcher = //
            new Switcher<string>().Case<int>(x => x.ToString())
                                  .Case<Int64>(x => x.ToString())
                                  .Case<bool>(x => x.ToString())
                                  .Case<string>(x => x)
                                  .Case<Enum>(x => x.ToString())
                                  .Case<Color>(x => x.A + "," + x.R + "," + x.G + "," + x.B)
                                  //.Case<INIValue>(x => x.ToINIString())
                                  .Case<IEnumerable<int>>(x => x.Aggregate("", (current, i) => current + i + ",").TrimEnd(','))
                                  .Case<IEnumerable<bool>>(x => x.Aggregate("", (current, i) => current + i + ",").TrimEnd(','))
                                  .Case<IEnumerable<string>>(x => {
                                                                 string[] sa = x.ToArray();
                                                                 // get rid of all commas, so we can use comma as the separator.
                                                                 for (int i = 0; i < sa.Length; ++i)
                                                                     if (sa[i] != null) sa[i] = sa[i].Replace(",", "!@c0mM4@!");
                                                                 return string.Join(",", sa);
                                                             }) // 
                                  .Case<IEnumerable<Enum>>(x => x.Aggregate("", (current, i) => current + i + ",").TrimEnd(',')) //
                                  //.Case<IEnumerable<INIValue>>(x => x.Aggregate("", (current, part) => current + part.ToINIString() + ",").TrimEnd(',')) //
                                  .Case<Size>(x => x.Width.ToString() + "," + x.Height.ToString()) //
                                  .Case<Point>(x => x.X.ToString() + "," + x.Y.ToString());
        private XmlDocument _settings;
        private readonly string[] _tags;
        private readonly string _xmlfile;
    }
}