//    DimTray/SettingsForm.cs Copyright (C) 2021 Bryan Rykowski
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
        TabControl TabControl1;

        DTmonitors monitors = new DTmonitors();

        Profiles profiles = new Profiles();

        public SettingsForm()
        {
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
            
            Button refreshButton = new Button 
            { 
                Text = "Refresh Monitors"
            };
            
            refreshButton.MouseClick += new MouseEventHandler(refresh_button);

            container.Controls.Add(refreshButton);

            Button saveButton = new Button
            {
                Text = "Save As Profile..."
            };

            saveButton.MouseClick += new MouseEventHandler(save_button);

            container.Controls.Add(saveButton);

            container.Controls.Add(monitorControls);
            
            TabControl1 = new TabControl 
            {
                Dock = DockStyle.Fill
            };

            TabControl1.Controls.AddRange(new Control[] { MonitorsTab});

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

        void save_button(object sender, MouseEventArgs e)
        {
            //TODO: open window to get name
            profiles.SaveNewProfile(monitors, "name");
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

                slider.Scroll += delegate (object sender, EventArgs e) { slider_scroll(sender, e, (short)slider.Value, ref slider, ref sliderVal); };
                slider.MouseUp += delegate (object sender, MouseEventArgs e) { slider_mouseup(sender, e, slider.mIndex, (short)slider.Value); };
                slider.KeyPress += delegate (object sender, KeyPressEventArgs e) { slider_keypress(sender, e, slider.mIndex, (short)slider.Value); };


                sliderPanel.Controls.Add(slider);
                sliderPanel.Controls.Add(sliderVal);

                controlPanel.Controls.Add(sliderPanel);

                monitorControls.Controls.Add(controlPanel);
            }
        }
    }
}
