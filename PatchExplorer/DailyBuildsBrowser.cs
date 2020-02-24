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
            var baseAddress = new Uri("https://builds.autologic.com/Identity/Account/Login");
            var cookieContainer = new System.Net.CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var homePageResult = client.GetAsync("/");
                homePageResult.Result.EnsureSuccessStatusCode();
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
                loginResult.EnsureSuccessStatusCode();
                //string contentasd = await loginResult.Content.ReadAsStringAsync();
                //get the stats
                var content23 = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("PageSize", "200"),
                });
                loginResult = client.PostAsync("https://builds.autologic.com/Build", content23).Result;
                reat = await loginResult.Content.ReadAsStringAsync();

                listView1.Items.Clear();
                ExtractText(reat);//add to list
            }
        }

        void ExtractText(string input)
        {
            string Edit = input.Replace(Environment.NewLine, "").Replace("    ", "");
            MatchCollection matches = Regex.Matches(Edit, "<td data-title=\"Brand\">.*?</");//" <.*?>"
            MatchCollection matchesED = Regex.Matches(Edit, "<td data-title=\"Edition\">.*?</");//" <.*?>"
            MatchCollection matchesVER = Regex.Matches(Edit, "<td data-title=\"Version\">.*?</");//" <.*?>"
            MatchCollection matchesSTAT = Regex.Matches(Edit, "<td data-title=\"Status\">.*?</td>");//" <.*?>"
            List<string> logListContent = new List<string>();
            for (int count = 0; count < matches.Count; count++)//clear out the names
            {
                ListViewItem item1 = new ListViewItem(matches[count].Value.Replace("<td data-title=\"Brand\">", "").Replace("</", ""), 0);
                item1.SubItems.Add(matchesED[count].Value.Replace("<td data-title=\"Edition\">", "").Replace("</", ""));//Version
                item1.SubItems.Add(matchesVER[count].Value.Replace("<td data-title=\"Version\">", "").Replace("</", ""));//Version-PT
                string status = matchesSTAT[count].Value.Replace("<td data-title=\"Status\">", "");
                item1.SubItems.Add(Regex.Replace(status, @"<span.*?</span>", "").Replace("</td", "").Replace(">", ""));
                listView1.Items.Add(item1);
            }
        }
    }
}
