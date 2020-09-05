using System;
using System.Windows.Forms;
using System.IO;


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
                settingsForm.ShowDialog();

                trayIcon = new NotifyIcon
                {
                    Icon = Properties.Resources.TestIcon, //TODO: Make a real icon.
                    
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
