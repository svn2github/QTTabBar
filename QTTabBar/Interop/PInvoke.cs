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

namespace QTTabBarLib.Interop {
    using BandObjectLib;
    using QTTabBarLib;
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Text;
    using System.Windows.Forms;

    [SuppressUnmanagedCodeSecurity]
    internal static class PInvoke {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool AppendMenu(IntPtr hMenu, uint uFlags, IntPtr uIDNewItem, string lpNewItem);
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hwnd, ref Point lpPoint);
        [DllImport("user32.dll")]
        public static extern bool CloseClipboard();
        [DllImport("ole32.dll")]
        public static extern int CoCreateInstance([In] ref Guid rclsid, IntPtr pUnkOuter, uint dwClsContext, [In] ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr pv);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDIBSection(IntPtr hDC, ref QTTabBarLib.Interop.BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateFontIndirect(ref QTTabBarLib.Interop.LOGFONT lplf);
        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref QTTabBarLib.Interop.ICONINFO piconinfo);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("user32.dll")]
        public static extern int DestroyIcon(IntPtr hIcon);
        [DllImport("shell32.dll")]
        public static extern void DragAcceptFiles(IntPtr HWND, bool fAccept);
        [DllImport("shell32.dll")]
        public static extern void DragFinish(IntPtr hDrop);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern uint DragQueryFile(IntPtr hDrop, uint iFile, StringBuilder lpszFile, int cch);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int DrawTextExW(IntPtr hdc, IntPtr lpchText, int cchText, ref QTTabBarLib.Interop.RECT lprc, int dwDTFormat, IntPtr lpDTParams);
        [DllImport("UxTheme.dll", CharSet = CharSet.Unicode)]
        public static extern int DrawThemeTextEx(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, string text, int iCharCount, TextFormatFlags dwFlags, ref QTTabBarLib.Interop.RECT pRect, ref DTTOPTS pOptions);
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref QTTabBarLib.Interop.MARGINS pMarInset);
        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(out bool pfEnabled);
        [DllImport("user32.dll")]
        public static extern bool EmptyClipboard();
        [DllImport("user32.dll")]
        public static extern int EnumChildWindows(IntPtr hWndParent, EnumWndProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern uint EnumClipboardFormats(uint format);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardData(uint uFormat);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetCommandLine();
        [DllImport("gdi32.dll")]
        public static extern IntPtr GetCurrentObject(IntPtr hdc, int uObjectType);
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
        [DllImport("user32.dll")]
        public static extern IntPtr GetCursor();
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetDriveType(string lpRootPathName);
        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        public static extern bool GetLayeredWindowAttributes(IntPtr hwnd, out int pcrKey, out byte pbAlpha, out int pdwFlags);
        [DllImport("user32.dll")]
        public static extern uint GetMenuItemID(IntPtr hMenu, int nPos);
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetObject(IntPtr hgdiobj, int cbBuffer, out QTTabBarLib.Interop.LOGFONT lpvObject);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex) {
            if(IntPtr.Size == 8) {
                return GetWindowLongPtr64(hWnd, nIndex);
            }
            return new IntPtr(GetWindowLong(hWnd, nIndex));
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out QTTabBarLib.Interop.RECT pRect);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalAlloc(uint uFlags, IntPtr dwBytes);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        public static extern bool GlobalUnlock(IntPtr hMem);

        public static int HiWord(int dwValue) {
            return (dwValue >> 16) & 0xFFFF;
        }

        [DllImport("shell32.dll")]
        public static extern IntPtr ILClone(IntPtr pidl);
        [DllImport("shell32.dll")]
        public static extern IntPtr ILCombine(IntPtr pidl1_ABSOLUTE, IntPtr pidl2_RELATIVE);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr ILCreateFromPath(string pszPath);
        [DllImport("shell32.dll")]
        public static extern IntPtr ILFindLastID(IntPtr pidl);
        [DllImport("shell32.dll")]
        public static extern uint ILGetSize(IntPtr pidl);
        [DllImport("user32.dll")]
        public static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
        [DllImport("user32.dll")]
        public static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr hWnd);
        public static QTTabBarLib.Interop.RECT ListView_GetItemRect(IntPtr hwnd, int iItem, int iSubItem, int code) {
            QTTabBarLib.Interop.RECT structure = new QTTabBarLib.Interop.RECT();
            structure.left = code;
            structure.top = iSubItem;
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
            Marshal.StructureToPtr(structure, ptr, false);
            if(iSubItem == 0) {
                SendMessage(hwnd, 0x100e, (IntPtr)iItem, ptr);
            }
            else {
                SendMessage(hwnd, 0x1038, (IntPtr)iItem, ptr);
            }
            structure = (QTTabBarLib.Interop.RECT)Marshal.PtrToStructure(ptr, typeof(QTTabBarLib.Interop.RECT));
            Marshal.FreeHGlobal(ptr);
            return structure;
        }

        public static int ListView_HitTest(IntPtr hwnd, IntPtr lParam) {
            QTTabBarLib.Interop.LVHITTESTINFO structure = new QTTabBarLib.Interop.LVHITTESTINFO();
            structure.pt.x = QTUtility2.GET_X_LPARAM(lParam);
            structure.pt.y = QTUtility2.GET_Y_LPARAM(lParam);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
            Marshal.StructureToPtr(structure, ptr, false);
            int num = (int)SendMessage(hwnd, 0x1012, IntPtr.Zero, ptr);
            Marshal.FreeHGlobal(ptr);
            return num;
        }

        public static int LoWord(int dwValue) {
            return dwValue & 0xFFFF;
        }

        [DllImport("user32.dll")]
        public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, ref QTTabBarLib.Interop.RECT lpPoints, int cPoints);
        [DllImport("user32.dll")]
        public static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, ref Point lpPoints, int cPoints);
        [DllImport("user32.dll")]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool PtInRect(ref QTTabBarLib.Interop.RECT lprc, BandObjectLib.POINT pt);
        public static IntPtr Ptr_OP_AND(IntPtr ptr, uint ui) {
            if(IntPtr.Size == 8) {
                return (IntPtr)(((long)ptr) & ui);
            }
            return (IntPtr)(((int)ptr) & ui);
        }

        public static IntPtr Ptr_OP_OR(IntPtr ptr, uint ui) {
            if(IntPtr.Size == 8) {
                return (IntPtr)(((long)ptr) | ui);
            }
            return (IntPtr)(((uint)ptr) | ui);
        }

        [DllImport("user32.dll")]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);
        [DllImport("advapi32.dll")]
        public static extern int RegCloseKey(IntPtr hKey);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern uint RegisterClipboardFormat(string lpszFormat);
        [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
        public static extern int RegisterDragDrop(IntPtr hwnd, _IDropTarget pDropTarget);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint RegisterWindowMessage(string lpString);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int RegOpenKeyEx(IntPtr hKey, string lpSubKey, int ulOptions, uint samDesired, out IntPtr phkResult);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, IntPtr lpReserved, out int lpType, IntPtr lpData, ref int lpcbData);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("ole32.dll")]
        public static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);
        [DllImport("ole32.dll")]
        public static extern int RevokeDragDrop(IntPtr hwnd);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("gdi32.dll")]
        public static extern int SetBkMode(IntPtr hdc, int iBkMode);
        [DllImport("user32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);
        [DllImport("user32.dll")]
        public static extern bool SetMenuItemBitmaps(IntPtr hMenu, int uPosition, int uFlags, IntPtr hBitmapUnchecked, IntPtr hBitmapChecked);
        public static void SetRedraw(IntPtr hwnd, bool fRedraw) {
            SendMessage(hwnd, 11, fRedraw ? ((IntPtr)1) : IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("gdi32.dll")]
        public static extern int SetTextColor(IntPtr hdc, int crColor);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
            if(IntPtr.Size == 8) {
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            }
            return new IntPtr(SetWindowLong(hWnd, nIndex, (uint)((int)dwNewLong)));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, QTTabBarLib.Interop.HookProc lpfn, IntPtr hInstance, int dwThreadId);
        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hwnd, ref Point lpPoint);
        [DllImport("shell32.dll")]
        public static extern int SHBindToParent(IntPtr pidl, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellFolder ppv, out IntPtr ppidlLast);
        [DllImport("shell32.dll")]
        public static extern int SHCreateShellItem(IntPtr pidlParent, IShellFolder psfParent, IntPtr pidl, out QTTabBarLib.Interop.IShellItem ppsi);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);
        public static int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp) {
            if(IntPtr.Size == 4) {
                return SHFileOperation32(ref lpFileOp);
            }
            SHFILEOPSTRUCT64 shfileopstruct = new SHFILEOPSTRUCT64();
            shfileopstruct.hwnd = lpFileOp.hwnd;
            shfileopstruct.wFunc = lpFileOp.wFunc;
            shfileopstruct.pFrom = lpFileOp.pFrom;
            shfileopstruct.pTo = lpFileOp.pTo;
            shfileopstruct.fFlags = lpFileOp.fFlags;
            shfileopstruct.fAnyOperationsAborted = lpFileOp.fAnyOperationsAborted;
            shfileopstruct.hNameMappings = lpFileOp.hNameMappings;
            shfileopstruct.lpszProgressTitle = lpFileOp.lpszProgressTitle;
            int num = SHFileOperation64(ref shfileopstruct);
            lpFileOp.fAnyOperationsAborted = shfileopstruct.fAnyOperationsAborted;
            return num;
        }

        [DllImport("shell32.dll", EntryPoint = "SHFileOperation", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation32(ref SHFILEOPSTRUCT lpFileOp);
        [DllImport("shell32.dll", EntryPoint = "SHFileOperation", CharSet = CharSet.Unicode)]
        private static extern int SHFileOperation64(ref SHFILEOPSTRUCT64 lpFileOp);
        [DllImport("shell32.dll")]
        public static extern bool SHFreeShared(IntPtr hData, uint dwSourceProcId);
        [DllImport("shell32.dll")]
        public static extern int SHGetDesktopFolder(out IShellFolder ppshf);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SHGetFileInfo(IntPtr pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, int cbSizeFileInfo, int uFlags);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, int cbSizeFileInfo, int uFlags);
        [DllImport("shell32.dll")]
        public static extern int SHGetFolderLocation(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwReserved, out IntPtr ppidl);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder pszPath);
        [DllImport("shell32.dll")]
        public static extern int SHGetKnownFolderIDList(ref Guid rfid, int dwFlags, IntPtr hToken, out IntPtr ppidl);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHGetKnownFolderPath(ref Guid rfid, int dwFlags, IntPtr hToken, [MarshalAs(UnmanagedType.LPWStr)] out string ppszPath);
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);
        [DllImport("shell32.dll")]
        public static extern IntPtr SHLockShared(IntPtr hData, uint dwOtherProcId);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("shell32.dll")]
        public static extern bool SHUnlockShared(IntPtr lpvData);
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrRetToBuf(ref STRRET pstr, IntPtr pidl, StringBuilder pszBuf, int cchBuf);
        [DllImport("user32.dll")]
        public static extern bool TrackMouseEvent(ref QTTabBarLib.Interop.TRACKMOUSEEVENT lpEventTrack);
        [DllImport("user32.dll")]
        public static extern uint TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref BandObjectLib.MSG lpMsg);
        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        public static extern bool ValidateRect(IntPtr hWnd, IntPtr lpRect);
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point Point);
    }
}
