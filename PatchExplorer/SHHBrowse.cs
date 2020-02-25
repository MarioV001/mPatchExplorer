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
    public partial class SHHBrowse : Form
    {
        public SHHBrowse()
        {
            InitializeComponent();
        }

        private void panel2_Click(object sender, EventArgs e)
        {
           this.Close();
        }

        private void SHHBrowse_Load(object sender, EventArgs e)
        {
            ShowVersionInfo();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form1.GetLiveVersions();
            ShowVersionInfo();
        }
        private void ShowVersionInfo()
        {
            listView1.Items.Clear();
            foreach (string Name in Form1.LiveBrandVersions)
            {
                string[] SplitID = Name.Split('|');
                ListViewItem item1 = new ListViewItem(SplitID[0], 0);//Brand
                item1.SubItems.Add(SplitID[1]);//Version
                item1.SubItems.Add(SplitID[2]);//Version-PT
                listView1.Items.Add(item1);
            }
        }

        private void SHHBrowse_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
