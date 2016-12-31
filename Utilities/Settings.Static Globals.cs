using System;
using System.IO;
using System.Windows.Forms;

namespace Utils {
    public partial class Settings {
        /*----------------------*/
        /* Static constructor   */
        /*----------------------*/
        static Settings() {
            _slockobj = new object();
            //_displaydefaults = new Settings(DisplaysFile, "default");
            //_displaysettings = new Settings(DisplaysFile, "display", "setting");
            //_globalsettings = new Settings(SettingsFile, "setting");
            //_ifc = new InstalledFontCollection();
        }
        /*----------------------*/
        /* Properties           */
        /*----------------------*/
        /// <summary>
        ///     <para>Gets the data folder for this application, e.g. "C:\ProgramData\NCR\AccuVIEW III\".</para>
        ///     <para>
        ///         This path is specified in the registry entry HKEY_CURRENT_USER\SOFTWARE\NCR\AccuVIEW III\DataFolder.
        ///     </para>
        ///     <para>Only gets the registry value once.  After that, just returns the value previous obtained.</para>
        ///     <para>This value will always include a final backslash.</para>
        /// </summary>
        public static string DataFolder {
            get {
                lock (_slockobj) {
                    //   if (_dataFolder == null) {
                    //      try {
                    //         RegistryKey regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\NCR\\AccuVIEW III") ?? Registry.CurrentUser.CreateSubKey("SOFTWARE\\NCR\\AccuVIEW III");
                    //         // ReSharper disable PossibleNullReferenceException
                    //         _dataFolder =
                    //            (regkey.GetValue("DataFolder") ?? (Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\NCR\\Accuview III")).ToString();
                    //         if (!Directory.Exists(_dataFolder)) Directory.CreateDirectory(_dataFolder);
                    //         // ReSharper restore PossibleNullReferenceException
                    //         if (!_dataFolder.EndsWith("\\")) _dataFolder += "\\";
                    //      }
                    //      catch (Exception ex) {
                    //         Tracer.Log("Registry entries are missing. AccuVIEW must be reinstalled. Exception:" + Environment.NewLine + ex);

                    //         MessageBox.Show("Registry entries are missing. AccuVIEW must be reinstalled.");
                    //      }
                    //   }
                    return _dataFolder;
                }
            }
            set {
                _dataFolder = value;
                CreateFolders();
                _globalsettings = new Settings(SettingsFile, "setting");
            }
        }
        public static Form MainForm { get; set; }
        //               if (!_ngConfigExeFolder.EndsWith("\\")) _ngConfigExeFolder += "\\";
        //               if (!File.Exists(_ngConfigExeFolder + "NGConfigD.exe")) {
        //                  var dlg = new FolderBrowserDialog {Description = "Use this dialog to browse to the location of NGConfigD.exe.", ShowNewFolderButton = true,};
        //                  if (dlg.ShowDialog(MainForm) == DialogResult.OK && File.Exists(dlg.SelectedPath + "\\NGConfigD.exe")) {
        //                     regkey.SetValue("NGConfigExeFolder", dlg.SelectedPath);
        //                     _ngConfigExeFolder = dlg.SelectedPath;
        //                     if (!_ngConfigExeFolder.EndsWith("\\")) _ngConfigExeFolder += "\\";
        //                  }
        //                  else {
        //                     throw new Exception("NGConfigD.exe not found.");
        //                  }
        //               }
        //            }
        //            catch {
        //               MessageBox.Show("Registry entries are missing. AccuVIEW must be reinstalled.");
        //               return "";
        //            }
        //         }
        //         return _ngConfigExeFolder;
        //      }
        //   }
        //}
        /// <summary>
        ///     The full path to the settings.xml file.
        /// </summary>
        public static string SettingsFile {
            get { return DataFolder + _settingsFilename; }
            set {
                string filename = Path.GetFileName(value);
                if (string.IsNullOrEmpty(filename) || String.Compare(filename, _settingsFilename, StringComparison.OrdinalIgnoreCase) == 0) return;
                _settingsFilename = filename;
                _globalsettings = new Settings(SettingsFile, "setting");
            }
        }
        /// <summary>
        ///     <para>Gets the path to the Templates folder.  Always includes the final backslash.</para>
        /// </summary>
        public static string TemplatesFolder {
            get {
                string str = DataFolder + "Templates\\";
                if (!Directory.Exists(str)) Directory.CreateDirectory(str);
                return str;
            }
        }
        //public static Settings DisplayDefaults {
        //   get { return _displaydefaults; }
        //}
        //public static Settings Displays {
        //   get { return _displaysettings; }
        //}
        ///// <summary>
        /////    The full path to the displays.xml file.
        ///// </summary>
        //public static string DisplaysFile {
        //   get { return DataFolder + "Displays.xml"; }
        //}
        //public static string[] FontNames {
        //   get { return _ifc.Families.Select(f => f.Name).ToArray(); }
        //}
        public static Settings Values {
            get {
                if (_globalsettings == null) _globalsettings = new Settings(SettingsFile, "setting");
                return _globalsettings;
            }
        }
        public static string ZipFolder {
            get {
                string str = DataFolder + "Zip\\";
                if (!Directory.Exists(str)) Directory.CreateDirectory(str);
                return str;
            }
        }
        /*----------------------*/
        /* Methods              */
        /*----------------------*/
        public static void CreateFolders() {
            if (!Directory.Exists(DataFolder)) Directory.CreateDirectory(DataFolder);
            //if (!Directory.Exists(DataFolder + "Displays")) Directory.CreateDirectory(DataFolder + "Displays");
            //if (!Directory.Exists(DataFolder + "Templates")) Directory.CreateDirectory(DataFolder + "Templates");
        }
        //public static void CreateNGConfigRegistry() {
        //   RegistryKey regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\NCR\\NGConfig", true) ?? Registry.CurrentUser.CreateSubKey("SOFTWARE\\NCR\\NGConfig");
        //   if (regkey != null && !regkey.GetValueNames().Contains("DataFolder")) regkey.SetValue("DataFolder", DataFolder, RegistryValueKind.String);
        //}
        //public static Display GetDisplay(string name, bool forceRefresh = false) {
        //   if (_displays == null) _displays = new List<Display>();
        //   Display display = _displays.FirstOrDefault(d => String.Compare(d.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
        //   Display oldInstance = display;
        //   if (forceRefresh) {
        //      _displays.RemoveAll(d => String.Compare(d.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
        //      display = null;
        //   }
        //   if (display == null && _displaysettings.GetNames().Any(s => string.Compare(s, name, StringComparison.OrdinalIgnoreCase) == 0)) {
        //      display = Display.FromSettings(name, oldInstance);
        //      _displays.Add(display);
        //   }
        //   return display;
        //}
        //public static Font GetFont(string name, float size, bool bold, bool italic) {
        //   FontFamily[] families = _ifc.Families;
        //   FontFamily family = families.FirstOrDefault(f => f.Name == name);
        //   if (family == null) return null;
        //   var s = FontStyle.Regular;
        //   if (bold) s |= FontStyle.Bold;
        //   if (italic) s |= FontStyle.Italic;
        //   return new Font(family, size, s, GraphicsUnit.Pixel);
        //}
        //public static void RegisterDisplay(Display disp) {
        //   if (_displays == null) _displays = new List<Display>();
        //   _displays.RemoveAll(d => String.Compare(d.Name, disp.Name, StringComparison.OrdinalIgnoreCase) == 0);
        //   _displays.Add(disp);
        //}
        /*----------------------*/
        /* Data                 */
        /*----------------------*/
        private static string _dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\QuickCOM\\SimData\\";
        //private static readonly Settings _displaydefaults;
        //private static List<Display> _displays;
        //private static readonly Settings _displaysettings;
        private static Settings _globalsettings;
        private static string _settingsFilename = "Settings.xml";
        //private static readonly InstalledFontCollection _ifc;
        //private static string _ngConfigDataFolder;
        //private static string _ngConfigExeFolder;
        private static readonly object _slockobj;
        ///// <summary>
        /////    <para>
        /////       Gets path to the the data folder used by NGConfigD.exe.  This path is used to locate the NGOCUList.txt file
        /////       that NGConfig outputs after it's discovery process.
        /////    </para>
        /////    <para>
        /////       This path is specified in the registry entry HKEY_CURRENT_USER\SOFTWARE\NCR\AccuVIEW III\NGConfigDataFolder.
        /////    </para>
        /////    <para>Only gets the registry value once.  After that, just returns the value previous obtained.</para>
        /////    <para>This value will always include a final backslash.</para>
        ///// </summary>
        //public static string NGConfigDataFolder {
        //   get {
        //      lock (_slockobj) {
        //         if (_ngConfigDataFolder == null) {
        //            try {
        //               RegistryKey regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\NCR\\AccuVIEW III") ?? Registry.CurrentUser.CreateSubKey("SOFTWARE\\NCR\\AccuVIEW III");
        //               // ReSharper disable PossibleNullReferenceException
        //               _ngConfigDataFolder = regkey.GetValueNames().Contains("NGConfigDataFolder") ? regkey.GetValue("NGConfigDataFolder").ToString() : DataFolder;
        //               if (!Directory.Exists(_ngConfigDataFolder)) Directory.CreateDirectory(_ngConfigDataFolder);
        //               // ReSharper restore PossibleNullReferenceException
        //               if (!_ngConfigDataFolder.EndsWith("\\")) _ngConfigDataFolder += "\\";
        //            }
        //            catch {
        //               MessageBox.Show("Registry entries are missing. AccuVIEW must be reinstalled.");
        //            }
        //         }
        //         return _ngConfigDataFolder;
        //      }
        //   }
        //}
        ///// <summary>
        /////    <para>Gets path to the folder where NGConfigD.exe is located.</para>
        /////    <para>
        /////       This path is specified in the registry entry HKEY_CURRENT_USER\SOFTWARE\NCR\AccuVIEW III\NGConfigExeFolder.
        /////    </para>
        /////    <para>Only gets the registry value once.  After that, just returns the value previous obtained.</para>
        /////    <para>This value will always include a final backslash.</para>
        ///// </summary>
        //public static string NGConfigExeFolder {
        //   get {
        //      lock (_slockobj) {
        //         if (_ngConfigExeFolder == null) {
        //            try {
        //               RegistryKey regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\NCR\\AccuVIEW III", true) ?? Registry.CurrentUser.CreateSubKey("SOFTWARE\\NCR\\AccuVIEW III");
        //               // ReSharper disable PossibleNullReferenceException
        //               _ngConfigExeFolder = regkey.GetValueNames().Contains("NGConfigExeFolder") ? regkey.GetValue("NGConfigExeFolder").ToString() : DataFolder;
        //               if (!Directory.Exists(_ngConfigExeFolder)) Directory.CreateDirectory(_ngConfigExeFolder);
        //               // ReSharper restore PossibleNullReferenceException
    }
}