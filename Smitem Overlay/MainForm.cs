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
        List<List<string>> tableHeadings;
        List<List<string>> tableContents;
        #endregion

        Rectangle resolution = Screen.PrimaryScreen.Bounds;

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        // Hotkey controls
        const int MOD_CONTROL = 0x0002;
        const int MOD_SHIFT = 0x0004;
        const int WM_HOTKEY = 0x0312;

        bool overlayEnabled = true;

        private void getSmiteguruGodList()
        {
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(("http://smite.guru/builds"));

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@id='gods-container']//a//h6//strong");
            
            foreach(HtmlNode node in nodes)
            {
                if (node.InnerHtml.Contains("&#039;"))
                    node.InnerHtml = node.InnerHtml.Replace("&#039;", "'");

                godNames.Add(node.InnerHtml.Trim());
            }
        }

        private void getImageLinks()
        {
            string godCode = sgGodCodes[comboBox1.SelectedIndex], itemCode;

            // load smiteguru god specific page
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(("http://smite.guru/builds/i/" + godCode));

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//ul[@class='list-inline text-center']");

            for (int i = 0; i < 6; i++)
            {
                itemCode = node.InnerHtml.Substring(node.InnerHtml.IndexOf("cdn.smitegu.ru/assets/img/id/") + "cdn.smitegu.ru/assets/img/id/".Length);
                itemCode = itemCode.Substring(0, 9);
                links.Add("http://cdn.smitegu.ru/assets/img/id/" + itemCode);

                node.InnerHtml = node.InnerHtml.Replace("cdn.smitegu.ru/assets/img/id/" + itemCode, "");
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
            overlayXpos.Value = f1.Location.X;
            overlayYpos.Value = f1.Location.Y;

            getSmiteguruGodList();
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

        private void overlayXpos_ValueChanged(object sender, EventArgs e)
        {
            f1.Location = new Point(Convert.ToInt32(overlayXpos.Value), Convert.ToInt32(overlayYpos.Value));
        }

        private void overlayYpos_ValueChanged(object sender, EventArgs e)
        {
            f1.Location = new Point(Convert.ToInt32(overlayXpos.Value), Convert.ToInt32(overlayYpos.Value));
        }

        private void topLeftButton_Click(object sender, EventArgs e)
        {
            f1.Location = new Point(resolution.X + 10, resolution.Y);

            overlayXpos.Value = resolution.X + 10;
            overlayYpos.Value = resolution.Y;
        }

        private void topRightButton_Click(object sender, EventArgs e)
        {
            f1.Location = new Point(resolution.Width - (f1.Width + 10), resolution.Y);

            overlayXpos.Value = resolution.Width - (f1.Width + 10);
            overlayYpos.Value = resolution.Y;
        }
    }
}