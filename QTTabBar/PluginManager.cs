//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using QTPlugin;

namespace QTTabBarLib {
    internal sealed class PluginManager {
        public Dictionary<string, string> dicFullNamesMenuRegistered_Sys = new Dictionary<string, string>();
        public Dictionary<string, string> dicFullNamesMenuRegistered_Tab = new Dictionary<string, string>();
        private static Dictionary<string, PluginAssembly> dicPluginAssemblies = new Dictionary<string, PluginAssembly>();
        private Dictionary<string, Plugin> dicPluginInstances = new Dictionary<string, Plugin>();
        private static Dictionary<string, Plugin> dicStaticPluginInstances = new Dictionary<string, Plugin>();
        private int iClosingCount;
        private int iRefCount;
        private static List<PluginButton> lstPluginButtonsOrder = new List<PluginButton>();
        private static IEncodingDetector plgEncodingDetector;
        private IFilterCore plgFilterCore;
        private IFilter plgIFilter;
        private QTTabBarClass.PluginServer pluginServer;

        public PluginManager(QTTabBarClass tabBar) {
            pluginServer = new QTTabBarClass.PluginServer(tabBar, this);
            LoadStartupPlugins();
            iRefCount++;
        }

        public static void AddAssembly(PluginAssembly pa) {
            PluginAssembly assembly;
            if(dicPluginAssemblies.TryGetValue(pa.Path, out assembly) && (assembly != pa)) {
                assembly.Dispose();
            }
            dicPluginAssemblies[pa.Path] = pa;
        }

        public void AddRef() {
            iRefCount++;
        }

        public void ClearFilterEngines() {
            plgIFilter = null;
            plgFilterCore = null;
        }

        public static void ClearIEncodingDetector() {
            plgEncodingDetector = null;
        }

        public void Close(bool fInteractive) {
            iRefCount--;
            if(iClosingCount == 0) {
                pluginServer.ClearEvents();
            }
            foreach(Plugin plugin in dicPluginInstances.Values) {
                if((plugin.PluginInformation != null) && (fInteractive ^ (plugin.PluginInformation.PluginType != PluginType.Interactive))) {
                    plugin.Close(EndCode.WindowClosed);
                }
            }
            if(!fInteractive) {
                plgIFilter = null;
                plgFilterCore = null;
            }
            if(iRefCount == 0) {
                dicPluginInstances.Clear();
                pluginServer.Dispose();
                pluginServer = null;
            }
            iClosingCount++;
        }

        public static void HandlePluginException(Exception ex, IntPtr hwnd, string pluginID, string strCase) {
            MessageForm.Show(hwnd, "Error : " + strCase + "\r\nPlugin : \"" + pluginID + "\"\r\nErrorType : " + ex, "Plugin Error", MessageBoxIcon.Hand, 0x7530);
        }

        public static void Initialize() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + @"Plugins\Paths")) {
                if(key == null) return;

                string[] enabled = Config.Plugin.Enabled;
                foreach(string str in key.GetValueNames()) {
                    string path = (string)key.GetValue(str, string.Empty);
                    if(path.Length > 0) {
                        PluginAssembly pa = new PluginAssembly(path);
                        if(pa.PluginInfosExist) {
                            foreach(PluginInformation information in pa.PluginInformations
                                    .Where(information => enabled.Contains(information.PluginID))) {
                                information.Enabled = true;
                                pa.Enabled = true;
                                if(information.PluginType == PluginType.Static) {
                                    LoadStatics(information, pa, false);
                                }
                            }                                
                            dicPluginAssemblies[path] = pa;
                        }
                    }
                }
            }
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + @"Plugins")) {
                if(key != null) {
                    PluginButton[] buttons = QTUtility2.ReadRegBinary<PluginButton>("Buttons_Order", key);
                    string[] keys = QTUtility2.ReadRegBinary<string>("ShortcutKeyIDs", key);
                    int[][] values = QTUtility2.ReadRegBinary<int[]>("ShortcutKeyValues", key);
                    if(buttons != null) lstPluginButtonsOrder.AddRange(buttons);
                    if(keys != null && values != null) {
                        for(int i = 0; i < Math.Min(keys.Length, values.Length); ++i) {
                            QTUtility.dicPluginShortcutKeys[keys[i]] = values[i];
                        }
                    }
                }
            }
        }

        public string InstanceToFullName(IPluginClient pluginClient, bool fTypeFullName) {
            Plugin plugin = dicPluginInstances.Values.FirstOrDefault(plugin1 => plugin1.Instance == pluginClient);
            return plugin == null
                    ? null
                    : fTypeFullName
                            ? plugin.PluginInformation.TypeFullName
                            : plugin.PluginInformation.PluginID;
        }

        public Plugin Load(PluginInformation pi, PluginAssembly pa) {
            try {
                if((pa == null) && !dicPluginAssemblies.TryGetValue(pi.Path, out pa)) {
                    return null;
                }
                Plugin plugin = pa.Load(pi.PluginID);
                if(plugin != null) {
                    string[] strArray;
                    int[] numArray;
                    dicPluginInstances[pi.PluginID] = plugin;
                    if((!pluginServer.OpenPlugin(plugin.Instance, out strArray) || (strArray == null)) || (strArray.Length <= 0)) {
                        return plugin;
                    }
                    pi.ShortcutKeyActions = strArray;
                    if(QTUtility.dicPluginShortcutKeys.TryGetValue(pi.PluginID, out numArray)) {
                        if(numArray == null) {
                            QTUtility.dicPluginShortcutKeys[pi.PluginID] = new int[strArray.Length];
                            return plugin;
                        }
                        if(numArray.Length != strArray.Length) {
                            int[] numArray2 = new int[strArray.Length];
                            int num = Math.Min(numArray.Length, strArray.Length);
                            for(int i = 0; i < num; i++) {
                                numArray2[i] = numArray[i];
                            }
                            QTUtility.dicPluginShortcutKeys[pi.PluginID] = numArray2;
                        }
                        return plugin;
                    }
                    QTUtility.dicPluginShortcutKeys[pi.PluginID] = new int[strArray.Length];
                }
                return plugin;
            }
            catch(Exception exception) {
                HandlePluginException(exception, IntPtr.Zero, pi.Name, "Loading plugin.");
                QTUtility2.MakeErrorLog(exception);
            }
            return null;
        }

        private void LoadStartupPlugins() {
            foreach(PluginInformation information in PluginInformations.Where(information => information.Enabled)) {
                if(information.PluginType == PluginType.Background) {
                    Plugin plugin = Load(information, null);
                    if(plugin != null) {
                        if(plgIFilter == null) {
                            plgIFilter = plugin.Instance as IFilter;
                        }
                        if(plgFilterCore == null) {
                            plgFilterCore = plugin.Instance as IFilterCore;
                        }
                    }
                    else {
                        information.Enabled = false;
                    }
                    continue;
                }
                if((information.PluginType == PluginType.BackgroundMultiple) && (Load(information, null) == null)) {
                    information.Enabled = false;
                }
            }
        }

        private static bool LoadStatics(PluginInformation pi, PluginAssembly pa, bool fForce) {
            Plugin plugin = pa.Load(pi.PluginID);
            if((plugin != null) && (plugin.Instance != null)) {
                dicStaticPluginInstances[pi.PluginID] = plugin;
                if((plgEncodingDetector == null) || fForce) {
                    IEncodingDetector instance = plugin.Instance as IEncodingDetector;
                    if(instance != null) {
                        try {
                            instance.Open(null, null);
                            plgEncodingDetector = instance;
                            return true;
                        }
                        catch(Exception exception) {
                            HandlePluginException(exception, IntPtr.Zero, pi.Name, "Loading static plugin.");
                        }
                    }
                }
            }
            return false;
        }

        public void OnExplorerStateChanged(ExplorerWindowActions windowAction) {
            pluginServer.OnExplorerStateChanged(windowAction);
        }

        public void OnMenuRendererChanged() {
            pluginServer.OnMenuRendererChanged();
        }

        public void OnMouseEnter() {
            pluginServer.OnMouseEnter();
        }

        public void OnMouseLeave() {
            pluginServer.OnMouseLeave();
        }

        public void OnNavigationComplete(int index, byte[] idl, string path) {
            pluginServer.OnNavigationComplete(index, idl, path);
        }

        public void OnPointedTabChanged(int index, byte[] idl, string path) {
            pluginServer.OnPointedTabChanged(index, idl, path);
        }

        public void OnSelectionChanged(int index, byte[] idl, string path) {
            pluginServer.OnSelectionChanged(index, idl, path);
        }

        public void OnSettingsChanged(int iType) {
            pluginServer.OnSettingsChanged(iType);
        }

        public void OnTabAdded(int index, byte[] idl, string path) {
            pluginServer.OnTabAdded(index, idl, path);
        }

        public void OnTabChanged(int index, byte[] idl, string path) {
            pluginServer.OnTabChanged(index, idl, path);
        }

        public void OnTabRemoved(int index, byte[] idl, string path) {
            pluginServer.OnTabRemoved(index, idl, path);
        }

        public bool PluginInstantialized(string pluginID) {
            return dicPluginInstances.ContainsKey(pluginID);
        }

        public void RefreshPluginAssembly(PluginAssembly pa, bool fStatic) {
            foreach(PluginInformation information in pa.PluginInformations) {
                if(!information.Enabled) {
                    UnloadPluginInstance(information.PluginID, EndCode.Unloaded, fStatic);
                }
                else if(information.PluginType == PluginType.Background) {
                    Plugin plugin;
                    if(!TryGetPlugin(information.PluginID, out plugin)) {
                        plugin = Load(information, pa);
                    }
                    if(plugin != null) {
                        if(plgIFilter == null) {
                            plgIFilter = plugin.Instance as IFilter;
                        }
                        if(plgFilterCore == null) {
                            plgFilterCore = plugin.Instance as IFilterCore;
                        }
                    }
                    else {
                        information.Enabled = false;
                    }
                }
                else if(information.PluginType == PluginType.BackgroundMultiple) {
                    if(!PluginInstantialized(information.PluginID) && (Load(information, pa) == null)) {
                        information.Enabled = false;
                    }
                }
                else if((fStatic && (information.PluginType == PluginType.Static)) && !dicStaticPluginInstances.ContainsKey(information.PluginID)) {
                    LoadStatics(information, pa, false);
                }
            }
        }

        public void RegisterMenu(IPluginClient pluginClient, MenuType menuType, string menuText, bool fRegister) {
            foreach(Plugin plugin in dicPluginInstances.Values.Where(plugin => plugin.Instance == pluginClient)) {
                if(fRegister) {
                    if((menuType & MenuType.Bar) == MenuType.Bar) {
                        dicFullNamesMenuRegistered_Sys[plugin.PluginInformation.PluginID] = menuText;
                    }
                    if((menuType & MenuType.Tab) == MenuType.Tab) {
                        dicFullNamesMenuRegistered_Tab[plugin.PluginInformation.PluginID] = menuText;
                    }
                }
                else {
                    if((menuType & MenuType.Bar) == MenuType.Bar) {
                        dicFullNamesMenuRegistered_Sys.Remove(plugin.PluginInformation.PluginID);
                    }
                    if((menuType & MenuType.Tab) == MenuType.Tab) {
                        dicFullNamesMenuRegistered_Tab.Remove(plugin.PluginInformation.PluginID);
                    }
                }
                break;
            }
        }

        public static bool RemoveFromButtonBarOrder(string pluginID) {
            // TODO
            /*
            int index = lstPluginButtonsOrder.IndexOf(pluginID);
            if(index != -1) {
                lstPluginButtonsOrder.Remove(pluginID);
                int num2 = 0;
                int length = -1;
                for(int i = 0; i < QTButtonBar.ButtonIndexes.Length; i++) {
                    if((QTButtonBar.ButtonIndexes[i] == 0x10000) && (num2++ == index)) {
                        length = i;
                        break;
                    }
                }
                if(length != -1) {
                    if(QTButtonBar.ButtonIndexes.Length > 1) {
                        int[] destinationArray = new int[QTButtonBar.ButtonIndexes.Length - 1];
                        if(length != 0) {
                            Array.Copy(QTButtonBar.ButtonIndexes, destinationArray, length);
                        }
                        if(length != (QTButtonBar.ButtonIndexes.Length - 1)) {
                            Array.Copy(QTButtonBar.ButtonIndexes, length + 1, destinationArray, length, (QTButtonBar.ButtonIndexes.Length - length) - 1);
                        }
                        QTButtonBar.ButtonIndexes = destinationArray;
                    }
                    else {
                        QTButtonBar.ButtonIndexes = new int[0];
                    }
                    return true;
                }
            }
             */
            return false;
        }

        public static void SaveButtonOrder() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + @"Plugins")) {
                if(key != null) {
                    QTUtility2.WriteRegBinary(lstPluginButtonsOrder.ToArray(), "Buttons_Order", key);
                }
            }
        }

        public static void SavePluginAssemblies() {
            const string RegPath = RegConst.Root + @""; // TODO
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegPath + @"Plugins\Paths")) {
                foreach(string str in key.GetValueNames()) {
                    key.DeleteValue(str);
                }
                int idx = 0;
                foreach(PluginAssembly asm in PluginManager.PluginAssemblies) {
                    key.SetValue((idx++).ToString(), asm.Path);
                }
            }
        }

        public static void SavePluginShortcutKeys() {
            using(RegistryKey key = Registry.CurrentUser.CreateSubKey(RegConst.Root + @"Plugins")) {
                string[] keys = QTUtility.dicPluginShortcutKeys.Keys.ToArray();
                int[][] values = keys.Select(k => QTUtility.dicPluginShortcutKeys[k]).ToArray();
                QTUtility2.WriteRegBinary(keys, "ShortcutKeyIDs", key);
                QTUtility2.WriteRegBinary(values, "ShortcutKeyValues", key);
            }
        }

        public bool TryGetPlugin(string pluginID, out Plugin plugin) {
            return dicPluginInstances.TryGetValue(pluginID, out plugin);
        }

        public void UninstallPluginAssembly(PluginAssembly pa, bool fStatic) {
            foreach(PluginInformation information in pa.PluginInformations) {
                UnloadPluginInstance(information.PluginID, EndCode.Removed, fStatic);
                if(fStatic) {
                    Plugin plugin;
                    QTUtility.dicPluginShortcutKeys.Remove(information.PluginID);
                    if((information.PluginType == PluginType.Static) && dicStaticPluginInstances.TryGetValue(information.PluginID, out plugin)) {
                        plugin.Close(EndCode.Removed);
                        dicStaticPluginInstances.Remove(information.PluginID);
                    }
                }
            }
            if(fStatic) {
                dicPluginAssemblies.Remove(pa.Path);
                SavePluginShortcutKeys();
                pa.Uninstall();
                pa.Dispose();
            }
        }

        public void UnloadPluginInstance(string pluginID, EndCode code, bool fStatic) {
            Plugin plugin;
            if(fStatic) {
                RemoveFromButtonBarOrder(pluginID);
            }
            dicFullNamesMenuRegistered_Sys.Remove(pluginID);
            dicFullNamesMenuRegistered_Tab.Remove(pluginID);
            if(dicPluginInstances.TryGetValue(pluginID, out plugin)) {
                pluginServer.RemoveEvents(plugin.Instance);
                dicPluginInstances.Remove(pluginID);
                plugin.Close(code);
            }
        }

        public static List<PluginButton> ActivatedButtonsOrder {
            get {
                return lstPluginButtonsOrder;
            }
            set {
                lstPluginButtonsOrder = value;
            }
        }

        public static IEncodingDetector IEncodingDetector {
            get {
                return plgEncodingDetector;
            }
        }

        public IFilter IFilter {
            get {
                return plgIFilter;
            }
        }

        public IFilterCore IFilterCore {
            get {
                return plgFilterCore;
            }
        }

        public static List<PluginAssembly> PluginAssemblies {
            get {
                return new List<PluginAssembly>(dicPluginAssemblies.Values);
            }
        }

        public static IEnumerable<PluginInformation> PluginInformations {
            get {
                return dicPluginAssemblies.Values.SelectMany(pa => pa.PluginInformations);
            }
        }

        public IEnumerable<Plugin> Plugins {
            get {
                return new List<Plugin>(dicPluginInstances.Values);
            }
        }

        public bool SelectionChangeAttached {
            get {
                return pluginServer.SelectionChangedAttached;
            }
        }

        [Serializable]
        public class PluginButton {
            public string id { get; set; }
            public int index { get; set; }
        }
    }

    internal sealed class Plugin {
        private bool fBackgroundButtonIsEnabled;
        private bool fBackgroundButtonIsSupported;
        private IPluginClient pluginClient;
        private PluginInformation pluginInfo;

        public Plugin(IPluginClient pluginClient, PluginInformation pluginInfo) {
            this.pluginClient = pluginClient;
            this.pluginInfo = pluginInfo;
            fBackgroundButtonIsSupported = ((pluginInfo.PluginType == PluginType.Background) && ((pluginClient is IBarButton) || (pluginClient is IBarCustomItem))) || ((pluginInfo.PluginType == PluginType.BackgroundMultiple) && (pluginClient is IBarMultipleCustomItems));
        }

        public void Close(EndCode code) {
            if(pluginClient != null) {
                try {
                    pluginClient.Close(code);
                }
                catch(Exception exception) {
                    PluginManager.HandlePluginException(exception, IntPtr.Zero, pluginInfo.Name, "Closing plugin.");
                }
                pluginClient = null;
            }
            pluginInfo = null;
        }

        public bool BackgroundButtonEnabled {
            get {
                return (fBackgroundButtonIsSupported && fBackgroundButtonIsEnabled);
            }
            set {
                if(fBackgroundButtonIsSupported) {
                    fBackgroundButtonIsEnabled = value;
                }
            }
        }

        public bool BackgroundButtonSupported {
            get {
                return fBackgroundButtonIsSupported;
            }
        }

        public IPluginClient Instance {
            get {
                return pluginClient;
            }
        }

        public PluginInformation PluginInformation {
            get {
                return pluginInfo;
            }
        }
    }

    internal sealed class PluginInformation : IDisposable {
        public string Author;
        public string Description;
        public bool Enabled;
        public Image ImageLarge;
        public Image ImageSmall;
        public string Name;
        public string Path;
        public string PluginID;
        public PluginType PluginType;
        public string[] ShortcutKeyActions;
        public string TypeFullName;
        public string Version;

        public PluginInformation(PluginAttribute pluginAtt, string path, string pluginID, string typeFullName) {
            Author = pluginAtt.Author;
            Name = pluginAtt.Name;
            Version = pluginAtt.Version;
            Description = pluginAtt.Description;
            PluginType = pluginAtt.PluginType;
            Path = path;
            PluginID = pluginID;
            TypeFullName = typeFullName;
        }

        public void Dispose() {
            if(ImageLarge != null) {
                ImageLarge.Dispose();
                ImageLarge = null;
            }
            if(ImageSmall != null) {
                ImageSmall.Dispose();
                ImageSmall = null;
            }
        }
    }

    internal sealed class PluginAssembly : IDisposable {
        private Assembly assembly;
        public string Author;
        public string Description;
        private Dictionary<string, PluginInformation> dicPluginInformations = new Dictionary<string, PluginInformation>();
        public bool Enabled;
        private static string IMGLARGE = "_large";
        private static string IMGSMALL = "_small";
        public string Name;
        public string Path;
        private static string RESNAME = "Resource";
        private static Type T_PLUGINATTRIBUTE = typeof(PluginAttribute);
        public string Title;
        private static string TYPENAME_PLUGINCLIENT = typeof(IPluginClient).FullName;
        public string Version;

        public PluginAssembly(string path) {
            Path = path;
            Title = Author = Description = Version = Name = string.Empty;
            if(File.Exists(path)) {
                try {
                    assembly = Assembly.Load(File.ReadAllBytes(path));
                    AssemblyName name = assembly.GetName();
                    AssemblyTitleAttribute customAttribute = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute));
                    AssemblyCompanyAttribute attribute2 = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute));
                    AssemblyDescriptionAttribute attribute3 = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                    Version = name.Version.ToString();
                    if(customAttribute != null) {
                        Title = customAttribute.Title;
                    }
                    if(attribute2 != null) {
                        Author = attribute2.Company;
                    }
                    if(attribute3 != null) {
                        Description = attribute3.Description;
                    }
                    Name = Title + Version + "(" + path.GetHashCode().ToString("X") + ")";
                    foreach(Type type in assembly.GetTypes()) {
                        try {
                            if(ValidateType(type)) {
                                PluginAttribute pluginAtt = Attribute.GetCustomAttribute(type, T_PLUGINATTRIBUTE) as PluginAttribute;
                                if(pluginAtt != null) {
                                    string pluginID = Name + "+" + type.FullName;
                                    PluginInformation info = new PluginInformation(pluginAtt, path, pluginID, type.FullName);
                                    GetImageFromAssembly(assembly, type, info);
                                    dicPluginInformations[pluginID] = info;
                                }
                                else {
                                    QTUtility2.MakeErrorLog(null, "failed attribute");
                                }
                            }
                        }
                        catch {
                        }
                    }
                }
                catch(ReflectionTypeLoadException exception) {
                    QTUtility2.MakeErrorLog(exception, "Failed to load plugin assembly.\r\n"
                            + exception.LoaderExceptions.StringJoin("\r\n") + "\r\n" + path);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, "Failed to load plugin assembly.\r\n" + path);
                }
            }
        }

        public void Dispose() {
            assembly = null;
            foreach(PluginInformation information in dicPluginInformations.Values) {
                information.Dispose();
            }
            dicPluginInformations.Clear();
        }

        private static void GetImageFromAssembly(Assembly asm, Type type, PluginInformation info) {
            try {
                Type type2 = asm.GetType(type.Namespace + "." + RESNAME);
                if(type2 != null) {
                    PropertyInfo property = type2.GetProperty(type.Name + IMGLARGE, BindingFlags.NonPublic | BindingFlags.Static);
                    PropertyInfo info3 = type2.GetProperty(type.Name + IMGSMALL, BindingFlags.NonPublic | BindingFlags.Static);
                    if(property != null) {
                        info.ImageLarge = (Image)property.GetValue(null, null);
                    }
                    if(info3 != null) {
                        info.ImageSmall = (Image)info3.GetValue(null, null);
                    }
                }
            }
            catch {
            }
        }

        public Plugin Load(string pluginID) {
            if(File.Exists(Path)) {
                try {
                    PluginInformation information;
                    if(dicPluginInformations.TryGetValue(pluginID, out information)) {
                        IPluginClient pluginClient = assembly.CreateInstance(information.TypeFullName) as IPluginClient;
                        if(pluginClient != null) {
                            Plugin plugin = new Plugin(pluginClient, information);
                            IBarButton button = pluginClient as IBarButton;
                            if(button != null) {
                                Image imageLarge = information.ImageLarge;
                                Image imageSmall = information.ImageSmall;
                                try {
                                    Image image = button.GetImage(true);
                                    Image image4 = button.GetImage(false);
                                    if(image != null) {
                                        information.ImageLarge = image;
                                        if(imageLarge != null) {
                                            imageLarge.Dispose();
                                        }
                                    }
                                    if(image4 != null) {
                                        information.ImageSmall = image4;
                                        if(imageSmall != null) {
                                            imageSmall.Dispose();
                                        }
                                    }
                                }
                                catch(Exception exception) {
                                    PluginManager.HandlePluginException(exception, IntPtr.Zero, information.Name, "Getting image from pluging.");
                                    throw;
                                }
                            }
                            return plugin;
                        }
                    }
                }
                catch(Exception exception2) {
                    QTUtility2.MakeErrorLog(exception2, null);
                }
            }
            return null;
        }

        public bool TryGetPluginInformation(string pluginID, out PluginInformation info) {
            return dicPluginInformations.TryGetValue(pluginID, out info);
        }

        public void Uninstall() {
            try {
                foreach(Type type in assembly.GetTypes()) {
                    try {
                        if(ValidateType(type)) {
                            MethodInfo method = type.GetMethod("Uninstall", BindingFlags.Public | BindingFlags.Static);
                            if(method != null) {
                                method.Invoke(null, null);
                            }
                        }
                    }
                    catch {
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, "failed uninstall type");
            }
        }

        private static bool ValidateType(Type t) {
            return (((t.IsClass && t.IsPublic) && !t.IsAbstract) && (t.GetInterface(TYPENAME_PLUGINCLIENT) != null));
        }

        public List<PluginInformation> PluginInformations {
            get {
                return new List<PluginInformation>(dicPluginInformations.Values);
            }
        }

        public bool PluginInfosExist {
            get {
                return (dicPluginInformations.Count > 0);
            }
        }
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct PluginKey {
        public string PluginID;
        public int[] Keys;
        public PluginKey(string pluginID, int[] keys) {
            PluginID = pluginID;
            Keys = keys;
        }
    }
}
