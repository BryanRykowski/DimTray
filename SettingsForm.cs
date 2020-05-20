using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DimTray
{
    public partial class SettingsForm : Form
    {
        TabPage MonitorsTab;
        TabPage ProfilesTab;
        TabPage OptionsTab;
        TabControl TabControl1;

        DTmonitors monitors = new DTmonitors();

        public SettingsForm()
        {
            InitializeComponent();

            MonitorsTab = new TabPage("Monitors");
            ProfilesTab = new TabPage("Profiles");
            OptionsTab = new TabPage("Options");

            TabControl1 = new TabControl();
            TabControl1.Controls.AddRange( new Control[]{MonitorsTab, ProfilesTab, OptionsTab });
            TabControl1.MinimumSize = new Size(640, 360);

            this.Controls.Add(TabControl1);

            Rectangle dimensions = this.RectangleToScreen(this.ClientRectangle);
            int titleHeight = dimensions.Top - this.Top;

            SuspendLayout();
            Size = new Size(TabControl1.Width + 16, titleHeight + TabControl1.Height + 8);
            ResumeLayout();

            refreshForm();
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void refreshForm()
        {
            monitors.getDTmonitors();
        }
    }
}
