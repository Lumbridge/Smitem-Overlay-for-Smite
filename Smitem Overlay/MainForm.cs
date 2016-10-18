using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Diagnostics;
using HtmlAgilityPack;

namespace Smiteguru_Overlay
{
    public partial class MainForm : Form
    {
        overlay f1 = new overlay();

        #region Global Lists
        List<string> contents = new List<string>();
        List<string> godNames = new List<string>();
        List<string> godPantheon = new List<string>();
        List<string> godAttackType = new List<string>();
        List<string> godPowerType = new List<string>();
        List<string> godClass = new List<string>();
        List<string> godFavorCost = new List<string>();
        List<string> godGemsCost = new List<string>();
        List<string> godReleaseDate = new List<string>();
        List<string> sgGodCodes = new List<string>();
        List<string> links = new List<string>();

        List<string> starterItems = new List<string>();
        List<string> coreItems = new List<string>();
        List<string> defensiveItems = new List<string>();
        List<string> damageItems = new List<string>();

        List<List<string>> tableHeadings;
        List<List<string>> tableContents;
        #endregion

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        // Hotkey controls
        const int MOD_CONTROL = 0x0002;
        const int MOD_SHIFT = 0x0004;
        const int WM_HOTKEY = 0x0312;

        bool overlayEnabled = true;

        public string Between(string STR, string FirstString, string LastString)
        {
            string FinalString;
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2 = STR.IndexOf(LastString);
            FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            return FinalString;
        }

        private void downloadWebpage()
        {
            WebClient webClient = new WebClient();
            string page = webClient.DownloadString("http://smite.gamepedia.com/List_of_gods");

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);

            doc.OptionFixNestedTags = true;

            tableHeadings = doc.DocumentNode.SelectSingleNode("//table")
                .Descendants("tr")
                .Skip(0)
                .Where(tr => tr.Elements("th").Count() > 1)
                .Select(tr => tr.Elements("th").Select(th => th.InnerText.Trim()).ToList())
                .ToList();

            tableContents = doc.DocumentNode.SelectSingleNode("//table")
                .Descendants("tr")
                .Skip(0)
                .Where(tr => tr.Elements("td").Count() > 1)
                .Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
                .ToList();
        }

        private void constructDataListView()
        {
            downloadWebpage();

            // adding all table contents to a list "contents"
            foreach (List<string> list in tableContents)
            {
                foreach (string items in list)
                {
                    contents.Add(items);
                }
            }
            // add god names to list 1n+9
            for (int i = 1; i < contents.Count; i += 9)
            {
                godNames.Add(contents[i]);
            }
            //// add pantheons to list 2n+9
            //for (int i = 2; i < contents.Count; i += 9)
            //{
            //    godPantheon.Add(contents[i]);
            //}
            //// add attack type to list 3n+9
            //for (int i = 3; i < contents.Count; i += 9)
            //{
            //    godAttackType.Add(contents[i]);
            //}
            //// add power type to list 4n+9
            //for (int i = 4; i < contents.Count; i += 9)
            //{
            //    godPowerType.Add(contents[i]);
            //}
            //// add class to list 5n+9
            //for (int i = 5; i < contents.Count; i += 9)
            //{
            //    godClass.Add(contents[i]);
            //}
            //// add favor cost to list 6n+9
            //for (int i = 6; i < contents.Count; i += 9)
            //{
            //    godFavorCost.Add(contents[i]);
            //}
            //// add gems cost to list 7n+9
            //for (int i = 7; i < contents.Count; i += 9)
            //{
            //    godGemsCost.Add(contents[i]);
            //}
            //// add release date to list 8n+9
            //for (int i = 8; i < contents.Count; i += 9)
            //{
            //    godReleaseDate.Add(contents[i]);
            //}

            //// add all data from lists to table
            //for (int i = 0; i < godNames.Count; i++)
            //{
            //    dataGridView1.Rows.Add();

            //    dataGridView1.Rows[i].Cells[0].Value = godNames[i];
            //    dataGridView1.Rows[i].Cells[1].Value = godPantheon[i];
            //    dataGridView1.Rows[i].Cells[2].Value = godAttackType[i];
            //    dataGridView1.Rows[i].Cells[3].Value = godPowerType[i];
            //    dataGridView1.Rows[i].Cells[4].Value = godClass[i];
            //    dataGridView1.Rows[i].Cells[5].Value = godFavorCost[i];
            //    dataGridView1.Rows[i].Cells[6].Value = godGemsCost[i];
            //    dataGridView1.Rows[i].Cells[7].Value = godReleaseDate[i];
            //}
        }

        private void getImageLinks()
        {
            // load smiteguru god specific page
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(("http://smite.gamepedia.com/" + comboBox1.SelectedItem + "#Standard Build"));

            // get HTML from div <div class="tabbertab" title="Arena Build" style="display: block;">
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[contains(@title,'Standard Build')]//table[@class='wikitable']");

            starterItems.Clear();
            coreItems.Clear();
            defensiveItems.Clear();
            damageItems.Clear();
            
            // get defense items
            for (int i = 0; i < 3; i++)
            {
                string link = Between(nodes[4].InnerHtml, "src=\"", "\" width=\"64\"");
                defensiveItems.Add(link);
                Console.WriteLine("Added " + link + "\n");
                nodes[4].InnerHtml = nodes[4].InnerHtml.Substring(nodes[4].InnerHtml.IndexOf("</a></td>"));
            }

            // get core items
            for (int i = 0; i < 3; i++)
            {
                string link = Between(nodes[3].InnerHtml, "src=\"", "\" width=\"64\"");
                coreItems.Add(link);
                Console.WriteLine("Added " + link + "\n");
                nodes[3].InnerHtml = nodes[3].InnerHtml.Substring(nodes[3].InnerHtml.IndexOf("</a></td>"));
            }

            // get damage items
            for (int i = 0; i < 3; i++)
            {
                string link = Between(nodes[1].InnerHtml, "src=\"", "\" width=\"64\"");
                damageItems.Add(link);
                Console.WriteLine("Added " + link + "\n");
                nodes[1].InnerHtml = nodes[1].InnerHtml.Substring(nodes[1].InnerHtml.IndexOf("</a></td>"));
            }

            //get starter items
            for (int i = 0; i < 3; i++)
            {
                string link = Between(nodes[0].InnerHtml, "src=\"", "\" width=\"64\"");
                starterItems.Add(link);
                Console.WriteLine("Added " + link + "\n");
                nodes[0].InnerHtml = nodes[0].InnerHtml.Substring(nodes[0].InnerHtml.IndexOf("</a></td>"));
            }

        }

        private void getImageLinksArena()
        {
            // load smiteguru god specific page
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(("http://smite.gamepedia.com/"+comboBox1.SelectedItem+"#Arena Build"));

            // get HTML from div <div class="tabbertab" title="Arena Build" style="display: block;">
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[contains(@title,'Arena Build')]//table[@class='wikitable']");

            starterItems.Clear();
            coreItems.Clear();
            defensiveItems.Clear();
            damageItems.Clear();

            // get damage items
            for (int i = 0; i < 3; i++)
            {
                string link = Between(nodes[4].InnerHtml, "src=\"", "\" width=\"64\"");
                damageItems.Add(link);
                Console.WriteLine("Added " + link + "\n");
                nodes[4].InnerHtml = nodes[4].InnerHtml.Substring(nodes[4].InnerHtml.IndexOf("</a></td>"));
            }

            // get defense items
            for (int i = 0; i < 3; i++)
            {
                string link = Between(nodes[1].InnerHtml, "src=\"", "\" width=\"64\"");
                defensiveItems.Add(link);
                Console.WriteLine("Added " + link + "\n");
                nodes[1].InnerHtml = nodes[1].InnerHtml.Substring(nodes[1].InnerHtml.IndexOf("</a></td>"));
            }

            // get core items
            for (int i = 0; i < 3; i++)
            {
                string link = Between(nodes[0].InnerHtml, "src=\"", "\" width=\"64\"");
                coreItems.Add(link);
                Console.WriteLine("Added " + link + "\n");
                nodes[0].InnerHtml = nodes[0].InnerHtml.Substring(nodes[0].InnerHtml.IndexOf("</a></td>"));
            }
        }

        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private int getRowIndexByColumnNameAndSearchTerm(string columnHeader, string searchTerm)
        {
            int rowIndex = -1;

            DataGridViewRow row = dataGridView1.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells[columnHeader].Value.ToString().Equals(searchTerm))
                .First();

            rowIndex = row.Index;

            return rowIndex;
        }

        IntPtr smiteHandle = FindWindow("LaunchUnrealUWindowsClient", "Smite (32-bit, DX9)");

        protected override void WndProc(ref Message m)
        {
            // Hotkey Detection Method
            if (m.Msg == WM_HOTKEY && (int)m.WParam == 1)
            {
                // CTRL+SHIFT+K Hotkey Pressed (Show/Hide Item Overlay)
                if (!this.ContainsFocus)
                {
                    this.Activate();
                }
                else
                {
                    SetForegroundWindow(smiteHandle);
                }
            }
            else if (m.Msg == WM_HOTKEY && (int)m.WParam == 2)
            {
                int openForms = Application.OpenForms.Count;

                // CTRL+SHIFT+X Hotkey Pressed (Show/Hide Config Window)
                if (overlayEnabled == true)
                {
                    Application.OpenForms[0].Opacity = 0;
                    overlayEnabled = false;
                }
                else
                {
                    Application.OpenForms[0].Opacity = 100;
                    overlayEnabled = true;
                }
            }
            base.WndProc(ref m);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, 1);
            UnregisterHotKey(this.Handle, 2);

            Application.Exit();
        }

        private void getGodCodes()
        {
            // load smiteguru god build page
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load("http://smite.guru/builds");

            HtmlNode node = doc.DocumentNode.SelectNodes("//div[@id='gods-container']").First();

            string allHTML = node.InnerHtml.ToString();
            string godCode = allHTML.Substring(allHTML.IndexOf("/i/") + "/i/".Length);

            // load all god codes into sgGodCodes
            foreach (string name in godNames)
            {
                godCode = allHTML.Substring(allHTML.IndexOf("/i/") + "/i/".Length);
                godCode = godCode.Substring(0, 4);

                sgGodCodes.Add(godCode);
                //Console.WriteLine(name + " - " + godCode);

                allHTML = allHTML.Replace("/i/" + godCode, "");
            }
        }

        public MainForm(overlay frm)
        {
            InitializeComponent();

            f1 = frm;

            RegisterHotKey(this.Handle, 1, MOD_CONTROL + MOD_SHIFT, (int)Keys.X);
            RegisterHotKey(this.Handle, 2, MOD_CONTROL + MOD_SHIFT, (int)Keys.K);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            constructDataListView();
            foreach (string name in godNames)
            {
                comboBox1.Items.Add(name);
            }
            getGodCodes();

            overlayXpos.Value = f1.Location.X;
            overlayYpos.Value = f1.Location.Y;
        }

        private void clearPicBoxes()
        {
            f1.pictureBox1.Image.Dispose();
            f1.pictureBox2.Image.Dispose();
            f1.pictureBox3.Image.Dispose();
            f1.pictureBox4.Image.Dispose();
            f1.pictureBox5.Image.Dispose();
            f1.pictureBox6.Image.Dispose();
            f1.pictureBox7.Image.Dispose();
            f1.pictureBox8.Image.Dispose();
            f1.pictureBox9.Image.Dispose();
        }

        private void arenaLayout()
        {
            f1.pictureBox10.Hide();
            f1.pictureBox11.Hide();
            f1.pictureBox12.Hide();
            f1.starterLabel.Hide();
        }

        private void standardLayout()
        {
            f1.pictureBox10.Show();
            f1.pictureBox11.Show();
            f1.pictureBox12.Show();
            f1.starterLabel.Show();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBox2.SelectedIndex == 0)
                {
                    getImageLinksArena();

                    arenaLayout();

                    f1.pictureBox1.Load(coreItems[0]);
                    f1.pictureBox2.Load(coreItems[1]);
                    f1.pictureBox3.Load(coreItems[2]);
                    f1.pictureBox4.Load(damageItems[0]);
                    f1.pictureBox5.Load(damageItems[1]);
                    f1.pictureBox6.Load(damageItems[2]);
                    f1.pictureBox7.Load(defensiveItems[0]);
                    f1.pictureBox8.Load(defensiveItems[1]);
                    f1.pictureBox9.Load(defensiveItems[2]);
                }
                else if (comboBox2.SelectedIndex == 1)
                {
                    getImageLinks();

                    standardLayout();

                    f1.pictureBox10.Load(starterItems[0]);
                    f1.pictureBox11.Load(starterItems[1]);
                    f1.pictureBox12.Load(starterItems[2]);
                    f1.pictureBox1.Load(coreItems[0]);
                    f1.pictureBox2.Load(coreItems[1]);
                    f1.pictureBox3.Load(coreItems[2]);
                    f1.pictureBox4.Load(damageItems[0]);
                    f1.pictureBox5.Load(damageItems[1]);
                    f1.pictureBox6.Load(damageItems[2]);
                    f1.pictureBox7.Load(defensiveItems[0]);
                    f1.pictureBox8.Load(defensiveItems[1]);
                    f1.pictureBox9.Load(defensiveItems[2]);
                }
            }
            catch
            {

            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBox2.SelectedIndex == 0)
                {
                    getImageLinksArena();

                    arenaLayout();

                    f1.pictureBox1.Load(coreItems[0]);
                    f1.pictureBox2.Load(coreItems[1]);
                    f1.pictureBox3.Load(coreItems[2]);
                    f1.pictureBox4.Load(damageItems[0]);
                    f1.pictureBox5.Load(damageItems[1]);
                    f1.pictureBox6.Load(damageItems[2]);
                    f1.pictureBox7.Load(defensiveItems[0]);
                    f1.pictureBox8.Load(defensiveItems[1]);
                    f1.pictureBox9.Load(defensiveItems[2]);
                }
                else if (comboBox2.SelectedIndex == 1)
                {
                    getImageLinks();

                    standardLayout();

                    f1.pictureBox10.Load(starterItems[0]);
                    f1.pictureBox11.Load(starterItems[1]);
                    f1.pictureBox12.Load(starterItems[2]);
                    f1.pictureBox1.Load(coreItems[0]);
                    f1.pictureBox2.Load(coreItems[1]);
                    f1.pictureBox3.Load(coreItems[2]);
                    f1.pictureBox4.Load(damageItems[0]);
                    f1.pictureBox5.Load(damageItems[1]);
                    f1.pictureBox6.Load(damageItems[2]);
                    f1.pictureBox7.Load(defensiveItems[0]);
                    f1.pictureBox8.Load(defensiveItems[1]);
                    f1.pictureBox9.Load(defensiveItems[2]);
                }
            }
            catch
            {

            }
        }

        #region Invoke DLLs
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName,
            string lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion

        private void overlayXpos_ValueChanged(object sender, EventArgs e)
        {
            f1.Location = new Point(Convert.ToInt32(overlayXpos.Value), Convert.ToInt32(overlayYpos.Value));
        }

        private void overlayYpos_ValueChanged(object sender, EventArgs e)
        {
            f1.Location = new Point(Convert.ToInt32(overlayXpos.Value), Convert.ToInt32(overlayYpos.Value));
        }
    }
}