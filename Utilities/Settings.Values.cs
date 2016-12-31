using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using Utilities.Properties;

namespace Utilities {
    public partial class Settings {
        /*----------------------*/
        /* Static constructor   */
        /*----------------------*/
        static Settings() {
            _slockobj = new object();
            if (!File.Exists(SettingsFile)) {
                try {
                    File.WriteAllText(SettingsFile, Resources.SettingsXml);
                }
                catch (Exception ex) {
                    MessageBox.Show("Unable to load settings file and failed to create default file (from application default)." + Environment.NewLine +
                                    "Exception message:" + Environment.NewLine + Environment.NewLine +
                                    ex.Message);
                    return;
                }
                _globalsettings = new Settings(SettingsFile, "setting");
                string[] sites = Values["sites"] as string[];
                if (sites == null) {
                    MessageBox.Show("Unable to load settings file (copied from IODP default).  Contact an IODP developer.");
                    return;
                }
                for (int i = 0; i < sites.Length; ++i) sites[i] = sites[i].Replace("$APPNAMELOWER$", ApplicationName.ToLower()).Replace("$APPNAME$", ApplicationName);
                Values["sites"] = sites;
            }
            else {
                _globalsettings = new Settings(SettingsFile, "setting");
                // Now I will read the settings.xml file in Properties.Resources in as a Settings instance, and overwrite the "sites" value in _globalsettings with whatever's in
                // there.
                File.WriteAllText(SettingsFile + ".$$$", Resources.SettingsXml);
                var tempsettings = new Settings(SettingsFile + ".$$$", "setting");
                string[] sites = (string[]) tempsettings["sites"];
                if (sites != null) {
                    for (int i = 0; i < sites.Length; ++i) sites[i] = sites[i].Replace("$APPNAMELOWER$", ApplicationName.ToLower()).Replace("$APPNAME$", ApplicationName);
                    Values["sites"] = sites;
                }
                File.Delete(SettingsFile + ".$$$");
            }
            //if (DeployedFromName == "ERROR") _globalsettings = null;
            //else CurrentSiteName = DeployedFromName;
        }
        /*----------------------*/
        /* Properties           */
        /*----------------------*/
        public static string ApplicationName => Application.ProductName;
        //public static string ApplicationVersion {
        //    get {
        //        try {
        //            return string.IsNullOrEmpty(DeployedFromUrl) ? "NOT DEPLOYED" : ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
        //        }
        //        catch {
        //            return "NOT DEPLOYED";
        //        }
        //    }
        //}
        //public static string CurrentPassword {
        //    get {
        //        string pw = Values["password"] as string;
        //        if (pw == null) {
        //            var aes = new AesCrypto_OLD();
        //            pw = aes.Encrypt("guest", Passphrase, SALT, IV, 10000);
        //        }
        //        return pw;
        //    }
        //    set { Values["password"] = value; }
        //}
        //public static JSONPrivilege[] CurrentPrivileges { get; set; }
        //public static string CurrentSiteName {
        //    get {
        //        if (Values == null) return null;
        //        string site = Values["currentsite"] as string;
        //        if (site == null) Values["currentsite"] = DeployedFromName;
        //        return Values["currentsite"] as string;
        //    }
        //    set {
        //        var name = value.ToUpper();
        //        if (!SiteNames.Contains(name)) throw new Exception("Attempted to set current site name to an unknown name");
        //        Values["currentsite"] = name;
        //    }
        //}
        /// <summary>
        ///     Returns the current site url, or empty string if the current site name is not in the site list (because it's "Not
        ///     deployed").<br />
        ///     Note that this value includes only the base of the url.  It will not include a final slash, or any
        ///     "tasapps/whatever.application" or anything like that.
        /// </summary>
        /// <summary>
        ///     <para>Gets the data folder for this application, e.g. "C:\ProgramData\IODP\[product name]\".</para>
        ///     <para>
        ///         This path is specified in the registry entry HKEY_LOCAL_MACHINE\SOFTWARE\IODP\[product name]\DataFolder.
        ///     </para>
        ///     <para>Only gets the registry value once.  After that, just returns the value previous obtained.</para>
        ///     <para>This value will always include a final backslash.</para>
        /// </summary>
        public static string DataFolder {
            get {
                lock (_slockobj) {
                    if (_dataFolder == null) {
                        try {
                            RegistryKey regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\TIM\\" + Application.ProductName) ??
                                                 Registry.CurrentUser.CreateSubKey("SOFTWARE\\TIM\\" + Application.ProductName,
                                                                                   RegistryKeyPermissionCheck.ReadWriteSubTree,
                                                                                   RegistryOptions.Volatile);
                            // ReSharper disable PossibleNullReferenceException
                            _dataFolder = (regkey.GetValue("DataFolder")
                                           ?? (Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\TIM\\" + Application.ProductName)).ToString();
                            if (!Directory.Exists(_dataFolder)) Directory.CreateDirectory(_dataFolder);
                            // ReSharper restore PossibleNullReferenceException
                            if (!_dataFolder.EndsWith("\\")) _dataFolder += "\\";
                        }
                        catch (Exception ex) {
                            var msg = "Registry entries are missing. " + Application.ProductName + " must be reinstalled.";
                            Trace.WriteLine(msg + " Exception:" + Environment.NewLine + ex);
                            MessageBox.Show(msg);
                        }
                    }
                    return _dataFolder;
                }
            }
        }
        /// <summary>
        ///     The default settings file deployed with the application for use only if IODPSettingsFile does not exist.
        /// </summary>
        public static string DefaultSettingsFile => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Settings.xml";
        ///// <summary>
        /////     Returns one of the site keys from Sites ("SHIP", "SHORE", etc.), or "not deployed", as follows:
        /////     1. If the command line has "deployedfrom=SITE" then the site specified is returned (which MUST be one of the sites
        /////     defined in Sites).
        /////     2. Otherwise, if this app is network deployed, returns the site it was deployed from.
        /////     3. Otherwise, returns "NOT DEPLOYED".
        ///// </summary>
        //public static string DeployedFromName {
        //    get {
        //        if (_deployedFrom != null) return _deployedFrom;
        //        string str = Environment.GetCommandLineArgs().FirstOrDefault(s => s.ToLower().Substring(1).StartsWith("deployedfrom"));
        //        if (str != null) {
        //            string url = str.Split('=')[1];
        //            _deployedFrom = Sites.Keys.FirstOrDefault(key => Sites[key].Equals(url, StringComparison.OrdinalIgnoreCase));
        //            if (_deployedFrom == null) {
        //                var newsites = Sites;
        //                if (!newsites.ContainsKey("CUSTOM")) newsites.Add("CUSTOM", url);
        //                else newsites["CUSTOM"] = url;
        //                Sites = newsites;
        //                _deployedFrom = "CUSTOM";
        //            }
        //        }
        //        else if (ApplicationDeployment.IsNetworkDeployed) {
        //            //string url = ApplicationDeployment.CurrentDeployment.UpdateLocation.AbsoluteUri.Replace(ApplicationName + ".application", "");
        //            //if (url.Contains("/tasapps")) url = url.Substring(0, url.IndexOf("/tasapps", StringComparison.Ordinal) + 1);
        //            string url = ApplicationDeployment.CurrentDeployment.UpdateLocation.AbsoluteUri;
        //            string name = Sites.Keys.FirstOrDefault(n => Sites[n].Contains(url));
        //            if (name != null) _deployedFrom = name;
        //            else {
        //                var newsites = Sites;
        //                if (!newsites.ContainsKey("CUSTOM")) newsites.Add("CUSTOM", url);
        //                else newsites["CUSTOM"] = url;
        //                Sites = newsites;
        //                _deployedFrom = "CUSTOM";
        //            }
        //        }
        //        else {
        //            MessageBox.Show(
        //                "This application does not appear to have been deployed.\n\n" +
        //                "If you believe you are getting this message in error, please contact an IODP developer for assistance.\n\n" +
        //                "If you want to run it for testing purposes, you must provide a command-line argument telling it where to consider itself deployed from. " +
        //                "The command-line argument should be something like:\n\n/deployedfrom=http://web.ship.iodp.tamu.edu/tasapps/" + ApplicationName.ToLower() + "/" +
        //                ApplicationName + ".application\n\nNote that the full path to the .application file must be included.");
        //            _deployedFrom = "ERROR";
        //        }
        //        return _deployedFrom;
        //    }
        //}
        ///// <summary>
        /////     Returns the url of the deployed-from site, or an empty string if the app was not deployed.
        ///// </summary>
        //public static string DeployedFromUrl {
        //    get {
        //        if (DeployedFromName == "Not deployed") return "";
        //        return Sites[DeployedFromName];
        //    }
        //}
        ///// <summary>
        /////     The default settings file in ProgramData\IODP.
        ///// </summary>
        //public static string IODPSettingsFile {
        //    get { return DataFolder + "..\\" + "Settings.xml"; }
        //}
        //public static string IV {
        //    get { return "F27D5C9927726BCEFE7510B1BDD3D137"; }
        //}
        //public static Form MainForm { get; set; }
        //public static string Passphrase {
        //    get { return "IODP"; }
        //}
        //public static string SALT {
        //    get { return "3FF2EC019C627B945225DEBAD71A01B6985FE84C95A70EB132882F88C0A59A55"; }
        //}
        /// <summary>
        ///     The full path to the settings.xml file.
        /// </summary>
        public static string SettingsFile => DataFolder + "Settings.xml";
        //public static string[] SiteNames {
        //    get { return Sites.Keys.ToArray(); }
        //}
        //public static Dictionary<string, string> Sites {
        //    get {
        //        if (_sites != null) return _sites;
        //        _sites = new Dictionary<string, string>();
        //        // Read the sites from settings.
        //        var sites = Values["sites"] as string[];
        //        if (sites == null) {
        //            sites = new[] {
        //                              "SHORE|http://web.iodp.tamu.edu/tasapps/" + ApplicationName.ToLower() + "/" + ApplicationName + ".application",
        //                              "SHIP|http://web.ship.iodp.tamu.edu/tasapps/" + ApplicationName.ToLower() + "/" + ApplicationName + ".application",
        //                              "SHORT|http://165.91.52.41:8080/tasapps/" + ApplicationName.ToLower() + "/" + ApplicationName + ".application",
        //                          };
        //            Values["sites"] = sites;
        //        }
        //        // Create the sites dictionary
        //        foreach (string site in sites) {
        //            var strs = site.Split('|');
        //            _sites.Add(strs[0], strs[1]);
        //        }
        //        return _sites;
        //    }
        //    set {
        //        if (value == null) return;
        //        string[] sites = new string[value.Count];
        //        string[] names = value.Keys.ToArray();
        //        for (int i = 0; i < value.Count; ++i) {
        //            sites[i] = names[i] + "|" + value[names[i]];
        //        }
        //        Values["sites"] = sites;
        //    }
        //}
        public static Settings Values => _globalsettings;
        /*----------------------*/
        /* Methods              */
        /*----------------------*/
        public static void CreateFolders() {
            if (!Directory.Exists(DataFolder)) Directory.CreateDirectory(DataFolder);
        }
        public static T Get<T>(params string[] names) {
            if (_globalsettings == null) return default(T);
            object o = _globalsettings.Get(names);
            if (o is T) return (T) o;
            try {
                o = _globalsettings._getSwitcher.Switch(typeof(T), (o ?? "").ToString());
                if (o is T) return (T) o;
            }
            catch {
                // ignored
            }
            return default(T);
        }
        //public static string GetSiteUrl(string name) {
        //    string url = Sites.ContainsKey(name) ? Sites[name] : "";
        //    if (url.Contains("//")) {
        //        int i = url.IndexOf('/', url.IndexOf("//", StringComparison.Ordinal) + 2);
        //        if (i > 0) url = url.Substring(0, i);
        //    }
        //    return url;
        //}
        public static void Put(object value, params string[] names) {
            _globalsettings[names] = value;
        }
        public static void SetDefaults(Dictionary<string, object> defaults) {
            foreach (string key in defaults.Keys) {
                if (Values[key] == null) Values[key] = defaults[key];
            }
        }
        //public static Bitmap SiteImage(string sitename = null) {
        //    if (sitename == null) sitename = CurrentSiteName;
        //    switch (sitename) {
        //        case "SHIP":
        //            return Resources.joidesresolution;
        //        case "SHORE":
        //            return Resources.iodpcore;
        //        default:
        //            return Resources.rtif;
        //    }
        //}
        private static string _dataFolder;
        /*----------------------*/
        /* Data                 */
        /*----------------------*/
        //private static string _deployedFrom;
        private static readonly Settings _globalsettings;
        //private static Dictionary<string, string> _sites;
        private static readonly object _slockobj;
    }
}