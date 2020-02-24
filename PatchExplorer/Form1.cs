using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PatchExplorer
{
    public partial class Form1 : Form
    {
        public static List<string> LiveBrandVersions = new List<string>();//Global so it can be used across the app...
        string PubPathName = "";
        int ToTalFileToUploadCount = 0;
        public Form1()
        {
            InitializeComponent();
        }
        private void StartPatchUpload(string TypeUPL,string ExeToUpload)
        {
            Process process = new Process();
            if (Convert.ToInt32(UnitTextBox.Text) > 100 && Convert.ToInt32(UnitTextBox.Text) < 30000) process.StartInfo.FileName = @"C:\path\putUser.exe";
            else if (Convert.ToInt32(UnitTextBox.Text) >= 30000) process.StartInfo.FileName = @"K:\Tools\NewPutUser\putuser.exe";
            if (TypeUPL == "ADV") process.StartInfo.Arguments = Convert.ToInt32(UnitTextBox.Text) + $" /pname{ConvertBrandToFixes(GetCurrentBrand(),2)}-pt /pversionv" + GetVersionFromEXE(ExeToUpload) + " " + ExeToUpload + ".exe";
            else if (TypeUPL == "PRO") process.StartInfo.Arguments = Convert.ToInt32(UnitTextBox.Text) + $" /pname{ConvertBrandToFixes(GetCurrentBrand(), 2)} /pversionv" + GetVersionFromEXE(ExeToUpload) + " " + ExeToUpload + ".exe";
            else if (TypeUPL == "DP") process.StartInfo.Arguments = Convert.ToInt32(UnitTextBox.Text) + " " + ExeToUpload + ".exe";
            MessageBox.Show(process.StartInfo.Arguments);//Debug
            process.StartInfo.WorkingDirectory = PubPathName;
            process.Start();
            process.WaitForExit();
            //if closed then assume successful
            DateTime DateNow = DateTime.Now;
            string writeString = $"Send By:{Environment.UserName} | Send Time:{DateNow.ToString(@"yyyy-MM-dd-h\:mm")} | Patch Name:{ExeToUpload} | Location:{PubPathName}"+ Environment.NewLine;
            using (StreamWriter file = new StreamWriter(@"W:\Technical_Services\CaseTimes\PatchExplorer.txt", true))
            {
                file.Write(writeString);
            }
            string patchText = $"\tUploadedBy\t: {Environment.UserName}{Environment.NewLine}\tUnit\t\t:{UnitTextBox.Text}{Environment.NewLine}\tDate\t\t: {DateNow.ToString(@"yyyy-MM-dd-h\:mm")}{Environment.NewLine}";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(PubPathName + @"\Readme.txt", true))
            {
                file.Write(patchText);
            }
        }

        private void LoadFixes(string SearchSTR = "")
        {
            List<string> logListContent = new List<string>();
            logListContent = Directory.GetDirectories(@"K:\Autologic\OneOffFixes\" + ConvertBrandToFixes(GetCurrentBrand())).ToList();
            logListContent = logListContent.OrderByDescending(x => x).ToList();
            MainlistView1.Items.Clear();
            int counter = 0;
            //
            for (int count = 0; count < logListContent.Count(); count++)//clear out the names
            {
                string[] words = logListContent[count].Split('\\');
                if (words[words.Count() - 1].ToLower().Contains(SearchSTR.ToLower()) == true)
                {
                    MainlistView1.Items.Add(words[words.Count() - 1]);
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


            MainlistView1.Items.Clear();
            int counter = 0;
            //
            for (int count = 0; count < logListContent.Count(); count++)//clear out the names
            {
                string[] words = logListContent[count].Split('\\');
                if (words[words.Count() - 1].ToLower().Contains(SearchSTR.ToLower()) == true)
                {
                    MainlistView1.Items.Add(words[words.Count() - 1]);
                    counter++;
                }
            }
            DisplayText.Text = "(" + counter + ")" + " Fixes Found";
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainlistView1.SelectedItems.Count > 0)
            {
                if (tabControl1.SelectedTab.Name == tabPage1.Name)//dealign with OneOFFixes
                {
                    GetPatchInfo();
                }
                else if (tabControl1.SelectedTab.Name == tabPage2.Name)//dealign with Builds
                {

                }
                
                
            }
        }
        private void GetPatchInfo()
        {
            listView2.Items.Clear();
            List<string> logListContent = new List<string>();
            PubPathName = @"K:\Autologic\OneOffFixes\" + ConvertBrandToFixes(GetCurrentBrand()) + @"\" + MainlistView1.SelectedItems[0].Text;//set the current patch folder
            logListContent = Directory.GetFiles(PubPathName, "*.exe").ToList();
            logListContent = logListContent.OrderByDescending(x => x).ToList();
            for (int count = 0; count < logListContent.Count(); count++)//clear out the names
            {
                string[] words = logListContent[count].Split('\\');
                if (words[words.Count() - 1].Contains("initdwnl.exe") | words[words.Count() - 1].Contains("download.exe")) continue;//ignore these
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
        private void RefreshChanges()
        {
            
            if (tabControl1.SelectedTab.Name == tabPage1.Name) LoadFixes(MainSearchTextBox.Text);//called when a Radiao button check is changed
            if (tabControl1.SelectedTab.Name == tabPage2.Name) LoadBuilds();
            if (tabControl1.SelectedTab.Name == tabPage3.Name)//Open Eazy patch
            {
                if(Properties.Settings.Default.EazyPatchUNLC == true)
                {
                    panel5.Enabled = true;
                }
                else { PasswordPanel.Visible = true; }
                
                tabControl1.Height = 260;
                splitContainer1.Enabled = false;
            }
            else{
                if (PasswordPanel.Visible == true) PasswordPanel.Visible = false;
                tabControl1.Height = 18;
                splitContainer1.Enabled = true;
            }
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
        private string GetVersionFromEXE(string Exe)
        {
            string[] doubleArray = System.Text.RegularExpressions.Regex.Split(Exe, @"[^0-9\.]+");
            if (doubleArray.Length < 3) { MessageBox.Show("No Version!"); return ""; }//Need to get version from Legion
            if (doubleArray[0]=="" | doubleArray[0] == "-1") doubleArray[0] = "3";

            return doubleArray[0] + "." + doubleArray[1] + "." + doubleArray[2].Replace(".", ""); ;
        }

        private string GetCurrentBrand()
        {
            if (radioButton1.Checked == true) return "landrover";
            if (radioButton2.Checked == true) return "jaguar";
            if (radioButton3.Checked == true) return "mercedes";
            if (radioButton4.Checked == true) return "bmw";
            if (radioButton5.Checked == true) return "vag";
            if (radioButton6.Checked == true) return "psa";
            if (radioButton7.Checked == true) return "ford";
            if (radioButton8.Checked == true) return "volvo";
            if (radioButton9.Checked == true) return "porsche";
            if (radioButton10.Checked == true) return "renault";
            return "";
        }

        private string ConvertBrandToFixes(string brand, int SecondType = 0)
        {
            //SecondType==0 For Normal OneOffFixes Folder
            //SecondType==1 For Builds Fodler Path name
            if (brand == "landrover" && SecondType == 0) return "LR";///LADNROVER
            if (brand == "landrover" && SecondType == 1) return "LandRover";
            if (brand == "landrover" && SecondType == 2) return "lr";
            if (brand == "jaguar" && SecondType == 0) return "JAG";///JAGUAR
            if (brand == "jaguar" && SecondType == 1) return "Jaguar";
            if (brand == "mercedes" & SecondType == 0) return "Mercedes";///MERCEDES
            if (brand == "mercedes" & SecondType == 1) return "Mercedes";
            if (brand == "mercedes" & SecondType == 2) return "Merc";
            if (brand == "bmw") return brand.ToUpper();
            if (brand == "vag") return brand.ToUpper();///VAG
            if (brand == "Renault") return "Renault";///Renault
            if (brand == "ford") return "Ford";///Ford
            if (brand == "volvo") return "Volvo";///Bolvo
            if (brand == "porsche") return "Porsche";///Porche
            if (brand == "porsche" & SecondType == 2) return "porsch";///Porche
            if (brand == "PSA") return "PSA";///Ford
            if (brand == "PSA" & SecondType == 2) return "PSA";//PSA
            return brand;
        }
        private string GetPatchState(string PatchName)
        {
            if (new string[] { "_PRO_DP_", "_GT_DP_", "_DP_ADV_", "_DP_PRO_", "DRIVEPRO_ADV" }.Any(s => PatchName.Contains(s))) return "DP";
            if (new string[] { "_A+_ADV","BLUEBOX_ADV", "ASSISTPLUS_ADV","_GT_","_GT."}.Any(s => PatchName.Contains(s))) return "ADV";
            if (new string[] { "_BB_PRO_" ,"A+_PRO" ,"_PLUS_" ,"_ProPlus_" ,"_PRO_","_PRO."}.Any(s => PatchName.Contains(s))) return "PRO";
            
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
            MainlistView1.View = View.LargeIcon;
        }

        private void tileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainlistView1.View = View.Tile;
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainlistView1.View = View.List;
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
            LoadFixes(MainSearchTextBox.Text);
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
                        //MessageBox.Show(file.Name);//For Debugging
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
            return SplitDec[0] + "."  + DecString.ToString("000");//formatting for last decimal as it cant be calculated correctly otherwise
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
            //BackroundLoadVersion.RunWorkerAsync();//Load Live Versions
        }

        private void BackroundLoadVersion_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            GetLiveVersions();
        }

        private async void button3_ClickAsync(object sender, EventArgs e)
        {
            DailyBuildsBrowser OpenForm = new DailyBuildsBrowser();
            OpenForm.Left = this.Bounds.Left + 877;
            OpenForm.Top = this.Bounds.Top + 106;
            OpenForm.Show();
        }
        /// <summary>
        /// /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////EAZYY-PATCH///////////////////////////////////////////////////////////
        /// /// /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CFGListBox.Enabled = EazyPatchCheckBx1.Checked;
            DLLListBox.Enabled = EazyPatchCheckBx2.Checked;
            EazyPatchRefresh.PerformClick();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (EazyPatchCheckBx1.Checked == true) GetCFGModifiedFiles();
            if (EazyPatchCheckBx2.Checked == true) GetDLLModifiedFiles();
            ToTalFileToUploadCount = 0;
            ProgressText.Text = "Total Files Selected For Uploading:" + ToTalFileToUploadCount;
            UploadStat.Visible = false;
            PubPathName = "";
            if (button1.Text == "UPLOAD PATCH TO UNIT")//Reset everything
            {
                EazyPatchBtn1.Text = "Create Patch For Selected Items";
                EazyProgBar.Value = 0;
            }
            EazyPatchRefresh.Text = "Refresh";
        }

        private void GetCFGModifiedFiles()
        {
            CFGListBox.Items.Clear();
            var files = Directory.GetFiles(@"X:\source\" + GetCurrentBrand() + @"\cfg\", "*.cfg", SearchOption.AllDirectories).Where(i => Directory.GetLastWriteTime(i) > DateTime.Today).ToArray();
            foreach (var item in files)
            {
                CFGListBox.Items.Add(item);
            }

        }
        private void GetDLLModifiedFiles()
        {
            DLLListBox.Items.Clear();
            var files = Directory.GetFiles(@"X:\source\" + GetCurrentBrand() + @"\dll\", "*.dll", SearchOption.AllDirectories).Where(i => Directory.GetLastWriteTime(i) > DateTime.Today).ToArray();
            foreach (var item in files) DLLListBox.Items.Add(item);
            var filesBAS = Directory.GetFiles(@"X:\source\" + GetCurrentBrand() + @"\dll\", "*.bas", SearchOption.AllDirectories).Where(i => Directory.GetLastWriteTime(i) > DateTime.Today).ToArray();
            foreach (var item in filesBAS) DLLListBox.Items.Add(item);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (EazyPatchBtn1.Text == "UPLOAD PATCH TO UNIT")
            {
                MainSearchTextBox.Text = EazyPatchName.Text;
                tabControl1.SelectedIndex =0;//Switching tabs will automatically search/update
               // RefreshChanges();
                if (MainlistView1.Items.Count > 0)//make sure it exists
                {
                    MainlistView1.Items[0].Selected = true;
                    MainlistView1.Select();
                }
            }
            else
            {
                string Path = @"K:\Autologic\OneOffFixes\" + ConvertBrandToFixes(GetCurrentBrand()) + @"\" + EazyPatchName.Text;
                EazyProgBar.Value = 0;
                if (Directory.Exists(Path) == true)
                {
                    DialogResult dialogResult = MessageBox.Show("Patch Folder Name Allready Exists" + Environment.NewLine + "Use Same Patch Folder, Everything Will Be Overwritten ?", "Name Allready Exists", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else if (dialogResult == DialogResult.Yes)//
                    {
                        Directory.Delete(Path, true);
                        EazyPatchBtn1.PerformClick();
                    }
                }
                else
                {
                    System.IO.Directory.CreateDirectory(Path);
                    SetUpTemplateForNewPatch(GetCurrentBrand(), Path);
                    //EDIT UP3 files to match
                    string[] files = Directory.GetFiles(Path + @"\", "*.UP3", SearchOption.TopDirectoryOnly);
                    EazyProgBar.Maximum = files.Length + 2;
                    foreach (string FullFilePath in files)//loop All .UP3 Files
                    {
                        string text = File.ReadAllText(FullFilePath);
                        string FInalString = "", CfgsToAdd = "", DLLsToAdd = "";
                        if (EazyPatchCheckBx1.Checked == true)//adding/updating CFG's
                        {
                            for (int i = 0; i < CFGListBox.Items.Count; i++)
                            {
                                string[] getnamefile = CFGListBox.Items[i].ToString().Split('\\');
                                if (CFGListBox.GetItemChecked(i) == true)
                                {
                                    CfgsToAdd += "72," + getnamefile[getnamefile.Length - 1] + @",C:\diagnos\" + ConvertBrandToFixes(GetCurrentBrand(), 2).ToLower() + @"\" + Environment.NewLine;
                                    //Add The files
                                    File.Copy(CFGListBox.Items[i].ToString(), Path + @"\" + getnamefile[getnamefile.Length - 1], true);
                                }
                            }

                            //text = text.Replace(@"72," + ConvertBrandToFixes(GetCurrentBrand(), 1).ToLower()+ @".cfg,C:\diagnos\"+ ConvertBrandToFixes(GetCurrentBrand(),1).ToLower() + @"\", CfgsToAdd);
                        }
                        if (EazyPatchCheckBx2.Checked == true)//adding/updating DLL's
                        {
                            for (int i = 0; i < DLLListBox.Items.Count; i++)
                            {
                                string[] getnamefile = DLLListBox.Items[i].ToString().Split('\\');
                                if (DLLListBox.GetItemChecked(i) == true)
                                {
                                    DLLsToAdd += "0," + getnamefile[getnamefile.Length - 1] + @",C:\diagnos\" + ConvertBrandToFixes(GetCurrentBrand(), 2).ToLower() + @"\" + Environment.NewLine;
                                    //Add The files
                                    File.Copy(DLLListBox.Items[i].ToString(), Path + @"\" + getnamefile[getnamefile.Length - 1], true);
                                }
                            }
                            //text = text.Replace(@"'0," + ConvertBrandToFixes(GetCurrentBrand(), 1).ToLower() + @".dll,C:\diagnos\" + ConvertBrandToFixes(GetCurrentBrand(), 1).ToLower() + @"\", DLLsToAdd);
                        }
                        string line;
                        System.IO.StreamReader file = new System.IO.StreamReader(FullFilePath);
                        while ((line = file.ReadLine()) != null)
                        {
                            if (line.Contains("72,") == true) { FInalString += line.Replace(line, CfgsToAdd); }
                            else if (line.Contains("0,") == true) { FInalString += line.Replace(line, DLLsToAdd); }
                            else FInalString += line + Environment.NewLine;
                        }
                        file.Close();
                        File.WriteAllText(FullFilePath, FInalString);//Write/Update UP3 File
                        //Start BUILD
                        if (ADVCheck.Checked == false & FullFilePath.Contains("A+_ADV") == true) continue;//skip this UP3 for BUILD
                        if (PROCheck.Checked == false & FullFilePath.Contains("BB_PRO") == true) continue;//skip this UP3 for BUILD
                        if (DPCheck.Checked == false & FullFilePath.Contains("DP_PRO") == true) continue;//skip this UP3 for BUILD
                        //RUN ALL BUILDS
                        Process process = new Process();
                        process.StartInfo.FileName = "mkupd3";
                        process.StartInfo.Arguments = FullFilePath;
                        process.StartInfo.WorkingDirectory = Path + @"\";
                        process.Start();
                        process.WaitForExit();//
                        EazyProgBar.Value++;
                    }
                    //check if exe was created
                    string[] ExeFiles = Directory.GetFiles(Path + @"\", "*.exe", SearchOption.TopDirectoryOnly);
                    bool ADVCreated = false, PROCreated = false, DPCreated = false;
                    foreach (string s in ExeFiles)
                    {
                        if (ADVCheck.Checked == true & s.Contains(GetCurrentBrand().ToUpper() + "_GT_3_") == true) ADVCreated = true;
                        if (PROCheck.Checked == true & s.Contains(GetCurrentBrand().ToUpper() + "_PRO_3_") == true) PROCreated = true;
                        if (DPCheck.Checked == true & s.Contains(GetCurrentBrand().ToUpper() + "_PRO_DP_3_") == true) DPCreated = true;
                    }
                    ProgressText.Text = "Advanced: " + GetCreatedStat(ADVCreated) + " || PRO: " + GetCreatedStat(PROCreated) + " || DP: " + GetCreatedStat(DPCreated);
                    //if (ADVCreated == true | PROCreated == true | DPCreated == true) { SetBuildTypeAccordingToUnit(); button1.Text = "UPLOAD PATCH TO UNIT"; PubPathName = Path + @"\"; }
                    //SetBuildTypeAccordingToUnit();
                    EazyPatchBtn1.Text = "UPLOAD PATCH TO UNIT";
                    EazyPatchRefresh.Text = "Finish";
                    PubPathName = Path + @"\";
                }
            }
        }
        private string GetCreatedStat(bool Value)
        {
            if (Value == true) return "Created";
            return "Not Created";
        }
        private void SetUpTemplateForNewPatch(string brand, string NewPatchPath)
        {
            EazyProgBar.Value = 1;
            //AutoGenerate UP3 Files
            string[] files = Directory.GetFiles(@"K:\Autologic\OneOffFixes\Template\" + ConvertBrandToFixes(brand, 1) + @"\", "*.UP3", SearchOption.TopDirectoryOnly);
            int TotalUP3s = 0;
            foreach (string s in files)
            {
                string[] JustFileName = s.Split('\\');
                File.Copy(s, NewPatchPath + @"\" + JustFileName[JustFileName.Length - 1], true);
                TotalUP3s++;
            }
            DateTime DateNow = DateTime.Now;
            string patchText = $"CreatedBy\t: {Environment.UserName}{Environment.NewLine}Date\t\t: {DateNow.ToString(@"yyyy-MM-dd-h\:mm")}{Environment.NewLine}Description\t: {EazyDescription.Text}{Environment.NewLine}" +
                $"Total UP3's\t: {TotalUP3s}{Environment.NewLine}-------------------{Environment.NewLine}"; 
            System.IO.File.WriteAllText(NewPatchPath + @"\Readme.txt", patchText);
            ProgressText.Text = "Total UP3's Created: " + TotalUP3s;
        }

        private void UnitTextBox_TextChanged(object sender, EventArgs e)
        {
            if (UnitTextBox.Text.Length == 0) { UnitTypeLabel.Visible = false; return; }
            else UnitTypeLabel.Visible = true;
            if (Convert.ToInt32(UnitTextBox.Text) >= 5000 & Convert.ToInt32(UnitTextBox.Text) < 20000) UnitTypeLabel.Text = "BB";
            else if (Convert.ToInt32(UnitTextBox.Text) >= 20000 & Convert.ToInt32(UnitTextBox.Text) < 30000) UnitTypeLabel.Text = "A+";
            else if (Convert.ToInt32(UnitTextBox.Text) >= 30000 & Convert.ToInt32(UnitTextBox.Text) < 40000) UnitTypeLabel.Text = "DP";
            else UnitTypeLabel.Text = "..";
        }

        private void UnitTypeLabel_TextChanged(object sender, EventArgs e)
        {
            if (UnitTypeLabel.Text == "BB" | UnitTypeLabel.Text == "A+" | UnitTypeLabel.Text == "DP") CheckExeCompatibility();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (textBox2.Text == "Yess")
            {
                Properties.Settings.Default.EazyPatchUNLC = true;
                Properties.Settings.Default.Save();
                PasswordPanel.Visible = false;
                panel5.Enabled = true;
                panel5.Visible = true;
            }
        }

        private void CFGListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked) ToTalFileToUploadCount++;
            else if (e.NewValue == CheckState.Unchecked) ToTalFileToUploadCount--;
            ProgressText.Text = "Total Files Selected For Uploading: " + ToTalFileToUploadCount;
            EazyProgBar.Maximum = ToTalFileToUploadCount + 3;
            //more checks
            if (e.NewValue == CheckState.Checked & DLLListBox.Items.Count > 0)
                if (DLLListBox.Items[e.Index].ToString().Substring(DLLListBox.Items[e.Index].ToString().Length - 5).Contains(".bas") == true)
                    MessageBox.Show("Selecting Bas File! will automatically compile the DLL and build it!" + Environment.NewLine +
                    Environment.NewLine + "If you do not want to re-compile the DLL then select the .dll to be uploaded");
        }
    }
}
