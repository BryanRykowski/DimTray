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
using System.IO;

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
        private readonly Size WindowSize = new Size( 758, 510);

        TableLayoutPanel monitorTable;
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
            AutoScaleMode = AutoScaleMode.None;

            SuspendLayout();

            Size = WindowSize;
            int clientWidth = ClientSize.Width;
            int clientHeight = ClientSize.Height;

            TableLayoutPanel monitorPanel = new TableLayoutPanel
            {
                Location = new Point(0, 0),
                Size = new Size(clientWidth / 2, clientHeight),
                ColumnCount = 1
            };
            Controls.Add(monitorPanel);
            {
                FlowLayoutPanel controlPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true
                };
                monitorPanel.Controls.Add(controlPanel);
                {
                    Button monitorRefreshButton = new Button
                    {
                        Text = "Refresh",
                        AutoSize = true,
                    };
                    controlPanel.Controls.Add(monitorRefreshButton);
                    {
                        monitorRefreshButton.MouseClick += new MouseEventHandler(refresh_monitors_button);
                    }

                    Button profileSaveButton = new Button
                    {
                        Text = "Save To Profile...",
                        AutoSize = true
                    };
                    controlPanel.Controls.Add(profileSaveButton);
                    {
                        profileSaveButton.MouseClick += new MouseEventHandler(save_button);
                    }
                }

                Panel monitorBorder = new Panel
                {
                    BorderStyle = BorderStyle.Fixed3D,
                    Dock = DockStyle.Fill,
                    AutoScroll = true
                };
                monitorPanel.Controls.Add(monitorBorder);
                {
                    monitorTable = new TableLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        AutoSize = true,
                        Margin = new Padding(0),
                        Padding = new Padding(3,3,3,4),
                    };
                    monitorBorder.Controls.Add(monitorTable);
                }
            }

            TableLayoutPanel profilePanel = new TableLayoutPanel
            {
                Location = new Point(clientWidth / 2, 0),
                Size = new Size(clientWidth / 2, clientHeight),
                ColumnCount = 1
            };
            Controls.Add(profilePanel);
            {
                FlowLayoutPanel controlPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true
                };
                profilePanel.Controls.Add(controlPanel);
                {
                    Button profileRefreshButton = new Button
                    {
                        Text = "Refresh",
                        AutoSize = true
                    };
                    controlPanel.Controls.Add(profileRefreshButton);
                    {
                        profileRefreshButton.MouseClick += new MouseEventHandler(refresh_profiles_button);
                    }
                }

                Panel profileBorder = new Panel
                {
                    BorderStyle = BorderStyle.Fixed3D,
                    Dock = DockStyle.Fill,
                    AutoScroll = true
                };
                profilePanel.Controls.Add(profileBorder);
                {
                    profileTable = new TableLayoutPanel
                    {
                        Dock = DockStyle.Top,
                        AutoSize = true,
                        Margin = new Padding(0),
                        Padding = new Padding(3, 3, 3, 3),
                        CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset
                    };
                    profileBorder.Controls.Add(profileTable);
                }

            }

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

        private void refresh_monitors_button(object sender, MouseEventArgs e)
        {
            refreshMonitors();
        }

        private void refresh_profiles_button(object sender, MouseEventArgs e)
        {
            refreshProfiles();
        }

        private void save_button(object sender, MouseEventArgs e)
        {
            profileNameForm.ClearAndDisplay();
        }

        private void Delete_Profile(object sender, MouseEventArgs e, int profileIndex)
        {
            try
            {
                File.Delete(profileManager.profiles[profileIndex].path);
            }
            catch
            {
                MessageBox.Show("Unable to delete profile \"" + profileManager.profiles[profileIndex].name + "\"", "DimTray - Error");
            }

            refreshProfiles();
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
            profileTable.SuspendLayout();

            foreach (Control control in profileTable.Controls)
            {
                control.Dispose();
            }

            profileTable.Controls.Clear();

            profileManager.GetProfiles();

            for (int i = 0; i < profileManager.profiles.Count; i++)
            {
                TableLayoutPanel profilePanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    Margin = new Padding(0),
                    Padding = new Padding(0, 0, 0, 3),
                    ColumnCount = 2, RowCount = 1,
                    AutoSize = true
                };
                profileTable.Controls.Add(profilePanel);
                {
                    Label ProfileName = new Label
                    {
                        Text = string.Format("{0}", profileManager.profiles[i].name),
                        Margin = new Padding(0),
                        Width = 160,
                        Anchor = AnchorStyles.None,
                        TextAlign = ContentAlignment.MiddleLeft,
                        AutoEllipsis = true
                    };
                    profilePanel.Controls.Add(ProfileName);

                    FlowLayoutPanel buttonPanel = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.RightToLeft,
                        Dock = DockStyle.Top,
                        AutoSize = true
                    };
                    profilePanel.Controls.Add(buttonPanel);
                    {
                        CustomButton deleteButton = new CustomButton
                        {
                            Text = "Delete",
                            AutoSize = true,
                            index = i,
                        };
                        buttonPanel.Controls.Add(deleteButton);
                        {
                            deleteButton.MouseClick += delegate (object sender, MouseEventArgs e) { Delete_Profile(sender, e, deleteButton.index); };
                        }

                        CustomButton applyButton = new CustomButton
                        {
                            Text = "Apply",
                            AutoSize = true,
                            index = i
                        };
                        buttonPanel.Controls.Add(applyButton);
                        {
                            applyButton.MouseClick += delegate (object sender, MouseEventArgs e) { apply_button(sender, e, applyButton.index); };
                        }
                    }
                }
            }

            profileTable.ResumeLayout();
        }

        private void refreshMonitors()
        {
            monitorTable.SuspendLayout();
            
            foreach (Control control in monitorTable.Controls)
            {
                control.Dispose();
            }

            monitorTable.Controls.Clear();

            monitorManager.getDTmonitors();

            for (int i = 0; i < monitorManager.Monitors.Count; i++)
            {
                TableLayoutPanel monitorControls = new TableLayoutPanel
                {
                    Parent = monitorTable,
                    Dock = DockStyle.Top,
                    Margin = new Padding(0, 0, 0, 3),
                    Padding = new Padding(0),
                    ColumnCount = 1,
                    RowCount = 2,
                    AutoSize = true,
                };
                monitorControls.Parent.Controls.Add(monitorControls);
                {
                    Label monitorLabel = new Label
                    {
                        Parent = monitorControls,
                        Text = string.Format("Monitor {0}: {1} - {2}", i, monitorManager.Monitors[i].Resolution, monitorManager.Monitors[i].Name),
                        Padding = new Padding(0, 0, 0, 3),
                        Margin = new Padding(0),
                        AutoSize = true,
                    };
                    monitorLabel.Parent.Controls.Add(monitorLabel);

                    Panel sliderTable = new Panel
                    {
                        Parent = monitorControls,
                        Dock = DockStyle.Top,
                        AutoSize = true,
                        Margin = new Padding(0, 0, 0, 3),
                        Padding = new Padding(0),
                    };
                    sliderTable.Parent.Controls.Add(sliderTable);
                    {
                        CustomTrackBar slider = new CustomTrackBar
                        {
                            Parent = sliderTable,
                            Dock = DockStyle.Top,
                            Margin = new Padding(0),
                            Padding = new Padding(-3, 0, -3, 0),
                            mIndex = i,
                            Minimum = monitorManager.Monitors[i].MinimumBrightness,
                            Maximum = monitorManager.Monitors[i].MaximumBrightness,
                            Value = monitorManager.Monitors[i].CurrentBrightness,
                            TickFrequency = 2,
                        };
                        slider.Parent.Controls.Add(slider);
                    
                        TextBox sliderVal = new TextBox
                        {
                            Parent = sliderTable,
                            Text = monitorManager.Monitors[i].CurrentBrightness.ToString(),
                            ReadOnly = true,
                            Width = 32,
                            Dock = DockStyle.Right,
                            Margin = new Padding(0)
                        };
                        sliderVal.Parent.Controls.Add(sliderVal);
                    
                        slider.Scroll += delegate (object sender, EventArgs e) { slider_scroll(sender, e, (short)slider.Value, ref slider, ref sliderVal); };
                        slider.MouseUp += delegate (object sender, MouseEventArgs e) { slider_mouseup(sender, e, slider.mIndex, (short)slider.Value); };
                        slider.KeyPress += delegate (object sender, KeyPressEventArgs e) { slider_keypress(sender, e, slider.mIndex, (short)slider.Value); };
                    }
                }


            }

            monitorTable.ResumeLayout();
        }
    }
}
