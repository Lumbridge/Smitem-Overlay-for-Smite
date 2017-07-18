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

using static Smitem_Overlay.Classes.WinAPI;
using Smitem_Overlay.Classes;

namespace Smiteguru_Overlay
{
    public partial class MainForm : Form
    {
        IntPtr SmiteHandle = FindWindow("LaunchUnrealUWindowsClient", "Smite (32-bit, DX9)");

        overlay f1 = new overlay();

        #region Global Lists
        List<string> contents = new List<string>();
        List<God> Gods = new List<God>();
        List<string> ImageLinks = new List<string>();
        List<string> sgGodCodes = new List<string>();
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

        private List<God> GetSmiteGuruGodList()
        {
            List<God> GodList = new List<God>();

            WebClient webClient = new WebClient();
            string page = webClient.DownloadString("http://smite.guru/builds");

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a[@class='champion-md']"))
            {
                string innertext = node.InnerText;

                // Check for ' code and replace it
                if (innertext.Contains("&#039;"))
                    innertext = node.InnerText.Replace("&#039;", "'");

                // insert a symbol at the first double space to collect the god name easier
                innertext = innertext.Insert(innertext.IndexOf("  "), "^");

                string godName = GetSubstringByString(innertext[0].ToString(), "^", innertext);

                // replace the god name, symbol and double space with nothing to collect the class easier
                innertext = innertext.Replace(godName + "^  ", "");

                // add the new god to the list
                GodList.Add(
                    new God()
                    {
                        Name = godName,
                        Class = innertext.Trim()
                    });
            }

            // return the completed god list
            return GodList;
        }

        private List<string> GetMostPopularConquestItemImageLinks(string GodName)
        {
            if (GodName.Contains(" "))
                GodName = GodName.Replace(" ", "-");

            List<string> LinkList = new List<string>();

            WebClient webClient = new WebClient();
            string page = webClient.DownloadString("http://smite.guru/builds/" + GodName);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@id='426']"))
            {
                foreach (HtmlNode node2 in node.SelectNodes(".//div[@class='card-icon card-md']"))
                {
                    foreach (HtmlNode node3 in node2.SelectNodes(".//img[@src]"))
                    {
                        HtmlAttribute src = node3.Attributes["src"];

                        string innertext = src.Value.Replace("//", "");

                        // Check for ' code and replace it
                        if (innertext.Contains("&#039;"))
                            innertext = node3.InnerText.Replace("&#039;", "'");

                        LinkList.Add("http://" + innertext);
                    }
                }
            }

            // return the completed god list
            return LinkList;
        }

        private List<string> GetMostPopularArenaItemImageLinks(string GodName)
        {
            if (GodName.Contains(" "))
                GodName = GodName.Replace(" ", "-");

            List<string> LinkList = new List<string>();

            WebClient webClient = new WebClient();
            string page = webClient.DownloadString("http://smite.guru/builds/" + GodName);

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@id='435']"))
            {
                foreach (HtmlNode node2 in node.SelectNodes(".//div[@class='card-icon card-md']"))
                {
                    foreach (HtmlNode node3 in node2.SelectNodes(".//img[@src]"))
                    {
                        HtmlAttribute src = node3.Attributes["src"];

                        string innertext = src.Value.Replace("//", "");

                        // Check for ' code and replace it
                        if (innertext.Contains("&#039;"))
                            innertext = node3.InnerText.Replace("&#039;", "'");

                        LinkList.Add("http://" + innertext);
                    }
                }
            }

            // return the completed god list
            return LinkList;
        }


        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        protected override void WndProc(ref Message m)
        {
            // Hotkey Detection Method
            if (m.Msg == WM_HOTKEY && (int)m.WParam == 1)
            {
                // CTRL+SHIFT+K Hotkey Pressed (Show/Hide Item Overlay)
                if (!ContainsFocus)
                {
                    Activate();
                }
                else
                {
                    SetForegroundWindow(SmiteHandle);
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
            UnregisterHotKey(Handle, 1);
            UnregisterHotKey(Handle, 2);

            Application.Exit();
        }

        public MainForm(overlay frm)
        {
            InitializeComponent();

            f1 = frm;

            RegisterHotKey(Handle, 1, MOD_CONTROL + MOD_SHIFT, (int)Keys.X);
            RegisterHotKey(Handle, 2, MOD_CONTROL + MOD_SHIFT, (int)Keys.K);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            overlayXpos.Value = f1.Location.X;
            overlayYpos.Value = f1.Location.Y;

            Gods = GetSmiteGuruGodList();

            foreach (var god in Gods)
            {
                ComboBox_SelectedGod.Items.Add(god.Name);
            }

            Console.WriteLine(Gods.Count + " gods loaded.");
        }

        private void ComboBox_SelectedGod_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ComboBox_SelectedMode.SelectedIndex == 0)
                {
                    // arena
                    ImageLinks = GetMostPopularArenaItemImageLinks(ComboBox_SelectedGod.Text.ToLower());
                }
                else if (ComboBox_SelectedMode.SelectedIndex == 1)
                {
                    //conq
                    ImageLinks = GetMostPopularConquestItemImageLinks(ComboBox_SelectedGod.Text.ToLower());
                }

                f1.pictureBox1.Load(ImageLinks[0]);
                f1.pictureBox2.Load(ImageLinks[1]);
                f1.pictureBox3.Load(ImageLinks[2]);
                f1.pictureBox4.Load(ImageLinks[3]);
                f1.pictureBox5.Load(ImageLinks[4]);
                f1.pictureBox6.Load(ImageLinks[5]);
            }
            catch (Exception ex)
            {
                
            }
        }

        private void ComboBox_SelectedMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ComboBox_SelectedMode.SelectedIndex == 0)
                {
                    // arena
                    ImageLinks = GetMostPopularArenaItemImageLinks(ComboBox_SelectedGod.Text.ToLower());
                }
                else if (ComboBox_SelectedMode.SelectedIndex == 1)
                {
                    // conq
                    ImageLinks = GetMostPopularConquestItemImageLinks(ComboBox_SelectedGod.Text.ToLower());
                }

                f1.pictureBox1.Load(ImageLinks[0]);
                f1.pictureBox2.Load(ImageLinks[1]);
                f1.pictureBox3.Load(ImageLinks[2]);
                f1.pictureBox4.Load(ImageLinks[3]);
                f1.pictureBox5.Load(ImageLinks[4]);
                f1.pictureBox6.Load(ImageLinks[5]);
            }
            catch (Exception ex)
            {
                
            }
        }

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