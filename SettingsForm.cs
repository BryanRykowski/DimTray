using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DimTray;

namespace DimTray
{
    public class CustomTrackBar : TrackBar
    {
        public int mIndex;
        public short savedVal;
    }

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

            Text = "DimTray Settings";
            MaximizeBox = false;
            MinimizeBox = false;

            monitorControls = new TableLayoutPanel();
            monitorControls.ColumnCount = 1;
            monitorControls.AutoSize = true;

            MonitorsTab = new TabPage("Monitors");

            FlowLayoutPanel container = new FlowLayoutPanel();
            container.FlowDirection = FlowDirection.TopDown;
            container.AutoSize = true;
            
            MonitorsTab.Controls.Add(container);
            
            Button refreshButton = new Button();
            refreshButton.Text = "Refresh Monitors";
            refreshButton.MouseClick += new MouseEventHandler(refresh_button);

            container.Controls.Add(refreshButton);
            container.Controls.Add(monitorControls);
            

            ProfilesTab = new TabPage("Profiles");
            OptionsTab = new TabPage("Options");

            TabControl1 = new TabControl();
            TabControl1.Controls.AddRange( new Control[]{MonitorsTab, ProfilesTab, OptionsTab });
            TabControl1.Dock = DockStyle.Fill;

            Controls.Add(TabControl1);

            Rectangle dimensions = this.RectangleToScreen(this.ClientRectangle);
            int titleHeight = dimensions.Top - this.Top;

            SuspendLayout();
            Size = new Size(600, 510);
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

        void slider_scroll(object sender, System.EventArgs e, short brightness, ref CustomTrackBar slider)
        {
            slider.savedVal = brightness;
        }

        void slider_mouseup(object sender, System.EventArgs e, int index, short brightness)
        {
            monitors.Monitors[index].SetBrightness(brightness);
        }

        void slider_keypress(object sender, KeyPressEventArgs e, int index, short brightness)
        {
            if ((e.KeyChar == (char)Keys.Enter) || (e.KeyChar == (char)Keys.Return))
            {
                monitors.Monitors[index].SetBrightness(brightness);
            }
        }

        void refresh_button(object sender, MouseEventArgs e)
        {
            refreshForm();
        }

        private void refreshForm()
        {
            foreach(Control control in monitorControls.Controls)
            {
                control.Dispose();
            }
            
            monitorControls.Controls.Clear();

            monitors.getDTmonitors();

            for (int i = 0; i < monitors.Monitors.Count; i++)
            {
                TableLayoutPanel controlPanel = new TableLayoutPanel();
                controlPanel.ColumnCount = 1;
                controlPanel.AutoSize = true;

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
                MonitorName.Text = monitors.Monitors[i].Name;
                MonitorName.AutoSize = true;

                Label MonitorRes = new Label();
                MonitorRes.Text = monitors.Monitors[i].Resolution;
                MonitorRes.AutoSize = true;

                monitorLabels.Controls.Add(MonitorNumber, 0, 1);
                monitorLabels.Controls.Add(MonitorName, 1, 1);
                monitorLabels.Controls.Add(MonitorRes, 2, 1);

                controlPanel.Controls.Add(monitorLabels);

                CustomTrackBar slider = new CustomTrackBar();
                slider.mIndex = i;
                slider.Minimum = monitors.Monitors[i].MinimumBrightness;
                slider.Maximum = monitors.Monitors[i].MaximumBrightness;
                slider.Value = monitors.Monitors[i].CurrentBrightness;
                slider.TickFrequency = 1;
                slider.Width = (int)(this.Width * 0.8);
                slider.Anchor = AnchorStyles.Left;
                slider.AutoSize = true;
                slider.Scroll += delegate (object sender, EventArgs e) { slider_scroll(sender, e, (short)slider.Value, ref slider); };
                slider.MouseUp += delegate (object sender, MouseEventArgs e) { slider_mouseup(sender, e, slider.mIndex, (short)slider.Value); };
                slider.KeyPress += delegate (object sender, KeyPressEventArgs e) { slider_keypress(sender, e, slider.mIndex, (short)slider.Value); };

                controlPanel.Controls.Add(slider);

                monitorControls.Controls.Add(controlPanel);
            }
        }
    }
}
