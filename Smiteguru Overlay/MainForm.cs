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
        Form1 f1 = new Form1();

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
            string godCode = sgGodCodes[comboBox1.SelectedIndex], itemCode;

            // load smiteguru god specific page
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(("http://smite.guru/builds/i/" + godCode));

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//ul[@class='list-inline text-center']");
            string nodeString = node.InnerHtml.ToString();

            for (int i = 0; i < 6; i++)
            {
                itemCode = nodeString.Substring(nodeString.IndexOf("cdn.smitegu.ru/assets/img/id/") + "cdn.smitegu.ru/assets/img/id/".Length);
                itemCode = itemCode.Substring(0, 9);
                links.Add("http://cdn.smitegu.ru/assets/img/id/" + itemCode);

                nodeString = nodeString.Replace("cdn.smitegu.ru/assets/img/id/" + itemCode, "");
            }

            if (node != null)
            {
                foreach (string item in links)
                {
                    //Console.WriteLine(item);
                }
            }
            else
            {
                Console.WriteLine("Nothing found!");
            }
        }

        private void getImageLinksArena()
        {
            string godCode = sgGodCodes[comboBox1.SelectedIndex], itemCode;

            // load smiteguru god specific page
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(("http://smite.guru/builds/i/" + godCode));

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div[@id='queue-435']//ul[@class='list-inline text-center']");
            string nodeString = node.InnerHtml.ToString();

            for (int i = 0; i < 6; i++)
            {
                itemCode = nodeString.Substring(nodeString.IndexOf("cdn.smitegu.ru/assets/img/id/") + "cdn.smitegu.ru/assets/img/id/".Length);
                itemCode = itemCode.Substring(0, 9);
                links.Add("http://cdn.smitegu.ru/assets/img/id/" + itemCode);

                nodeString = nodeString.Replace("cdn.smitegu.ru/assets/img/id/" + itemCode, "");
            }

            if (node != null)
            {
                foreach (string item in links)
                {
                    //Console.WriteLine(item);
                }
            }
            else
            {
                Console.WriteLine("Nothing found!");
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

        public MainForm(Form1 frm)
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
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            links.Clear();

            try
            {
                if (comboBox2.SelectedIndex == 0)
                {
                    getImageLinksArena();
                }
                else if (comboBox2.SelectedIndex == 1)
                {
                    getImageLinks();
                }

                f1.pictureBox1.Load(links[0]);
                f1.pictureBox2.Load(links[1]);
                f1.pictureBox3.Load(links[2]);
                f1.pictureBox4.Load(links[3]);
                f1.pictureBox5.Load(links[4]);
                f1.pictureBox6.Load(links[5]);
            }
            catch
            {

            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            links.Clear();

            try
            {
                if (comboBox2.SelectedIndex == 0)
                {
                    getImageLinksArena();
                }
                else if (comboBox2.SelectedIndex == 1)
                {
                    getImageLinks();
                }

                f1.pictureBox1.Load(links[0]);
                f1.pictureBox2.Load(links[1]);
                f1.pictureBox3.Load(links[2]);
                f1.pictureBox4.Load(links[3]);
                f1.pictureBox5.Load(links[4]);
                f1.pictureBox6.Load(links[5]);
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

    }
}