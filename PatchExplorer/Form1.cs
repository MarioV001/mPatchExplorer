using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static List<string> LiveBrandVersions = new List<string>();//Global so it can be used across the app...
        string PubPathName = "";
        int ToTalFileToUploadCount = 0;
        public Form1()
        {
            InitializeComponent();
        }
        private void StartPatchUpload(string TypeUPL, string ExeToUpload, string SubDir = "")
        {
            Process process = new Process();
            if (Convert.ToInt32(UnitTextBox.Text) > 100 && Convert.ToInt32(UnitTextBox.Text) < 30000) process.StartInfo.FileName = @"C:\path\putUser.exe";
            else if (Convert.ToInt32(UnitTextBox.Text) >= 30000)
            {
                process.StartInfo.FileName = @"K:\Tools\NewPutUser\putuser.exe";
                TypeUPL = "DP";//overide Type For DriveProUnit's (doesnt matter)
            }
            if (TypeUPL == "ADV") process.StartInfo.Arguments = Convert.ToInt32(UnitTextBox.Text) + $" /pname{ConvertBrandToFixes(GetCurrentBrand(), 2)}-pt /pversionv" + GetVersionFromEXE(ExeToUpload) + " " + ExeToUpload + ".exe";
            else if (TypeUPL == "PRO") process.StartInfo.Arguments = Convert.ToInt32(UnitTextBox.Text) + $" /pname{ConvertBrandToFixes(GetCurrentBrand(), 2)} /pversionv" + GetVersionFromEXE(ExeToUpload) + " " + ExeToUpload + ".exe";
            else if (TypeUPL == "DP") process.StartInfo.Arguments = Convert.ToInt32(UnitTextBox.Text) + " " + ExeToUpload + ".exe";
            MessageBox.Show("ARGS: " + Environment.NewLine + process.StartInfo.Arguments);//Debug
            process.StartInfo.WorkingDirectory = PubPathName + SubDir;
            process.Start();
            process.WaitForExit();
            //if closed then assume successful
            DateTime DateNow = DateTime.Now;
            string writeString = $"Send By:{Environment.UserName} | Send Time:{DateNow.ToString(@"yyyy-MM-dd-h\:mm")} | Patch Name:{ExeToUpload} | Location:{PubPathName}" + Environment.NewLine;
            using (StreamWriter file = new StreamWriter(@"W:\Technical_Services\CaseTimes\PatchExplorer.txt", true))
            {
                file.Write(writeString);
            }
            string patchText = $"{Environment.NewLine}\tUploadedBy\t: {Environment.UserName}{Environment.NewLine}\tUnit\t\t:{UnitTextBox.Text}{Environment.NewLine}\tDate\t\t: {DateNow.ToString(@"yyyy-MM-dd-h\:mm")}";
            using (StreamWriter file = new StreamWriter(PubPathName + @"\Readme.txt", true))
            {
                file.Write(patchText);
            }
        }

        private void LoadFixes(string SearchSTR = "")
        {
            List<string> logListContent = new List<string>();
            logListContent = Directory.GetDirectories(@"K:\Autologic\OneOffFixes\" + ConvertBrandToFixes(GetCurrentBrand())).OrderByDescending(i => Directory.GetLastWriteTime(i)).ToList();

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
            logListContent = Directory.GetDirectories(@"W:\Build_Archive\Latest\" + ConvertBrandToFixes(GetCurrentBrand(), 1)).OrderByDescending(i => Directory.GetLastWriteTime(i)).ToList();
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
                PatcheslistView.Items.Clear();
                PatcheslistView.Groups.Clear();
                listView1.Items.Clear();
                CreateBytextBox.Text = "";
                DatetextBox.Text = "";
                DescriptionTextBox.Text = "";
                if (tabControl1.SelectedTab.Name == tabPage1.Name)//dealign with OneOFFixes
                {
                    GetPatchInfo();
                }
                else if (tabControl1.SelectedTab.Name == tabPage2.Name)//dealign with Builds
                {
                    GetBuildsInfo();
                }


            }
        }
        private void GetBuildsInfo()
        {
            PubPathName = @"W:\Build_Archive\Latest\" + ConvertBrandToFixes(GetCurrentBrand(), 1) + @"\" + MainlistView1.SelectedItems[0].Text;//set the current patch folder
            List<string> DirectoryList = new List<string>();
            DirectoryList = Directory.GetDirectories(PubPathName).ToList();
            for (int idx = 0; idx < DirectoryList.Count(); idx++)//go through all Directories
            {
                string[] BuildFolderType = DirectoryList[idx].Split('\\');
                List<string> EXEsList = new List<string>();
                PatcheslistView.Groups.Add(new ListViewGroup(BuildFolderType[BuildFolderType.Count() - 1], HorizontalAlignment.Left));

                EXEsList = Directory.GetFiles(PubPathName + '\\' + BuildFolderType[BuildFolderType.Count() - 1], "*.exe").ToList();
                for (int count = 0; count < EXEsList.Count(); count++)//Get All exes in that dir
                {
                    string[] words = EXEsList[count].Split('\\');
                    ListViewItem lvi = new ListViewItem(words[words.Count() - 1], 0);
                    PatcheslistView.Items.Add(lvi);
                    PatcheslistView.Groups[idx].Items.Add(lvi);
                }
            }
            CheckExeCompatibility();//check Patch exe compatebility with unit
        }

        private void GetPatchInfo()
        {

            List<string> logListContent = new List<string>();
            PubPathName = @"K:\Autologic\OneOffFixes\" + ConvertBrandToFixes(GetCurrentBrand()) + @"\" + MainlistView1.SelectedItems[0].Text;//set the current patch folder
            logListContent = Directory.GetFiles(PubPathName, "*.exe").ToList();
            logListContent = logListContent.OrderByDescending(x => x).ToList();
            for (int count = 0; count < logListContent.Count(); count++)//clear out the names
            {
                string[] words = logListContent[count].Split('\\');
                if (words[words.Count() - 1].Contains("initdwnl.exe") | words[words.Count() - 1].Contains("download.exe")) continue;//ignore these
                PatcheslistView.Items.Add(words[words.Count() - 1], 0);
            }
            CheckExeCompatibility();//check Patch exe compatebility with unit
            if (File.Exists(PubPathName + @"\Readme.txt"))
            {
                StreamReader file = new StreamReader(PubPathName + @"\Readme.txt");
                string[] Linestr = file.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                file.Close();
                for (int i = 0; i < Linestr.Length; i++)
                {
                    if (Linestr[i].ToLower().Contains("createdby")) CreateBytextBox.Text = Linestr[i].Split(':')[1];
                    else if (Linestr[i].ToLower().Contains("date") & DatetextBox.Text == "") DatetextBox.Text = Linestr[i].Split(':')[1];
                    else if (Linestr[i].ToLower().Contains("description") & DescriptionTextBox.Text == "" |
                        Linestr[i].ToLower().Contains("desc") & Linestr[i].Length > 30 & DescriptionTextBox.Text == "") DescriptionTextBox.Text = Linestr[i].Split(new char[] { ':' }, 2)[1];
                    else if (Linestr[i].ToLower().Contains("note") & i <= 6) DescriptionTextBox.Text += Environment.NewLine + Environment.NewLine + Linestr[i].Split(new char[] { ':' }, 2)[1];
                    else if (Linestr[i].ToLower().Contains("uploadedby"))///using PatchExplorerFormat
                    {
                        ListViewItem item1 = new ListViewItem(Linestr[i].Split(':')[1].Replace(".", " "), 0);//Brand
                        item1.SubItems.Add(Linestr[i + 1].Split(':')[1]);//Version
                        item1.SubItems.Add(Linestr[i + 2].Split(':')[1]);//Version-PT
                        listView1.Items.Add(item1);
                    }
                    else if (Linestr[i].ToLower().Contains("unitno"))///using old format
                    {
                        ListViewItem item1 = new ListViewItem("", 0);//Brand
                        item1.SubItems.Add(Linestr[i].Split(':')[1]);//Version
                        item1.SubItems.Add(Linestr[i + 1].Split(':')[1]);//Version-PT
                        listView1.Items.Add(item1);
                    }
                }

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
                if (Properties.Settings.Default.EazyPatchUNLC == true)
                {
                    panel5.Enabled = true;
                }
                else { PasswordPanel.Visible = true; }

                tabControl1.Height = 260;
                splitContainer1.Enabled = false;
            }
            else {
                if (PasswordPanel.Visible == true) PasswordPanel.Visible = false;
                tabControl1.Height = 25;
                splitContainer1.Enabled = true;
            }
            //image Update
            if (Properties.Settings.Default.EazyPatchUNLC == false) tabPage3.ImageIndex = 0;
            if (Properties.Settings.Default.EazyPatchUNLC == true) tabPage3.ImageIndex = 1;
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
            if (UnitTypeLabel.Visible == false | UnitTypeLabel.Text == "..") { MessageBox.Show("A Valid Unit ID is required!", "Invalid Unit ID"); return; }
            if (PatcheslistView.SelectedItems.Count > 0)
            {
                for (int items = 0; items < PatcheslistView.SelectedItems.Count; items++)
                {
                    if (PatcheslistView.SelectedItems[items].ImageIndex == 1) {
                        DialogResult result = MessageBox.Show("Patch: '" + PatcheslistView.SelectedItems[items].Text.Replace(".exe", "") + "' Is Not Valid For This Unit!" + Environment.NewLine +
                            Environment.NewLine + "Do you still wish to proceed? ", "Invalid patch for unit", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.No) return;//if presses then abort
                    }
                    if (tabControl1.SelectedTab.Name == tabPage1.Name)//if one of Fixes
                    {
                        StartPatchUpload(GetPatchState(PatcheslistView.SelectedItems[items].Text), PatcheslistView.SelectedItems[items].Text.Replace(".exe", ""));
                    }
                    else if (tabControl1.SelectedTab.Name == tabPage2.Name)//if builds
                    {
                        StartPatchUpload(GetPatchState(PatcheslistView.SelectedItems[items].Group.Header), PatcheslistView.SelectedItems[items].Text.Replace(".exe", ""), "\\" + PatcheslistView.SelectedItems[items].Group.Header);
                    }

                }
            } else MessageBox.Show("No patches have been selected!", "Select a patch");
        }
        private string GetVersionFromEXE(string Exe)
        {
            string[] doubleArray = System.Text.RegularExpressions.Regex.Split(Exe, @"[^0-9\.]+");
            //MessageBox.Show(doubleArray.Length.ToString());//debug
            if (doubleArray.Length < 3) { MessageBox.Show("No Version in Patch!"); return ""; }//Need to get version from Legion
            if (doubleArray[0] == "" | doubleArray[0] == "-1") doubleArray[0] = "3";
            if (doubleArray.Length == 3) return doubleArray[0] + "." + doubleArray[1] + "." + doubleArray[2].Replace(".", "");
            else if (doubleArray.Length == 4) return doubleArray[0] + "." + doubleArray[2] + "." + doubleArray[3].Replace(".", "");
            return "";
            
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
            if (new string[] { "_PRO_DP_", "_GT_DP_", "_DP_ADV_", "_DP_PRO_", "DRIVEPRO_ADV", "DRIVEPRO_PRO" }.Any(s => PatchName.Contains(s))) return "DP";
            if (new string[] { "_A+_ADV", "BLUEBOX_ADV", "ASSISTPLUS_ADV", "_GT_", "_GT.", "BLUEBOX_ADV" }.Any(s => PatchName.Contains(s))) return "ADV";
            if (new string[] { "_BB_PRO_", "A+_PRO", "_PLUS_", "_ProPlus_", "_PRO_", "_PRO.", "BLUEBOX_PRO" }.Any(s => PatchName.Contains(s))) return "PRO";

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
                if (Convert.ToInt32(UnitTextBox.Text) >= 100)
                {
                    for (int items = 0; items < PatcheslistView.Items.Count; items++)
                    {
                        if (PatcheslistView.Items[items].Text.Contains("_PRO_DP_") == true | PatcheslistView.Items[items].Text.Contains("_DP_ADV_") == true |
                            PatcheslistView.Items[items].Text.Contains("_GT_ADV_") == true | PatcheslistView.Items[items].Text.Contains("_DP_PRO_") == true |
                            PatcheslistView.Items[items].Text.Contains("_GT_DP_") == true)//For DRivePro Formats
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 30000 & Convert.ToInt32(UnitTextBox.Text) >= 30000) PatcheslistView.Items[items].ImageIndex = 2;//Index (2) will show green
                            else PatcheslistView.Items[items].ImageIndex = 1;//Index (1) will show Red
                        }
                        else if (PatcheslistView.Items[items].Text.Contains("_A+_") == true | PatcheslistView.Items[items].Text.Contains("_PLUS_") == true)//For AssistPLus Formats
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 20000 & Convert.ToInt32(UnitTextBox.Text) < 30000) PatcheslistView.Items[items].ImageIndex = 2;
                            else PatcheslistView.Items[items].ImageIndex = 1;
                        }
                        else if (PatcheslistView.Items[items].Text.Contains("_BB_") == true)//For BlueBox Formats
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 500 & Convert.ToInt32(UnitTextBox.Text) < 20000) PatcheslistView.Items[items].ImageIndex = 2;
                            else PatcheslistView.Items[items].ImageIndex = 1;
                        }
                        else if (PatcheslistView.Items[items].Text.Contains("_GT_") == true | PatcheslistView.Items[items].Text.Contains("_PRO_") == true |
                            PatcheslistView.Items[items].Text.Contains("_GT.") == true | PatcheslistView.Items[items].Text.Contains("_PRO.") == true)//For Advaced/GT Formats
                        {
                            if (Convert.ToInt32(UnitTextBox.Text) >= 500 & Convert.ToInt32(UnitTextBox.Text) < 30000) PatcheslistView.Items[items].ImageIndex = 0;
                            else PatcheslistView.Items[items].ImageIndex = 1;
                        }
                        else PatcheslistView.Items[items].ImageIndex = 0;//Index (0) will show default/white
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


        public static string GetLiveVersionsForBrand(string Brand, bool UpdateDB = false)//Get Single Brand Version From List
        {
            if (UpdateDB == true) GetLiveVersions();
            foreach (string Name in Form1.LiveBrandVersions)
            {
                string[] SplitID = Name.Split('|');
                if (SplitID[0].Contains(Brand)) return SplitID[1];
            }
            return "";
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
                    decimal BMWDecimal = 0.0m, BMWDecimalPT = 0.0m, CIR = 0.0m;
                    decimal CIRpt = 0.0m, CIP = 0.0m, FORDpt = 0.0m, JAG = 0.0m, JAGpt = 0.0m;
                    decimal LR = 0.0m, LRpt = 0.0m, MERC = 0.0m, MERCpt = 0.0m, PORCH = 0.0m, PORCHpt = 0.0m;
                    decimal REN = 0.0m, RENpt = 0.0m, VAG = 0.0m, VAGpt = 0.0m, VOLVO = 0.0m, VOLVOpt = 0.0m, PSA = 0.0m, PSApt = 0.0m;
                    decimal current = 0.0m;
                    foreach (var file in files)
                    {
                        if (ExtractNumber(file.Name) == "") continue;
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
                    LiveBrandVersions.Add("CIP |3." + CIP.ToString() + "|");
                    LiveBrandVersions.Add("FORD ||3." + FORDpt.ToString());
                    LiveBrandVersions.Add("JAG |3." + JAG.ToString() + "|3." + JAGpt.ToString());
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
            return;
        }
        public static string ExtractNumber(string original)
        {
            if (original.Length < 4) return "";
            string result = System.Text.RegularExpressions.Regex.Match(original, @"[0-9]+(\.[0-9]+)+(\.[0-9]+)?").Value;
            if (result == "") return "";
            string[] SplitDec = result.Substring(2).Split('.');
            if (SplitDec.Length < 2) return "";
            decimal DecString = decimal.Parse(SplitDec[1], NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);
            return SplitDec[0] + "." + DecString.ToString("000");//formatting for last decimal as it cant be calculated correctly otherwise
        }

        public static List<string> GetAPPtchesOnUnit(string UnitNum)
        {
            var authMethod = new PrivateKeyAuthenticationMethod("autologic", new PrivateKeyFile("id_rsa_be"));
            var info = new ConnectionInfo("legion.autologic.com", "autologic", authMethod);
            List<string> addresses = new List<string>();
            using (var client = new Renci.SshNet.SftpClient(info))//SFTP
            {
                if (client.IsConnected == false)
                {
                    client.Connect();
                    if (client.Exists("/home/parsly/packages/A0" + UnitNum) == true)
                    {
                        var files = client.ListDirectory("/home/parsly/packages/A0" + UnitNum);//get Brand Patches
                        foreach (var file in files)
                        {
                            if (file.Name.Contains(".db") | file.Name == ".." | file.Name == ".") continue;
                            addresses.Add(file.Name.Replace(".tar.gz", ""));//add all patches to dataset
                        }
                    }
                    else addresses.Add("No Patches Found For Unit");
                    client.Disconnect();

                }
            }
            return addresses;
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

            PatcheslistView.Items.Clear();
            if (Properties.Settings.Default.EazyPatchUNLC == false) tabPage3.ImageIndex = 0;
            if (Properties.Settings.Default.EazyPatchUNLC == true) tabPage3.ImageIndex = 1;
        }

        private void BackroundLoadVersion_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            GetLiveVersions();
        }

        private void button3_ClickAsync(object sender, EventArgs e)
        {
            DailyBuildsBrowser OpenForm = new DailyBuildsBrowser();
            OpenForm.Left = this.Bounds.Left + 460;
            OpenForm.Top = this.Bounds.Top + 106;
            OpenForm.Show();
        }
        /////BLUE BOX DATA FETCH FROM WEBSITE
        private void button1_Click_1(object sender, EventArgs e)//blue box
        {
            GetBBVersionsAsync();
        }
        public async void GetBBVersionsAsync()
        {
            //"W:\Brand\Tools\software_versions"
        }
        void SaveVersions(string input)
        {

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

        private void button4_Click(object sender, EventArgs e)//Eazypatch-Reset
        {
            if (EazyPatchCheckBx1.Checked == true) GetCFGModifiedFiles();
            if (EazyPatchCheckBx2.Checked == true) GetDLLModifiedFiles();
            ToTalFileToUploadCount = 0;
            ProgressText.Text = "Total Files Selected For Uploading:" + ToTalFileToUploadCount;
            UploadStat.Visible = false;
            PubPathName = "";
            EazyPatchBtn1.Text = "Create Patch For Selected Items";
            EazyProgBar.Value = 0;
            EazyDescription.Enabled = true;
            EazyPatchName.Enabled = true;
        }
        private void BackroundCFGWorker(object sender, DoWorkEventArgs e)
        {
            var files = Directory.GetFiles(@"X:\source\" + GetCurrentBrand() + @"\cfg\", "*.cfg", SearchOption.AllDirectories).Where(i => Directory.GetLastWriteTime(i) > DateTime.Today).ToArray();
            foreach (var item in files) CFGListBox.Invoke(new Action(() => CFGListBox.Items.Add(item)));
        }
        void work_RunCFGWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadingCFGS.Visible = false;
            EazyPatchCheckBx1.Enabled = true;
        }
        private void GetCFGModifiedFiles()
        {
            LoadingCFGS.Visible = true;
            EazyPatchCheckBx1.Enabled = false;
            CFGListBox.Items.Clear();
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(BackroundCFGWorker);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(work_RunCFGWorkerCompleted);
            worker.RunWorkerAsync();
        }
        private void BackroundDLLWorker(object sender, DoWorkEventArgs e)
        {
            var files = Directory.GetFiles(@"X:\source\" + GetCurrentBrand() + @"\dll\", "*.dll", SearchOption.AllDirectories).Where(i => Directory.GetLastWriteTime(i) > DateTime.Today).ToArray();
            foreach (var item in files) DLLListBox.Items.Add(item);
            var filesBAS = Directory.GetFiles(@"X:\source\" + GetCurrentBrand() + @"\dll\", "*.bas", SearchOption.AllDirectories).Where(i => Directory.GetLastWriteTime(i) > DateTime.Today).ToArray();
            foreach (var item in filesBAS) DLLListBox.Invoke(new Action(() => DLLListBox.Items.Add(item))); 
        }
        
        private void GetDLLModifiedFiles()
        {
            EazyPatchLoadign.Visible = true;
            EazyPatchCheckBx2.Enabled = false;
            DLLListBox.Items.Clear();
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(BackroundDLLWorker);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(work_RunDLLWorkerCompleted);
            worker.RunWorkerAsync();
        }
        void work_RunDLLWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EazyPatchLoadign.Visible = false;
            EazyPatchCheckBx2.Enabled = true;
        }
        private void button5_Click(object sender, EventArgs e)//Creat Patch Button
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
                    EazyPatchBtn1.Enabled = false;
                    EazyDescription.Enabled = false;
                    EazyPatchName.Enabled = false;
                    //
                    Directory.CreateDirectory(Path);
                    SetUpTemplateForNewPatch(GetCurrentBrand(), Path);
                    //EDIT UP3 files to match
                    string[] files = Directory.GetFiles(Path + @"\", "*.UP3", SearchOption.TopDirectoryOnly);
                    EazyProgBar.Maximum = files.Length+1;
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
                    int ADVCreated = 0, PROCreated = 0, DPCreated = 0;
                    foreach (string s in ExeFiles)
                    {
                        if (GetPatchState(s) == "PRO") PROCreated++;
                        if (GetPatchState(s) == "ADV") ADVCreated++;
                        if (GetPatchState(s) == "DP") DPCreated++;
                    }
                    ProgressText.Text = "Advanced: " + ADVCreated + " || PRO: "+ PROCreated + " || DP: "+ DPCreated;
                    
                    //SetBuildTypeAccordingToUnit();
                    EazyPatchBtn1.Text = "UPLOAD PATCH TO UNIT";
                    PubPathName = Path + @"\";
                    EazyPatchBtn1.Enabled = true;
                }
            }
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
                $"Total UP3's\t: {TotalUP3s}{Environment.NewLine}-------------------"; 
            System.IO.File.WriteAllText(NewPatchPath + @"\Readme.txt", patchText);
            ProgressText.Text = "Total UP3's Created: " + TotalUP3s;
        }

        private void UnitTextBox_TextChanged(object sender, EventArgs e)
        {
            if (UnitTextBox.Text.Length == 0) { UnitTypeLabel.Visible = false; return; }
            else UnitTypeLabel.Visible = true;
            if (Convert.ToInt32(UnitTextBox.Text) >= 2000 & Convert.ToInt32(UnitTextBox.Text) < 20000) UnitTypeLabel.Text = "BB";
            else if (Convert.ToInt32(UnitTextBox.Text) >= 20000 & Convert.ToInt32(UnitTextBox.Text) < 30000) UnitTypeLabel.Text = "A+";
            else if (Convert.ToInt32(UnitTextBox.Text) >= 30000 & Convert.ToInt32(UnitTextBox.Text) < 40000) UnitTypeLabel.Text = "DP";
            else UnitTypeLabel.Text = "..";
        }

        private void UnitTypeLabel_TextChanged(object sender, EventArgs e)
        {
            if (UnitTypeLabel.Text == "BB" | UnitTypeLabel.Text == "A+" | UnitTypeLabel.Text == "DP") CheckExeCompatibility();
            if (UnitTypeLabel.Text == "A+") { pictureBox7.Image = Properties.Resources.compare; pictureBox7.Enabled = true; }
            else{ pictureBox7.Image = Properties.Resources.compare_G; pictureBox7.Enabled = false;}
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
                tabPage3.ImageIndex = 1;
            }
        }

        private void CFGListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked) ToTalFileToUploadCount++;
            else if (e.NewValue == CheckState.Unchecked) ToTalFileToUploadCount--;
            ProgressText.Text = "Total Files Selected For Uploading: " + ToTalFileToUploadCount;
            EazyProgBar.Maximum = ToTalFileToUploadCount + 1;
            //more checks
            if (e.NewValue == CheckState.Checked & DLLListBox.Items.Count > 0)
                if (DLLListBox.Items[e.Index].ToString().Substring(DLLListBox.Items[e.Index].ToString().Length - 5).Contains(".bas") == true)
                    MessageBox.Show("Selecting Bas File! will automatically compile the DLL and build it!" + Environment.NewLine +
                    Environment.NewLine + "If you do not want to re-compile the DLL then select the .dll to be uploaded");
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            AP_Patches OpenForm = new AP_Patches(UnitTextBox.Text);
            OpenForm.Left = this.Bounds.Right - 370;
            OpenForm.Top = this.Bounds.Top + 106;
            OpenForm.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (radioButton1.Checked == true) Properties.Settings.Default.LastBrandBrowse = radioButton1.Name;
            if (radioButton2.Checked == true) Properties.Settings.Default.LastBrandBrowse = radioButton2.Name;
            if (radioButton3.Checked == true) Properties.Settings.Default.LastBrandBrowse = radioButton3.Name;
            if (radioButton4.Checked == true) Properties.Settings.Default.LastBrandBrowse = radioButton4.Name;
            if (radioButton5.Checked == true) Properties.Settings.Default.LastBrandBrowse = radioButton5.Name;
            if (radioButton6.Checked == true) Properties.Settings.Default.LastBrandBrowse = radioButton6.Name;
            if (radioButton7.Checked == true) Properties.Settings.Default.LastBrandBrowse = radioButton7.Name;
            if (radioButton8.Checked == true) Properties.Settings.Default.LastBrandBrowse = radioButton8.Name;
            if (radioButton9.Checked == true) Properties.Settings.Default.LastBrandBrowse = radioButton9.Name;
            if (radioButton10.Checked == true) Properties.Settings.Default.LastBrandBrowse = radioButton10.Name;
            Properties.Settings.Default.Save();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            string Path = @"K:\Autologic\OneOffFixes\" + ConvertBrandToFixes(GetCurrentBrand()) + @"\" + EazyPatchName.Text;
            if (Directory.Exists(Path) == true)
            {
                Process.Start("explorer.exe", Path);
            }
            else MessageBox.Show("Directory Not Found!" + Environment.NewLine + Environment.NewLine + "Patch Not Created yet ?");
        }
    }
}
