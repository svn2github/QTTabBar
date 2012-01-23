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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using QTPlugin;
using Size = System.Drawing.Size;

namespace QTTabBarLib {
    internal partial class Options11_ButtonBar : OptionsDialogTab {
        private ImageStrip imageStripLarge;
        private ImageStrip imageStripSmall;
        private ObservableCollection<ButtonEntry> ButtonPool;
        private ObservableCollection<ButtonEntry> CurrentButtons;

        public Options11_ButtonBar() {
            InitializeComponent();
        }

        public override void InitializeConfig() {
            // Initialize the button bar tab.
            imageStripLarge = new ImageStrip(new Size(24, 24));
            using(Bitmap b = Resources_Image.ButtonStrip24) {
                imageStripLarge.AddStrip(b);
            }
            imageStripSmall = new ImageStrip(new Size(16, 16));
            using(Bitmap b = Resources_Image.ButtonStrip16) {
                imageStripSmall.AddStrip(b);
            }
            ButtonPool = new ObservableCollection<ButtonEntry>();
            CurrentButtons = new ObservableCollection<ButtonEntry>();

            // Create a list of all the plugin buttons, and store the list 
            // index of the first button of each plugin in a dictionary keyed
            // on plugin ID.
            int pluginListPos = 0;
            var dicPluginListPos = new Dictionary<string, int>();
            var lstPluginButtons = new List<ButtonEntry>();
            foreach(PluginInformation pi in PluginManager.PluginInformations.OrderBy(pi => pi.Name)) {
                if(pi.PluginType == PluginType.Interactive) {
                    dicPluginListPos[pi.PluginID] = pluginListPos;
                    lstPluginButtons.Add(new ButtonEntry(this, pluginListPos++, pi, 0));
                }
                else if(pi.PluginType == PluginType.BackgroundMultiple) {
                    Plugin plugin;
                    if(pluginManager.TryGetPlugin(pi.PluginID, out plugin)) {
                        IBarMultipleCustomItems bmci = plugin.Instance as IBarMultipleCustomItems;
                        try {
                            if(bmci != null && bmci.Count > 0) {
                                // This is to maintain backwards compatibility.
                                bmci.Initialize(Enumerable.Range(0, bmci.Count).ToArray());
                                dicPluginListPos[pi.PluginID] = pluginListPos;
                                for(int i = 0; i < bmci.Count; i++) {
                                    lstPluginButtons.Add(new ButtonEntry(this, pluginListPos++, pi, i));
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            // Add the current buttons (right pane)
            int pluginIndex = 0;
            foreach(int i in WorkingConfig.bbar.ButtonIndexes) {
                if(i == QTButtonBar.BUTTONINDEX_PLUGIN) {
                    if(pluginIndex < PluginManager.ActivatedButtonsOrder.Count) {
                        var pluginButton = PluginManager.ActivatedButtonsOrder[pluginIndex];
                        if(dicPluginListPos.ContainsKey(pluginButton.id)) {
                            CurrentButtons.Add(lstPluginButtons[dicPluginListPos[pluginButton.id] + pluginButton.index]);
                        }
                    }
                    pluginIndex++;
                }
                else {
                    CurrentButtons.Add(new ButtonEntry(this, i));
                }
            }

            // Add the rest of the buttons to the button pool (left pane)
            ButtonPool.Add(new ButtonEntry(this, QTButtonBar.BII_SEPARATOR));
            for(int i = 1; i < QTButtonBar.INTERNAL_BUTTON_COUNT; i++) {
                if(!WorkingConfig.bbar.ButtonIndexes.Contains(i)) {
                    ButtonPool.Add(new ButtonEntry(this, i));
                }
            }
            foreach(ButtonEntry entry in lstPluginButtons) {
                if(!CurrentButtons.Contains(entry)) {
                    ButtonPool.Add(entry);
                }
            }
            lstButtonBarPool.ItemsSource = ButtonPool;
            lstButtonBarCurrent.ItemsSource = CurrentButtons;
        }

        public override void ResetConfig() {
            DataContext = WorkingConfig.bbar = new Config._BBar();
            InitializeConfig();
        }

        public override void CommitConfig() {
            var pluginButtons = new List<PluginManager.PluginButton>();
            for(int i = 0; i < CurrentButtons.Count; i++) {
                ButtonEntry entry = CurrentButtons[i];
                if(entry.Index >= QTButtonBar.BUTTONINDEX_PLUGIN) {
                    if(entry.PluginInfo.Enabled) {
                        pluginButtons.Add(new PluginManager.PluginButton {
                            id = entry.PluginInfo.PluginID,
                            index = entry.PluginButtonIndex
                        });
                    }
                    else {
                        CurrentButtons.RemoveAt(i--);
                    }
                }
            }
            PluginManager.ActivatedButtonsOrder = pluginButtons;
            PluginManager.SaveButtonOrder();
            WorkingConfig.bbar.ButtonIndexes = CurrentButtons.Select(
                    e => Math.Min(e.Index, QTButtonBar.BUTTONINDEX_PLUGIN)).ToArray();

            // TODO: Validate image strip
        }

        private void btnBBarAdd_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarPool.SelectedIndex;
            if(sel == -1) return;
            ButtonEntry entry = ButtonPool[sel];
            if(entry.Index == QTButtonBar.BII_SEPARATOR) {
                entry = new ButtonEntry(this, QTButtonBar.BII_SEPARATOR);
            }
            else {
                ButtonPool.RemoveAt(sel);
                if(sel == ButtonPool.Count) --sel;
                if(sel >= 0) {
                    lstButtonBarPool.SelectedIndex = sel;
                    lstButtonBarPool.ScrollIntoView(lstButtonBarPool.SelectedItem);
                }
            }
            if(lstButtonBarCurrent.SelectedIndex == -1) {
                CurrentButtons.Add(entry);
                lstButtonBarCurrent.SelectedIndex = CurrentButtons.Count - 1;
            }
            else {
                CurrentButtons.Insert(lstButtonBarCurrent.SelectedIndex + 1, entry);
                lstButtonBarCurrent.SelectedIndex++;
            }
            lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
        }

        private void btnBBarRemove_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarCurrent.SelectedIndex;
            if(sel == -1) return;
            ButtonEntry entry = CurrentButtons[sel];
            CurrentButtons.RemoveAt(sel);
            if(sel == CurrentButtons.Count) --sel;
            if(sel >= 0) {
                lstButtonBarCurrent.SelectedIndex = sel;
                lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
            }
            if(entry.Index != QTButtonBar.BII_SEPARATOR) {
                int i = 0;
                while(i < ButtonPool.Count && ButtonPool[i].Index < entry.Index) ++i;
                ButtonPool.Insert(i, entry);
                lstButtonBarPool.SelectedIndex = i;
            }
            else {
                lstButtonBarPool.SelectedIndex = 0;
            }
            lstButtonBarPool.ScrollIntoView(lstButtonBarPool.SelectedItem);
        }

        private void btnBBarUp_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarCurrent.SelectedIndex;
            if(sel <= 0) return;
            CurrentButtons.Move(sel, sel - 1);
            lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
        }

        private void btnBBarDown_Click(object sender, RoutedEventArgs e) {
            int sel = lstButtonBarCurrent.SelectedIndex;
            if(sel == -1 || sel == CurrentButtons.Count - 1) return;
            CurrentButtons.Move(sel, sel + 1);
            lstButtonBarCurrent.ScrollIntoView(lstButtonBarCurrent.SelectedItem);
        }

        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class ButtonEntry : INotifyPropertyChanged {
            public event PropertyChangedEventHandler PropertyChanged;
            private Options11_ButtonBar parent;

            public PluginInformation PluginInfo { get; private set; }
            public int Index { get; private set; }
            public bool IsPluginButton { get { return Index >= QTButtonBar.BUTTONINDEX_PLUGIN; } }
            public int PluginButtonIndex { get; private set; }
            public string PluginButtonText {
                get {
                    if(!IsPluginButton) return "";
                    if(PluginInfo.PluginType == PluginType.BackgroundMultiple && PluginButtonIndex != -1) {
                        Plugin plugin;
                        if(parent.pluginManager.TryGetPlugin(PluginInfo.PluginID, out plugin)) {
                            try {
                                return ((IBarMultipleCustomItems)plugin.Instance).GetName(PluginButtonIndex);
                            }
                            catch { }
                        }
                    }
                    return PluginInfo.Name;
                }
            }

            public Image LargeImage { get { return getImage(true); } }
            public Image SmallImage { get { return getImage(false); } }
            private Image getImage(bool large) {
                if(Index >= QTButtonBar.BUTTONINDEX_PLUGIN) {
                    if(PluginInfo.PluginType == PluginType.BackgroundMultiple && PluginButtonIndex != -1) {
                        Plugin plugin;
                        if(parent.pluginManager.TryGetPlugin(PluginInfo.PluginID, out plugin)) {
                            try {
                                return ((IBarMultipleCustomItems)plugin.Instance).GetImage(large, PluginButtonIndex);
                            }
                            catch { }
                        }
                    }
                    return large
                            ? PluginInfo.ImageLarge ?? Resources_Image.imgPlugin24
                            : PluginInfo.ImageSmall ?? Resources_Image.imgPlugin16;
                }
                else if(Index == 0 || Index >= QTButtonBar.BII_WINDOWOPACITY) {
                    return null;
                }
                else {
                    return large
                            ? parent.imageStripLarge[Index - 1]
                            : parent.imageStripSmall[Index - 1];
                }
            }
            public ButtonEntry(Options11_ButtonBar parent, int Index) {
                this.parent = parent;
                this.Index = Index;
            }
            public ButtonEntry(Options11_ButtonBar parent, int Index, PluginInformation PluginInfo, int PluginButtonIndex) {
                this.parent = parent;
                this.PluginInfo = PluginInfo;
                this.Index = QTButtonBar.BUTTONINDEX_PLUGIN + Index;
                this.PluginButtonIndex = PluginButtonIndex;
            }
        }

        #endregion

    }
}
