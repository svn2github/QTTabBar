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

namespace QTTabBarLib {
    internal sealed class MenuItemArguments {
        public MenuGenre Genre;
        public int Index;
        public bool IsBack;
        public string Path;
        public MenuTarget Target;
        public UserApp App;
        public ShellBrowserEx ShellBrowser;

        public MenuItemArguments(string path, MenuTarget target, MenuGenre genre) {
            Path = path;
            Genre = genre;
            Target = target;
        }

        public MenuItemArguments(string path, bool fback, int index, MenuGenre genre) {
            Path = path;
            IsBack = fback;
            Index = index;
            Genre = genre;
        }

        public MenuItemArguments(UserApp app, ShellBrowserEx shellBrowser, MenuGenre genre) {
            App = app;
            Path = app.Path;
            ShellBrowser = shellBrowser;
            Genre = genre;
        }
    }
}
