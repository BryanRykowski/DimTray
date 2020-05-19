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
        DTmonitors monitors = new DTmonitors();

        public SettingsForm()
        {
            InitializeComponent();
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
