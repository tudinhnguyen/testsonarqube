using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teechip.Controller;
using System.Diagnostics;
using System.IO;

namespace Teechip.View
{
    public partial class frmURLShortener : Form
    {
        public frmURLShortener()
        {
            InitializeComponent();
        }

        List<string> ExportFiles = new List<string>();
        TeechipControl TeechipControl = TeechipControl.Instance;
        Stopwatch Stopwatch = new Stopwatch();
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() == "" || textBox2.Text == "")
            {
                MessageBox.Show(
                    string.Format("Please input gmail account and password!")
                    , this.Text
                    , MessageBoxButtons.OK
                    , MessageBoxIcon.Exclamation
                    );

                return;
            }


            Stopwatch.Start();

            if (!TeechipControl.Login(0, textBox1.Text.Trim(), textBox2.Text, checkBox1.Checked))
            {
                MessageBox.Show(
                    string.Format("Invalid account or password!")
                    , this.Text
                    , MessageBoxButtons.OK
                    , MessageBoxIcon.Exclamation
                    );

                return;
            }
            OpenFileDialog openDlg = new OpenFileDialog();

            openDlg.Multiselect = true;
            openDlg.CheckFileExists = true;
            openDlg.Title = "Choose export file(s) ...";
            openDlg.Filter = "All Export Files|*.txt";
            if (openDlg.ShowDialog(this) == DialogResult.OK)
            {
                button1.Enabled = false;

                ExportFiles.Clear();
                foreach (var item in openDlg.FileNames)
                {
                    ExportFiles.Add(item);
                }

                timer1.Interval = 1000;
                timer1.Enabled = true;
            }
        }

        struct stCPLink
        {
            public string Title;
            public string URL;
            public string ImgFile;
            public string DefaultProduct { get; set; }
            public string DefaultColor { get; set; }
            public string DefaultSide { get; set; }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            try
            {
                foreach (var item in ExportFiles)
                {
                    Dictionary<string, stCPLink> CPLinks = new Dictionary<string, stCPLink>();
                    using (StreamReader sr = new StreamReader(item))
                    {
                        string line = sr.ReadLine();
                        while (!string.IsNullOrEmpty(line))
                        {
                            string[] arr = line.Split('|');

                            stCPLink CPLink = new stCPLink();

                            CPLink.Title = arr[0].Trim();
                            CPLink.URL = TeechipControl.Shortener(arr[1].Trim());
                            CPLink.ImgFile = arr[2].Trim();

                            if (arr.Length >= 6)
                            {
                                CPLink.DefaultProduct = arr[3].Trim();
                                CPLink.DefaultColor = arr[4].Trim();
                                CPLink.DefaultSide = arr[5].Trim();
                            }

                            CPLinks.Add(CPLink.URL, CPLink);

                            line = sr.ReadLine();

                            toolStripStatusLabel1.Text = string.Format("Eslapsed: {0}. Processing {1} ...", StopwatchEslapse(), item);
                        }
                    }

                    using (StreamWriter sw = new StreamWriter(item))
                    {
                        foreach (string url in CPLinks.Keys)
                        {
                            sw.WriteLine(string.Format("{0} | {1} | {2} | {3} | {4} | {5}", CPLinks[url].Title, CPLinks[url].URL, CPLinks[url].ImgFile, CPLinks[url].DefaultProduct, CPLinks[url].DefaultColor, CPLinks[url].DefaultSide));
                            
                            toolStripStatusLabel1.Text = string.Format("Eslapsed: {0}. Processing {1} ...", StopwatchEslapse(), item);
                        }
                    }
                }
            }
            catch
            {
            }


            MessageBox.Show(
                string.Format("Process completed.")
                , this.Text
                , MessageBoxButtons.OK
                , MessageBoxIcon.Information
                );

            button1.Enabled = true;

            Stopwatch.Stop();
        }

        string StopwatchEslapse()
        {
            TimeSpan ts = Stopwatch.Elapsed;

            // Format and display the TimeSpan value. 
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}",
                ts.Hours, ts.Minutes, ts.Seconds);

            return elapsedTime;
        }

        private void frmURLShortener_Load(object sender, EventArgs e)
        {
			this.Text += " - " + FileVersionInfo.GetVersionInfo(Path.Combine(Environment.SystemDirectory, Application.ExecutablePath)).FileVersion.ToString() + " - " + File.GetLastWriteTime(Application.ExecutablePath).ToString("yyyy-MM-dd");
            string user = "";
            string pwd = "";
            int type = 0;
            if (Utility.LoadAccount(TeechipControl.LoginInforFile2, ref user, ref pwd, ref type))
            {
                checkBox1.Checked = true;
                textBox1.Text = user;
                textBox2.Text = pwd;
            }
        }
    }
}
