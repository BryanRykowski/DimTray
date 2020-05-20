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
        TableLayoutPanel monitorControls;

        TabPage MonitorsTab;
        TabPage ProfilesTab;
        TabPage OptionsTab;
        TabControl TabControl1;

        DTmonitors monitors = new DTmonitors();

        public SettingsForm()
        {
            InitializeComponent();

            monitorControls = new TableLayoutPanel();
            monitorControls.ColumnCount = 1;
            monitorControls.AutoSize = true;

            MonitorsTab = new TabPage("Monitors");
            MonitorsTab.Controls.Add(monitorControls);

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

            int i = 0;

            foreach (var monitor in monitors.Monitors)
            {
                TableLayoutPanel monitorLabels = new TableLayoutPanel();
                monitorLabels.RowCount = 1;
                monitorLabels.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
                monitorLabels.AutoSize = true;

                Label MonitorNumber = new Label();
                MonitorNumber.Text = string.Format("Monitor {0}: ", i);
                MonitorNumber.Padding = new Padding(0);
                MonitorNumber.Margin = new Padding(0);
                MonitorNumber.AutoSize = true;

                Label MonitorName = new Label();
                MonitorName.Text = monitor.Name;
                MonitorName.AutoSize = true;

                Label MonitorRes = new Label();
                MonitorRes.Text = monitor.Resolution;
                MonitorRes.AutoSize = true;

                monitorLabels.Controls.Add(MonitorNumber, 0, 1);
                monitorLabels.Controls.Add(MonitorName, 1, 1);
                monitorLabels.Controls.Add(MonitorRes, 2, 1);

                monitorControls.Controls.Add(monitorLabels);

                ++i;
            }
        }
    }
}
