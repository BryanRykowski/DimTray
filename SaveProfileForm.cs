using DimTray;
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

namespace DimTrayFramework
{
    public partial class SaveProfileForm : Form
    {
        private string Path;
        private List<short> Brightness_Vals;

        public SaveProfileForm(string path, List<short> brightness_vals)
        {
            Path = path;
            Brightness_Vals = brightness_vals;

            InitializeComponent();

            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            Text = "Save Profile";
            MinimizeBox = false;
            MaximizeBox = false;

            TextBox inputBox = new TextBox 
            {
                Width = 400,
                Top = 15,
                Left = 10,
            };

            Panel textPanel = new Panel
            {
                Width = inputBox.Width + (inputBox.Left * 2),
                Height = inputBox.Height + (inputBox.Top * 2),
                Margin = new Padding(0)
            };

            textPanel.Controls.Add(inputBox);

            Button cancelButton = new Button 
            {
                Text = "Cancel",
                Margin = new Padding(0, 0, 10, 4)
            };

            cancelButton.MouseClick += new MouseEventHandler(cancel_button);

            Button saveButton = new Button 
            {
                Text = "Save",
                Margin = new Padding(0,0,5,4)
            };

            //saveButton.MouseClick += new MouseEventHandler(save_button);
            saveButton.MouseClick += delegate (object sender, MouseEventArgs e) 
            {
                save_button(sender, e, inputBox.Text, Brightness_Vals);
            };

            FlowLayoutPanel buttonStrip = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Width = textPanel.Width,
                Height = saveButton.Height,
                Margin = new Padding(0)
            };
            
            buttonStrip.Controls.AddRange(new Control[] { saveButton, cancelButton});

            FlowLayoutPanel outerContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Width = textPanel.Width,
                Height = textPanel.Height + buttonStrip.Height,
                Margin = new Padding(5,0,0,5)
            };

            outerContainer.Controls.AddRange(new Control[] { textPanel, buttonStrip });

            //SuspendLayout();
            //this.ClientSize = new Size(outerContainer.Width, outerContainer.Height + 20);
            //this.Size = new Size(outerContainer.Width, outerContainer.Height);
            //ResumeLayout();

            Controls.Add(outerContainer);
        }

        private void save_button(object sender, MouseEventArgs e, string text, List<short> vals)
        {
            if (text.Length > 0)
            {
                Profile profile = new Profile(Path + text + ".DTprofile", vals);
                profile.Serialize();
                this.Close();
            }
        }

        private void cancel_button(object sender, MouseEventArgs e)
        {
            this.Close();
        }
    }
}