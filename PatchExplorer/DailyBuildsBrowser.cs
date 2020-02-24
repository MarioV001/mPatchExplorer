using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatchExplorer
{
    public partial class DailyBuildsBrowser : Form
    {
        public DailyBuildsBrowser()
        {
            InitializeComponent();
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            if (FullLiveVersions.Count() == 0)//only load Info Once!
            { 
                var baseAddress = new Uri("https://builds.autologic.com/Identity/Account/Login");
                var cookieContainer = new System.Net.CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    var homePageResult = client.GetAsync("/");
                    //homePageResult.Result.EnsureSuccessStatusCode();
                    string reat = await homePageResult.Result.Content.ReadAsStringAsync();
                    string Token = "";

                    string[] PageLines = reat.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in PageLines)
                    {
                        if (line.Contains("__RequestVerificationToken"))
                        {
                            string[] Splitter = line.Split('"');
                            Token = Splitter[5];
                            break;
                        }
                    }
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("Input.Email", "technical-services-team@autologic.com"),
                        new KeyValuePair<string, string>("Input.Password", "techserv1"),
                        new KeyValuePair<string, string>("__RequestVerificationToken", Token),
                    });
                    HttpResponseMessage loginResult = client.PostAsync(baseAddress, content).Result;
                    //loginResult.EnsureSuccessStatusCode();
                    var content23 = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("PageSize", "200"),//Force page to show 200 items at once
                    });
                    loginResult = client.PostAsync("https://builds.autologic.com/Build", content23).Result;
                    reat = await loginResult.Content.ReadAsStringAsync();
                    SaveVersions(reat);//Save New info...
                }
            }
            listView1.Items.Clear();
            ShowFilteredVersions();//add to list
        }
        List<string> FullLiveVersions = new List<string>();
        void SaveVersions(string input)
        {
            FullLiveVersions.Clear();
            string Edit = input.Replace(Environment.NewLine, "").Replace("    ", "");
            MatchCollection matches = Regex.Matches(Edit, "<td data-title=\"Brand\">.*?</");//" <.*?>"
            MatchCollection matchesED = Regex.Matches(Edit, "<td data-title=\"Edition\">.*?</");//" <.*?>"
            MatchCollection matchesVER = Regex.Matches(Edit, "<td data-title=\"Version\">.*?</");//" <.*?>"
            MatchCollection matchesSTAT = Regex.Matches(Edit, "<td data-title=\"Status\">.*?</td>");//" <.*?>"
            
            for (int count = 0; count < matches.Count; count++)//clear out the names
            {
                string Formats = matches[count].Value.Replace("<td data-title=\"Brand\">", "").Replace("</", "") + "|" +
                matchesED[count].Value.Replace("<td data-title=\"Edition\">", "").Replace("</", "") + "|" +
                matchesVER[count].Value.Replace("<td data-title=\"Version\">", "").Replace("</", "");
                string status = matchesSTAT[count].Value.Replace("<td data-title=\"Status\">", "");
                Formats += "|" + Regex.Replace(status, @"<span.*?</span>", "").Replace("</td", "").Replace(">", "");
                FullLiveVersions.Add(Formats);
            }
            matches = null;
            matchesED = null;
            matchesVER = null;
            matchesSTAT = null;
        }
        void ShowFilteredVersions()
        {
            int Total = 0;
            foreach (string Name in FullLiveVersions)
            {
                bool CanAdd = false, CanAdd2 = false;
                string[] SplitID = Name.Split('|');
                if (BrandComboBox.SelectedIndex != 0 & SplitID[0] == BrandComboBox.Text) CanAdd = true;
                else if(BrandComboBox.SelectedIndex == 0) CanAdd = true;
                ListViewItem item1 = new ListViewItem(SplitID[0], 0);//Brand
                item1.SubItems.Add(SplitID[1]);//Edition
                item1.SubItems.Add(SplitID[2]);//Version
                if (StatusComboBox.SelectedIndex != 0 & SplitID[3] == StatusComboBox.Text) CanAdd2 = true;
                else if (StatusComboBox.SelectedIndex == 0) CanAdd2 = true;
                item1.SubItems.Add(SplitID[3]);//Status
                if (CanAdd == true & CanAdd2 == true) { listView1.Items.Add(item1); Total++; }
            }
            label5.Text = $"Found ({Total})";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BrandComboBox.SelectedIndex = 0;
            StatusComboBox.SelectedIndex = 0;
        }

        private void DailyBuildsBrowser_Load(object sender, EventArgs e)
        {
            BrandComboBox.SelectedIndex = 0;
            StatusComboBox.SelectedIndex = 0;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DailyBuildsBrowser_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
