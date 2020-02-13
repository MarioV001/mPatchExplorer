using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatchExplorer
{
    public partial class Form1 : Form
    {
        string PubPathName = "";
        public Form1()
        {
            InitializeComponent();
        }
        private void StartPatchUpload(string TypeUPL,string ExeToUpload)
        {
            Process process = new Process();
            if (Convert.ToInt32(UnitTextBox.Text) > 100 && Convert.ToInt32(UnitTextBox.Text) < 30000) process.StartInfo.FileName = @"C:\path\putUser.exe";
            else if (Convert.ToInt32(UnitTextBox.Text) >= 30000) process.StartInfo.FileName = @"K:\Tools\NewPutUser\putuser.exe";
            if (TypeUPL == "ADV") process.StartInfo.Arguments = Convert.ToInt32(UnitTextBox.Text) + " /pnamebmw-pt /pversionv" + ExeToUpload.Replace("_", ".") + " " + ExeToUpload + ".exe";
            else if (TypeUPL == "PRO") process.StartInfo.Arguments = Convert.ToInt32(UnitTextBox.Text) + " /pnamebmw /pversionv" + ExeToUpload.Replace("_", ".") + " " + ExeToUpload + ".exe";
            else if (TypeUPL == "DP") process.StartInfo.Arguments = Convert.ToInt32(UnitTextBox.Text) + " " + ExeToUpload + ".exe";
            MessageBox.Show(process.StartInfo.Arguments);//Debug
            process.StartInfo.WorkingDirectory = PubPathName;
            process.Start();
            process.WaitForExit();
            //if closed then assume successful

        }

        private void LoadFixes(string SearchSTR = "")
        {
            List<string> logListContent = new List<string>();
            logListContent = Directory.GetDirectories(@"K:\Autologic\OneOffFixes\" + ConvertBrandToFixes(GetCurrentBrand())).ToList();
            logListContent = logListContent.OrderByDescending(x => x).ToList();
            listView1.Items.Clear();
            int counter = 0;
            //
            for (int count = 0; count < logListContent.Count(); count++)//clear out the names
            {
                string[] words = logListContent[count].Split('\\');
                if (words[words.Count() - 1].ToLower().Contains(SearchSTR.ToLower()) == true)
                {
                    listView1.Items.Add(words[words.Count() - 1]);
                    counter++;
                }
            }
            DisplayText.Text = "(" + counter + ")" + " Fixes Found";
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView2.Items.Clear();
                List<string> logListContent = new List<string>();
                PubPathName = @"K:\Autologic\OneOffFixes\" + ConvertBrandToFixes(GetCurrentBrand()) + @"\" + listView1.SelectedItems[0].Text;//set the current patch folder
                logListContent = Directory.GetFiles(PubPathName, "*.exe").ToList();
                logListContent = logListContent.OrderByDescending(x => x).ToList();
                for (int count = 0; count < logListContent.Count(); count++)//clear out the names
                {
                    string[] words = logListContent[count].Split('\\');
                    listView2.Items.Add(words[words.Count() - 1],0);
                }
                CheckExeCompatibility();//check Patch exe compatebility with unit
                if (File.Exists(PubPathName + @"\Readme.txt"))
                {
                    string lines = System.IO.File.ReadAllText(PubPathName + @"\Readme.txt");
                    DescriptionTextBox.Text = lines;
                }
                else DescriptionTextBox.Text = "No Readme file foud!";
                
            }
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            LoadFixes();//called when a Radiao button check is changed
        }

        private void pictureBox3_Click(object sender, EventArgs e)//Upload Buttons
        {
            if (Convert.ToInt32(UnitTextBox.Text) <= 500) { MessageBox.Show("Invalid Unit ID"); return;}
            if (listView2.SelectedItems.Count > 0)
            {
                for (int items = 0; items < listView2.SelectedItems.Count; items++)
                {
                    if (listView2.SelectedItems[items].ImageIndex == 1){ MessageBox.Show("Patch: '" + listView2.SelectedItems[items].Text.Replace(".exe", "") + "' Is not For this Unit!");return;}
                    StartPatchUpload(GetPatchState(listView2.SelectedItems[items].Text), listView2.SelectedItems[items].Text.Replace(".exe", ""));
                }
            }
        }

        private string GetCurrentBrand()
        {
            if (radioButton1.Checked == true) return "landrover";
            if (radioButton2.Checked == true) return "jaguar";
            if (radioButton3.Checked == true) return "mercedes";
            if (radioButton4.Checked == true) return "bmw";
            if (radioButton5.Checked == true) return "vag";
            if (radioButton6.Checked == true) return "renault";
            if (radioButton7.Checked == true) return "ford";
            if (radioButton8.Checked == true) return "volvo";
            if (radioButton9.Checked == true) return "porsche";
            return "";
        }

        private string ConvertBrandToFixes(string brand, int SecondType = 0)
        {
            //SecondType==0 For Normal OneOffFixes Folder
            //SecondType==1 For Templates Fodler Path name
            if (brand == "landrover" && SecondType == 0) return "LR";///LADNROVER
            if (brand == "landrover" && SecondType == 1) return "LandRover";
            if (brand == "jaguar" && SecondType == 0) return "JAG";///JAGUAR
            if (brand == "jaguar" && SecondType == 1) return "JAG";
            if (brand == "mercedes" & SecondType == 0) return "Mercedes";///MERCEDES
            if (brand == "mercedes" & SecondType == 1) return "merc";
            if (brand == "bmw") return brand.ToUpper();
            if (brand == "vag") return brand.ToUpper();///VAG
            if (brand == "Renault") return "Renault";///Renault
            if (brand == "ford") return "Ford";///Ford
            if (brand == "volvo") return "Volvo";///Bolvo
            if (brand == "porsche") return "Porsche";///Porche
            if (brand == "PSA") return "PSA";///Ford
            if (brand == "PSA" & SecondType == 2) return "PSA";//PSA
            return brand;
        }
        private string GetPatchState(string PatchName)
        {
            if (PatchName.Contains("_GT_3_")) return "ADV";
            if (PatchName.Contains("_PRO_3_")) return "PRO";
            if (PatchName.Contains("_PRO_DP_")) return "DP";
            return "";
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(Control.MousePosition);
        }

        private void CheckExeCompatibility()
        {
            if (UnitTextBox.Text != "")
            {
                if (Convert.ToInt32(UnitTextBox.Text) >= 100)
                {
                    for (int items = 0; items < listView2.Items.Count; items++)
                    {
                        if (listView2.Items[items].Text.Contains("_PRO_DP_") == true | listView2.Items[items].Text.Contains("_DP_ADV_") == true |
                            listView2.Items[items].Text.Contains("_GT_ADV_") == true | listView2.Items[items].Text.Contains("_DP_PRO_") == true |
                            listView2.Items[items].Text.Contains("_GT_DP_") == true)
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 30000 & Convert.ToInt32(UnitTextBox.Text) >= 30000) listView2.Items[items].ImageIndex = 2;
                            else listView2.Items[items].ImageIndex = 1;
                        }
                        else if (listView2.Items[items].Text.Contains("_A+_") == true)
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 20000 & Convert.ToInt32(UnitTextBox.Text) < 30000) listView2.Items[items].ImageIndex = 2;
                            else listView2.Items[items].ImageIndex = 1;
                        }
                        else if (listView2.Items[items].Text.Contains("_BB_") == true)
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 500 & Convert.ToInt32(UnitTextBox.Text) < 20000) listView2.Items[items].ImageIndex = 2;
                            else listView2.Items[items].ImageIndex = 1;
                        }
                        else if (listView2.Items[items].Text.Contains("_GT_") == true | listView2.Items[items].Text.Contains("_PRO_") == true |
                            listView2.Items[items].Text.Contains("_GT.") == true | listView2.Items[items].Text.Contains("_PRO.") == true)
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 500 & Convert.ToInt32(UnitTextBox.Text) < 30000) listView2.Items[items].ImageIndex = 0;
                            else listView2.Items[items].ImageIndex = 1;
                        }
                        else listView2.Items[items].ImageIndex = 0;
                    }
                }
            }
        }
        private void largeIconViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        private void tileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Tile;
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.List;
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            LoadFixes(textBox2.Text);
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchBTN.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        private void listView2_Click(object sender, EventArgs e)
        {

        }
        
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckExeCompatibility();
        }
    }
}
