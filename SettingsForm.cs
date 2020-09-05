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
using DimTrayFramework;

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
        public string ProfilePath;
        ProfileList profiles;

        public SettingsForm()
        {
            ProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DimTray\\Profiles\\";
            profiles = new ProfileList(ProfilePath);

            InitializeComponent();

            Text = "DimTray Settings";
            MaximizeBox = false;
            MinimizeBox = false;

            monitorControls = new TableLayoutPanel 
            {
                ColumnCount = 1,
                AutoSize = true
            };

            MonitorsTab = new TabPage("Monitors");

            FlowLayoutPanel container = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoScroll = true
            };
            
            MonitorsTab.Controls.Add(container);

            FlowLayoutPanel buttonStrip = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Dock = DockStyle.Left,
                AutoSize = true
            };
            
            Button refreshButton = new Button
            {
                Text = "Refresh Monitors",
                AutoSize = true
            };
            
            refreshButton.MouseClick += new MouseEventHandler(refresh_button);
            
            buttonStrip.Controls.Add(refreshButton);

            Button saveNewProfileButton = new Button
            {
                Text = "Save to New Profile",
                AutoSize = true
            };

            saveNewProfileButton.MouseClick += new MouseEventHandler(save_new_profile_button);

            buttonStrip.Controls.Add(saveNewProfileButton);

            container.Controls.Add(buttonStrip);

            container.Controls.Add(monitorControls);
            

            ProfilesTab = new TabPage("Profiles");
            OptionsTab = new TabPage("Options");

            TabControl1 = new TabControl 
            {
                Dock = DockStyle.Fill
            };

            TabControl1.Controls.AddRange(new Control[] { MonitorsTab, ProfilesTab, OptionsTab });

            Controls.Add(TabControl1);

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

        void slider_scroll(object sender, System.EventArgs e, short brightness, ref CustomTrackBar slider, ref TextBox sliderVal)
        {
            slider.savedVal = brightness;
            sliderVal.Text = brightness.ToString();
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
                TableLayoutPanel controlPanel = new TableLayoutPanel 
                {
                    ColumnCount = 1,
                    AutoSize = true
                };

                TableLayoutPanel monitorLabels = new TableLayoutPanel 
                {
                    RowCount = 1,
                    GrowStyle = TableLayoutPanelGrowStyle.AddColumns,
                    AutoSize = true
                };

                Label MonitorNumber = new Label 
                {
                    Text = string.Format("Monitor {0}: ", i),
                    Padding = new Padding(0),
                    Margin = new Padding(0),
                    AutoSize = true
                };

                Label MonitorName = new Label
                {
                    Text = monitors.Monitors[i].Name,
                    AutoSize = true
                };

                Label MonitorRes = new Label 
                {
                    Text = monitors.Monitors[i].Resolution,
                    AutoSize = true
                };

                monitorLabels.Controls.Add(MonitorNumber, 0, 1);
                monitorLabels.Controls.Add(MonitorName, 1, 1);
                monitorLabels.Controls.Add(MonitorRes, 2, 1);

                controlPanel.Controls.Add(monitorLabels);

                FlowLayoutPanel sliderPanel = new FlowLayoutPanel 
                {
                    AutoSize = true
                };

                TextBox sliderVal = new TextBox 
                {
                    Text = monitors.Monitors[i].CurrentBrightness.ToString(),
                    ReadOnly = true,
                    Width = 48
                };

                CustomTrackBar slider = new CustomTrackBar 
                {
                    mIndex = i,
                    Minimum = monitors.Monitors[i].MinimumBrightness,
                    Maximum = monitors.Monitors[i].MaximumBrightness,
                    Value = monitors.Monitors[i].CurrentBrightness,
                    TickFrequency = 1,
                    Width = (int)(this.Width * 0.8),
                    Anchor = AnchorStyles.Left,
                    AutoSize = true
                };

                slider.Scroll += delegate (object sender, EventArgs e)
                {
                    slider_scroll(sender, e, (short)slider.Value, ref slider, ref sliderVal);
                };
                
                slider.MouseUp += delegate (object sender, MouseEventArgs e)
                {
                    slider_mouseup(sender, e, slider.mIndex, (short)slider.Value); 
                };
                
                slider.KeyPress += delegate (object sender, KeyPressEventArgs e) 
                {
                    slider_keypress(sender, e, slider.mIndex, (short)slider.Value); 
                };


                sliderPanel.Controls.Add(slider);
                sliderPanel.Controls.Add(sliderVal);

                controlPanel.Controls.Add(sliderPanel);

                monitorControls.Controls.Add(controlPanel);
            }
        }

        private void save_new_profile_button(object sender, MouseEventArgs e)
        {
            List<short> brightness_vals = new List<short>();

            foreach (var monitor in monitors.Monitors)
            {
                brightness_vals.Add(monitor.CurrentBrightness);
            }

            using (SaveProfileForm saveProfileForm = new SaveProfileForm(ProfilePath, brightness_vals))
            {
                saveProfileForm.StartPosition = FormStartPosition.CenterParent;
                saveProfileForm.ShowDialog();
            }
        }
    }
}
