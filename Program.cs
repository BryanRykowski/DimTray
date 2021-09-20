//    DimTray/Program.cs Copyright (C) 2021 Bryan Rykowski
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;
using System.ComponentModel.Design;
using System.Threading;

namespace DimTray
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main()
        {
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DimTrayApplicationContext());

            return (0);
        }

        public class DimTrayApplicationContext : ApplicationContext
        {
            private NotifyIcon trayIcon;

            SettingsForm settingsForm = new SettingsForm();

            public DimTrayApplicationContext()
            {
                settingsForm.Hide();

                trayIcon = new NotifyIcon
                {
                    Icon = DimTray.Properties.Resources.dimtray_icon, //TODO: Make a real icon.
                    
                    Text = "DimTray"
                };


                ContextMenuStrip menu = new ContextMenuStrip();
                ToolStripLabel label = new ToolStripLabel("DimTray");

                menu.Items.Add(label);
                menu.Items.Add("-");
                menu.Items.Add("&Settings", null, Settings);
                menu.Items.Add("-");
                menu.Items.Add("E&xit", null, Exit);

                trayIcon.ContextMenuStrip = menu;

                trayIcon.Visible = true;
            }

            void Exit(object sender, EventArgs e)
            {
                trayIcon.Visible = false;
                Application.Exit();
            }

            void Settings(object sender, EventArgs e)
            {
                if (settingsForm.Visible == false)
                {
                    settingsForm.ShowDialog();
                }
            }
        }
    }
}
