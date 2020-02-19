using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PatchExplorer
{
    public partial class Form1 : Form
    {
        public static List<string> LiveBrandVersions = new List<string>();
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
        private void LoadBuilds(string SearchSTR = "")
        {
            List<string> logListContent = new List<string>();
            logListContent = Directory.GetDirectories(@"W:\Build_Archive\Latest\" + ConvertBrandToFixes(GetCurrentBrand(), 1)).ToList();
            logListContent = logListContent.OrderByDescending(d => new FileInfo(d).CreationTime).ToList();


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
                if (tabControl1.SelectedTab.Name == "tabPage1")//dealign with OneOFFixes
                {
                    listView2.Items.Clear();
                    List<string> logListContent = new List<string>();
                    PubPathName = @"K:\Autologic\OneOffFixes\" + ConvertBrandToFixes(GetCurrentBrand()) + @"\" + listView1.SelectedItems[0].Text;//set the current patch folder
                    logListContent = Directory.GetFiles(PubPathName, "*.exe").ToList();
                    logListContent = logListContent.OrderByDescending(x => x).ToList();
                    for (int count = 0; count < logListContent.Count(); count++)//clear out the names
                    {
                        string[] words = logListContent[count].Split('\\');
                        if (words[words.Count() - 1].Contains("initdwnl.exe")| words[words.Count() - 1].Contains("download.exe")) continue;//ignore these
                        listView2.Items.Add(words[words.Count() - 1], 0);
                    }
                    CheckExeCompatibility();//check Patch exe compatebility with unit
                    if (File.Exists(PubPathName + @"\Readme.txt"))
                    {
                        string lines = System.IO.File.ReadAllText(PubPathName + @"\Readme.txt");
                        DescriptionTextBox.Text = lines;
                    }
                    else DescriptionTextBox.Text = "No Readme file foud!";
                    //show controls
                    pictureBox4.Visible = true;
                }
                else if (tabControl1.SelectedTab.Name == "tabPage2")//dealign with Builds
                {

                }
                
                
            }
        }

        private void RefreshChanges()
        {
            if (tabControl1.SelectedTab.Name == tabPage1.Name) LoadFixes();//called when a Radiao button check is changed
            else if (tabControl1.SelectedTab.Name == tabPage2.Name) LoadBuilds();
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshChanges();
        }
        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            RefreshChanges();
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
            //SecondType==1 For Builds Fodler Path name
            if (brand == "landrover" && SecondType == 0) return "LR";///LADNROVER
            if (brand == "landrover" && SecondType == 1) return "LandRover";
            if (brand == "jaguar" && SecondType == 0) return "JAG";///JAGUAR
            if (brand == "jaguar" && SecondType == 1) return "Jaguar";
            if (brand == "mercedes" & SecondType == 0) return "Mercedes";///MERCEDES
            if (brand == "mercedes" & SecondType == 1) return "Mercedes";
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
            if (new string[] { "_PRO_DP_", "_GT_DP_", "_DP_ADV_", "_DP_PRO_", "DRIVEPRO_ADV" }.Any(s => PatchName.Contains(s))) return "DP";
            if (new string[] { "_A+_ADV","BLUEBOX_ADV", "ASSISTPLUS_ADV","_GT_"}.Any(s => PatchName.Contains(s))) return "ADV";
            if (new string[] { "_BB_PRO_" ,"A+_PRO" ,"_PLUS_" ,"_ProPlus_" ,"_PRO_"}.Any(s => PatchName.Contains(s))) return "PRO";
            
            return "";
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(Control.MousePosition);
        }

        private void CheckExeCompatibility()
        {
            if (UnitTextBox.Text != "")//makesure theres something in the Txtbox
            {
                if(Convert.ToInt32(UnitTextBox.Text) >= 100)
                { 
                    for (int items = 0; items < listView2.Items.Count; items++)
                    {
                        if (listView2.Items[items].Text.Contains("_PRO_DP_") == true | listView2.Items[items].Text.Contains("_DP_ADV_") == true |
                            listView2.Items[items].Text.Contains("_GT_ADV_") == true | listView2.Items[items].Text.Contains("_DP_PRO_") == true |
                            listView2.Items[items].Text.Contains("_GT_DP_") == true)//For DRivePro Formats
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 30000 & Convert.ToInt32(UnitTextBox.Text) >= 30000) listView2.Items[items].ImageIndex = 2;//Index (2) will show green
                            else listView2.Items[items].ImageIndex = 1;//Index (1) will show Red
                        }
                        else if (listView2.Items[items].Text.Contains("_A+_") == true | listView2.Items[items].Text.Contains("_PLUS_") == true)//For AssistPLus Formats
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 20000 & Convert.ToInt32(UnitTextBox.Text) < 30000) listView2.Items[items].ImageIndex = 2;
                            else listView2.Items[items].ImageIndex = 1;
                        }
                        else if (listView2.Items[items].Text.Contains("_BB_") == true)//For BlueBox Formats
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 500 & Convert.ToInt32(UnitTextBox.Text) < 20000) listView2.Items[items].ImageIndex = 2;
                            else listView2.Items[items].ImageIndex = 1;
                        }
                        else if (listView2.Items[items].Text.Contains("_GT_") == true | listView2.Items[items].Text.Contains("_PRO_") == true |
                            listView2.Items[items].Text.Contains("_GT.") == true | listView2.Items[items].Text.Contains("_PRO.") == true)//For Advaced/GT Formats
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 500 & Convert.ToInt32(UnitTextBox.Text) < 30000) listView2.Items[items].ImageIndex = 0;
                            else listView2.Items[items].ImageIndex = 1;
                        }
                        else listView2.Items[items].ImageIndex = 0;//Index (0) will show default/white
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
            //CheckExeCompatibility();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
            var authMethod = new PrivateKeyAuthenticationMethod("autologic", new PrivateKeyFile("id_rsa_be"));
            var info = new ConnectionInfo("legion.autologic.com", "autologic", authMethod);
            //using (var client = new SshClient(info))
            //{
            //    client.Connect();
            //    
            //    textBox1.Text = client.IsConnected.ToString() + Environment.NewLine;
            //
            //    client.RunCommand("");
            //    client.Disconnect();
            //}
            using (var client = new Renci.SshNet.SftpClient(info))//SFTP
            {
                client.Connect();
                //textBox1.Text = client.IsConnected.ToString();
                var files = client.ListDirectory("/home/parsly/packages");//get all units with patches
                List<string> UnitPatches = new List<string>();
                foreach (var file in files)
                {
                    UnitPatches.Add(file.Name);
                }
                //GetLiveVersion
                files = client.ListDirectory("/home/parsly/packages/diag");//get Brand Patches
                List<string> LatestBrands = new List<string>();
                foreach (var file in files)
                {
                    LatestBrands.Add(file.Name);
                }
                client.Disconnect();

                //textBox1.Text = string.Join(Environment.NewLine, UnitPatches);
                //textBox3.Text = string.Join(Environment.NewLine, LatestBrands);
                //sort it
                foreach (string s in UnitPatches)
                {
                    
                }
                foreach (string s in LatestBrands)
                {
                    
                }
            }

            
        }
        public static void GetLiveVersions()//it will load all live version into the Global List
        {
            var authMethod = new PrivateKeyAuthenticationMethod("autologic", new PrivateKeyFile("id_rsa_be"));
            var info = new ConnectionInfo("legion.autologic.com", "autologic", authMethod);
            using (var client = new Renci.SshNet.SftpClient(info))//SFTP
            {
                if (client.IsConnected == false)
                {
                    client.Connect();
                    var files = client.ListDirectory("/home/parsly/packages/diag");//get Brand Patches
                    

                    decimal BMWDecimal = 0.0m,BMWDecimalPT = 0.0m, CIR = 0.0m;
                    decimal CIRpt = 0.0m , CIP =0.0m , FORDpt=0.0m, JAG = 0.0m, JAGpt = 0.0m;
                    decimal LR = 0.0m, LRpt = 0.0m, MERC=0.0m, MERCpt=0.0m, PORCH = 0.0m, PORCHpt = 0.0m;
                    decimal REN = 0.0m, RENpt = 0.0m,VAG=0.0m,VAGpt=0.0m, VOLVO = 0.0m, VOLVOpt = 0.0m ,PSA=0.0m , PSApt=0.0m;
                    decimal current =0.0m;
                    foreach (var file in files)
                    {
                        if (ExtractNumber(file.Name) == "" ) continue;
                        current = decimal.Parse(ExtractNumber(file.Name), NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);
                        //MessageBox.Show(file.Name);
                        if (file.Name.Contains("bmw-v") & current > BMWDecimal) BMWDecimal = current;
                        if (file.Name.Contains("bmw-pt-") & current > BMWDecimalPT) BMWDecimalPT = current;
                        if (file.Name.Contains("cir-v") & current > CIR) CIR = current;
                        if (file.Name.Contains("cir-pt") & current > CIRpt) CIRpt = current;
                        if (file.Name.Contains("cip-v") & current > CIP) CIP = current;
                        if (file.Name.Contains("ford-ptv") & current > FORDpt) FORDpt = current;
                        if (file.Name.Contains("jag-v") & current > JAG) JAG = current;
                        if (file.Name.Contains("jag-pt") & current > JAGpt) JAGpt = current;
                        if (file.Name.Contains("lr-v") & current > LR) LR = current;
                        if (file.Name.Contains("lr-pt") & current > LRpt) LRpt = current;
                        if (file.Name.Contains("merc-pt") & current > MERCpt) MERCpt = current;
                        if (file.Name.Contains("merc-v") & current > MERC) MERC = current;
                        if (file.Name.Contains("porsch-pt") & current > PORCHpt) PORCHpt = current;
                        if (file.Name.Contains("porsch-v") & current > PORCH) PORCH = current;
                        if (file.Name.Contains("renault-pt") & current > RENpt) RENpt = current;
                        if (file.Name.Contains("renault-v") & current > REN) REN = current;
                        if (file.Name.Contains("vag-pt") & current > VAGpt) VAGpt = current;
                        if (file.Name.Contains("vag-v") & current > VAG) VAG = current;
                        if (file.Name.Contains("volvo2-pt") & current > VOLVOpt) VOLVOpt = current;
                        if (file.Name.Contains("volvo2-v") & current > VOLVO) VOLVO = current;
                        if (file.Name.Contains("psa-pt-") & current > PSApt) PSApt = current;
                        if (file.Name.Contains("psa-v") & current > PSA) PSA = current;
                        
                    }
                    client.Disconnect();
                    LiveBrandVersions.Clear();
                    LiveBrandVersions.Add("BMW |3." + BMWDecimal + "|3." + BMWDecimalPT.ToString());
                    LiveBrandVersions.Add("CIR |3." + CIR.ToString() + "|3." + CIRpt.ToString());
                    LiveBrandVersions.Add("CIP |3." + CIP.ToString() +"|");
                    LiveBrandVersions.Add("FORD ||3." + FORDpt.ToString());
                    LiveBrandVersions.Add("JAG |3." + JAG.ToString() +"|3."+ JAGpt.ToString());
                    LiveBrandVersions.Add("LAND ROVER |3." + LR.ToString() + "|3." + LRpt.ToString());
                    LiveBrandVersions.Add("MERCEDES |3." + MERC.ToString() + "|3." + MERCpt.ToString());
                    LiveBrandVersions.Add("PORSCHE |3." + PORCH.ToString() + "|3." + PORCHpt.ToString());
                    LiveBrandVersions.Add("RENAULT |3." + REN.ToString() + "|3." + RENpt.ToString());
                    LiveBrandVersions.Add("VAG |3." + VAG.ToString() + "|3." + VAGpt.ToString());
                    LiveBrandVersions.Add("VOLVO |3." + VOLVO.ToString() + "|3." + VOLVOpt.ToString());
                    LiveBrandVersions.Add("PSA |3." + PSA.ToString() + "|3." + PSApt.ToString());
                    //BMWL = BMWL.OrderByDescending(i => i).ToList();
                    //VAGL = VAGL.OrderBy(x => x.Split('3')[0]).ThenBy(x => ExtractNumber(x.Split('3')[1])).ToList();
                    //list.OrderBy(x => x.Split('_')[0]).ThenBy(x => int.Parse(x.Split('_')[1]))
                    
                }
                client.Dispose();//to make sure...
            }
            return ;
        }
        public static string ExtractNumber(string original)
        {
            if (original.Length < 4) return "";
            string result = System.Text.RegularExpressions.Regex.Match(original, @"[0-9]+(\.[0-9]+)+(\.[0-9]+)?").Value;
            if (result == "") return "";
            string[] SplitDec = result.Substring(2).Split('.');
            if (SplitDec.Length < 2) return"";
            decimal DecString = decimal.Parse(SplitDec[1], NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);
            return SplitDec[0] + "."  + DecString.ToString("000");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SHHBrowse OpenForm = new SHHBrowse();
            OpenForm.Left = this.Bounds.Left + 877;
            OpenForm.Top = this.Bounds.Top + 106;
            OpenForm.Show();
            
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Process.Start(PubPathName);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(PubPathName) == true) Clipboard.SetText(PubPathName);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BackroundLoadVersion.RunWorkerAsync();
        }

        private void BackroundLoadVersion_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            GetLiveVersions();
        }
    }
}
