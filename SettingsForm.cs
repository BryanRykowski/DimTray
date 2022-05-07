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

    public class CustomButton : Button
    {
        public int index;
    }

    public partial class SettingsForm : Form
    {
        TableLayoutPanel monitorControls;
        TableLayoutPanel profileTable;
        MonitorManager monitorManager = new MonitorManager();
        ProfileManager profileManager = new ProfileManager();

        ProfileNameForm profileNameForm;

        public SettingsForm()
        {
            profileNameForm = new ProfileNameForm(SaveFunc);
            profileNameForm.Hide();

            InitializeComponent();

            Text = "DimTray Settings";
            MaximizeBox = false;
            MinimizeBox = false;

            SuspendLayout();

            FlowLayoutPanel rootContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill
            };
            {
                FlowLayoutPanel buttonStrip = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    Anchor = (AnchorStyles.Left | AnchorStyles.Right),
                    AutoSize = true,
                    AutoScroll = true
                };
                {
                    Button refreshButton = new Button
                    {
                        Text = "Refresh Monitors",
                        AutoSize = true
                    };
                    {
                        refreshButton.MouseClick += new MouseEventHandler(refresh_button);
                    }
                    buttonStrip.Controls.Add(refreshButton);

                    Button saveButton = new Button
                    {
                        Text = "Save As Profile...",
                        AutoSize = true
                    };
                    {
                        saveButton.MouseClick += new MouseEventHandler(save_button);
                    }
                    buttonStrip.Controls.Add(saveButton);
                }
                rootContainer.Controls.Add(buttonStrip);

                TableLayoutPanel containerSplit = new TableLayoutPanel
                {
                    ColumnCount = 2,
                    RowCount = 1,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    AutoScroll = true,
                };
                {
                    FlowLayoutPanel monitorContainer = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.TopDown,
                        WrapContents = false,
                        AutoSize = true,
                        AutoScroll = true,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    {
                        monitorControls = new TableLayoutPanel 
                        {
                            ColumnCount = 1,
                            AutoSize = true
                        };
                        monitorContainer.Controls.Add(monitorControls);
                    }
                    containerSplit.Controls.Add(monitorContainer);

                    profileTable = new TableLayoutPanel
                    {
                        ColumnCount = 1,
                        AutoSize = true,
                        AutoScroll = true,
                        BorderStyle = BorderStyle.FixedSingle,
                    };
                    containerSplit.Controls.Add(profileTable);
                }
                rootContainer.Controls.Add(containerSplit);
            }
            Controls.Add(rootContainer);

            Size = new Size(850, 510);
            ResumeLayout();


            refreshMonitors();
            refreshProfiles();
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void slider_scroll(object sender, System.EventArgs e, short brightness, ref CustomTrackBar slider, ref TextBox sliderVal)
        {
            slider.savedVal = brightness;
            sliderVal.Text = brightness.ToString();
        }

        private void slider_mouseup(object sender, System.EventArgs e, int index, short brightness)
        {
            monitorManager.Monitors[index].SetBrightness(brightness);
        }

        private void slider_keypress(object sender, KeyPressEventArgs e, int index, short brightness)
        {
            if ((e.KeyChar == (char)Keys.Enter) || (e.KeyChar == (char)Keys.Return))
            {
                monitorManager.Monitors[index].SetBrightness(brightness);
            }
        }

        private void refresh_button(object sender, MouseEventArgs e)
        {
            refreshMonitors();
        }

        private void save_button(object sender, MouseEventArgs e)
        {
            profileNameForm.ClearAndDisplay();
        }

        private void SaveFunc(object sender, MouseEventArgs e, string profileName)
        {
            var vals = new List<short>();

            foreach (var monitor in monitorManager.Monitors)
            {
                vals.Add(monitor.CurrentBrightness);
            }

            profileManager.SaveNewProfile(vals, profileName);
            refreshProfiles();
        }

        private void apply_button(object sender, MouseEventArgs e, int profileIndex)
        {
            if (monitorManager.Monitors.Count == profileManager.profiles[profileIndex].data.brightnessVals.Count)
            {
                for (int i = 0; i < monitorManager.Monitors.Count; i++)
                {
                    bool support = monitorManager.Monitors[i].BrightnessSupported;
                    bool inRange = (profileManager.profiles[profileIndex].data.brightnessVals[i] <= monitorManager.Monitors[i].MaximumBrightness) && 
                                   (profileManager.profiles[profileIndex].data.brightnessVals[i] >= monitorManager.Monitors[i].MinimumBrightness);
                    
                    if (support && inRange)
                    {
                        monitorManager.Monitors[i].SetBrightness(profileManager.profiles[profileIndex].data.brightnessVals[i]);
                    }
                    else if (!inRange)
                    {
                        string mNumber = String.Format("{0}", i);
                        MessageBox.Show("Brightness value in profile for monitor " + mNumber + " out of allowed range!", "DimTray - Error");
                    }
                }

                refreshMonitors();
            }
            else
            {
                MessageBox.Show("Mismatch between number of monitors in profile (" + profileManager.profiles[profileIndex].data.brightnessVals.Count + ") and number currently connected (" + monitorManager.Monitors.Count + ")!", "DimTray - Error");
            }
        }

        private void refreshProfiles()
        {
            SuspendLayout();
            Size = new Size(850, 510);

            foreach (Control control in profileTable.Controls)
            {
                control.Dispose();
            }

            profileTable.Controls.Clear();

            profileManager.GetProfiles();

            for (int i = 0; i < profileManager.profiles.Count; i++)
            {
                FlowLayoutPanel profilePanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoSize = true,
                    BorderStyle = BorderStyle.FixedSingle,
                    Dock = DockStyle.Fill
                };
                {
                    FlowLayoutPanel textContainer = new FlowLayoutPanel
                    {
                        AutoSize = true
                    };
                    {
                        Label ProfileName = new Label
                        {
                            Text = string.Format("{0}", profileManager.profiles[i].name),
                            Padding = new Padding(4),
                            Margin = new Padding(4),
                            AutoSize = true
                        };
                        textContainer.Controls.Add(ProfileName);

                    }
                    profilePanel.Controls.Add(textContainer);

                    FlowLayoutPanel buttonContainer = new FlowLayoutPanel
                    {
                        AutoSize = true,
                        Dock = DockStyle.Right,
                        Anchor = AnchorStyles.Right,
                        BackColor = Color.Red
                    };
                    {
                        CustomButton applyButton = new CustomButton
                        {
                            Text = "Apply",
                            AutoSize = true,
                            index = i
                        };
                        {
                            applyButton.MouseClick += delegate (object sender, MouseEventArgs e) { apply_button(sender, e, applyButton.index); };
                        }
                        buttonContainer.Controls.Add(applyButton);
                    }
                    profilePanel.Controls.Add(buttonContainer);
                }
                profileTable.Controls.Add(profilePanel);
            }

            ResumeLayout();
        }

        private void refreshMonitors()
        {
            SuspendLayout();
            Size = new Size(850, 510);
            

            foreach (Control control in monitorControls.Controls)
            {
                control.Dispose();
            }
            
            monitorControls.Controls.Clear();

            monitorManager.getDTmonitors();

            for (int i = 0; i < monitorManager.Monitors.Count; i++)
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
                    Text = monitorManager.Monitors[i].Name,
                    AutoSize = true
                };

                Label MonitorRes = new Label 
                {
                    Text = monitorManager.Monitors[i].Resolution,
                    AutoSize = true
                };

                monitorLabels.Controls.Add(MonitorNumber, 0, 1);
                monitorLabels.Controls.Add(MonitorName, 1, 1);
                monitorLabels.Controls.Add(MonitorRes, 2, 1);

                controlPanel.Controls.Add(monitorLabels);

                FlowLayoutPanel sliderPanel = new FlowLayoutPanel 
                {
                    AutoSize = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Margin = new Padding(4, 0, 0, 0)
                };

                TextBox sliderVal = new TextBox 
                {
                    Text = monitorManager.Monitors[i].CurrentBrightness.ToString(),
                    ReadOnly = true,
                    Width = 48
                };

                CustomTrackBar slider = new CustomTrackBar 
                {
                    mIndex = i,
                    Minimum = monitorManager.Monitors[i].MinimumBrightness,
                    Maximum = monitorManager.Monitors[i].MaximumBrightness,
                    Value = monitorManager.Monitors[i].CurrentBrightness,
                    TickFrequency = 1,
                    Width = 320,
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

            ResumeLayout();
        }
    }
}
