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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace QTTabBarLib {

    static class RegFileWriter {
        // implements exporting of registry key as a reg file
        // to avoid UAC dialog caused by using Regedit.exe
        const string NEWLINE = "\r\n";
        const bool fNewLineForBinary = true;

        public static void Export(string keyName, string filePath) {
            using(RegistryKey rk = Registry.CurrentUser.OpenSubKey(keyName)) {
                StringBuilder sb = new StringBuilder("Windows Registry Editor Version 5.00" + NEWLINE + NEWLINE);

                buildSubkeyString(rk, sb);

                using(FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read)) {
                    // reg file is encoded by UTF16LE with BOM
                    using(StreamWriter sw = new StreamWriter(fs, new UnicodeEncoding(false, true))) {
                        sw.Write(sb.ToString());
                    }
                }
            }
        }


        private static void buildSubkeyString(RegistryKey rk, StringBuilder sb) {
            // exclude volatile keys
            // TODO: make this more general
            if(rk.Name == @"HKEY_CURRENT_USER\Software\QTTabBar\Cache") {
                return;
            }
            sb.Append(readValues(rk));

            foreach(string subKeyName in rk.GetSubKeyNames()) {
                using(RegistryKey rkSub = rk.OpenSubKey(subKeyName)) {
                    buildSubkeyString(rkSub, sb);
                }
            }
        }

        private static string readValues(RegistryKey rk) {
            string s = "";
            foreach(string valName in rk.GetValueNames()) {
                switch(rk.GetValueKind(valName)) {
                    case RegistryValueKind.Binary:
                        s += binaryToString(rk, valName);
                        break;

                    case RegistryValueKind.QWord:
                        s += qwordToString(rk, valName);
                        break;

                    case RegistryValueKind.DWord:
                        s += dwordToString(rk, valName);
                        break;

                    case RegistryValueKind.String:
                        s += szToString(rk, valName);
                        break;

                    case RegistryValueKind.ExpandString:
                        s += expandSzToString(rk, valName);
                        break;

                    case RegistryValueKind.MultiString:
                        s += multiSzToString(rk, valName);
                        break;
                }
            }
            return s.Length > 0
                    ? "[" + rk.Name + "]" + NEWLINE + s + NEWLINE
                    : "";
        }


        private static string binaryToString(RegistryKey rk, string valName) {
            return "\"" + sanitizeValName(valName) + "\"=hex:" + byteArrayToString((byte[])rk.GetValue(valName)) + NEWLINE;
        }

        private static string qwordToString(RegistryKey rk, string valName) {
            return "\"" + sanitizeValName(valName) + "\"=hex(b):" + byteArrayToString(BitConverter.GetBytes((long)rk.GetValue(valName))) + NEWLINE;
        }

        private static string dwordToString(RegistryKey rk, string valName) {
            return "\"" + sanitizeValName(valName) + "\"=dword:" + ((int)rk.GetValue(valName)).ToString("x8") + NEWLINE;
        }

        private static string szToString(RegistryKey rk, string valName) {
            return "\"" + sanitizeValName(valName) + "\"=\"" + ((string)rk.GetValue(valName)).Replace(@"\", @"\\") + "\"" + NEWLINE;
        }

        private static string expandSzToString(RegistryKey rk, string valName) {
            //REG_EXPAND_SZ
            return "\"" + sanitizeValName(valName) + "\"=hex(2):" + byteArrayToString(new UnicodeEncoding().GetBytes((string)rk.GetValue(valName) + "\0")) + NEWLINE;
        }

        private static string multiSzToString(RegistryKey rk, string valName) {
            //REG_MULTI_SZ
            string str = ((string[])rk.GetValue(valName)).StringJoin("\0") + "\0\0";
            return "\"" + sanitizeValName(valName) + "\"=hex(7):" + byteArrayToString(new UnicodeEncoding().GetBytes(str)) + NEWLINE;
        }

        private static string sanitizeValName(string str) {
            return str.Replace(@"\", @"\\");
        }

        private static string byteArrayToString(byte[] bytes) {
            StringBuilder sb = new StringBuilder();
            int c = 0, n = 20;
            for(int i = 0; i < bytes.Length; i++) {
                sb.Append(bytes[i].ToString("x2"));
                if(i == bytes.Length - 1) continue;
                sb.Append(",");

                if(fNewLineForBinary) {
                    c++;
                    if(c == n) {
                        sb.Append("\\" + NEWLINE + "  ");
                        c = 0;
                        n = 25;
                    }
                }
            }
            return sb.ToString();
        }
    }

    class SafePtr : IDisposable {
        private IntPtr ptr;

        public SafePtr(int size) {
            ptr = Marshal.AllocHGlobal(size);
        }

        public SafePtr(string str, bool unicode = true) {
            ptr = unicode ? Marshal.StringToHGlobalUni(str) : Marshal.StringToHGlobalAnsi(str);
        }

        public static implicit operator IntPtr(SafePtr safePtr) {
            return safePtr.ptr;
        }

        public void Dispose() {
            if(ptr != IntPtr.Zero) {
                Marshal.FreeHGlobal(ptr);
                ptr = IntPtr.Zero;
            }
        }
    }
}

