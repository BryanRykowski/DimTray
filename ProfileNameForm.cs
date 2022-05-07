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
    public partial class ProfileNameForm : Form
    {
        Action<object, MouseEventArgs, string> SaveFunc;
        public ProfileNameForm(Action<object,MouseEventArgs,string> saveFunc)
        {
            InitializeComponent();

            SaveFunc = saveFunc;

            saveButton.MouseClick += new MouseEventHandler(Save);
            cancelButton.MouseClick += new MouseEventHandler(Cancel);
        }

        public void ClearAndDisplay()
        {
            textBox.Text = "";
            this.ShowDialog();
        }

        private void Save(object sender, MouseEventArgs e)
        {
            SaveFunc(sender, e, textBox.Text);
            Hide();
        }

        private void Cancel(object sender, MouseEventArgs e)
        {
            Hide();
        }

        private void CloseForm(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}
