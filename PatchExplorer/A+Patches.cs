using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatchExplorer
{
    public partial class AP_Patches : Form
    {
        public AP_Patches(string UnitNum)
        {
            InitializeComponent();
            UnitLabel.Text = UnitNum;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void A_Patches_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }

        private void A_Patches_Load(object sender, EventArgs e)
        {
            listBox1.DataSource = Form1.GetAPPtchesOnUnit(UnitLabel.Text);
        }
    }
}
