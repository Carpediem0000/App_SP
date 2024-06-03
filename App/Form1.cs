using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            cb_Disk.Items.Add("All Disk");
            cb_Disk.SelectedIndex = 0;
            foreach (var item in DriveInfo.GetDrives())
            {
                cb_Disk.Items.Add(item.Name);
            }

            cb_type.SelectedIndex = 0;
        }
        public static IEnumerable<string> EnumerateAllFilesAndDirectories(string path, string pattern)
        {
            IEnumerable<string> dirs = null;
            IEnumerable<string> files = null;
            try
            {
                dirs = Directory.EnumerateDirectories(path, pattern);
                files = Directory.EnumerateFiles(path, pattern);
            }
            catch { }

            if (dirs != null)
            {
                foreach (var dir in dirs)
                {
                    yield return dir;
                }
            }
            if (files != null)
            {
                foreach (var file in files)
                {
                    yield return file;
                }
            }

            IEnumerable<string> directories = null;
            try { directories = Directory.EnumerateDirectories(path); }
            catch { }

            if (directories != null)
            {
                foreach (var directory in directories)
                {
                    foreach (var res in EnumerateAllFilesAndDirectories(directory, pattern))
                    {
                        yield return res;
                    }
                }
            }
        }

        private void btn_Search_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            label4.Text = "";

            string pattern = null;

            if (tb_FileName.Text.Contains("*") || tb_FileName.Text.Contains("?"))
            {
                pattern = tb_FileName.Text;
            }
            else
            {
                if (cb_type.SelectedIndex != 0)
                {
                    pattern = $"{tb_FileName.Text}{cb_type.Text}";
                }
                else { pattern = $"{tb_FileName.Text}*"; }
            }


            if (cb_Disk.Text == "All Disk")
            {
                progressBar1.Value = 0;
                var drives = DriveInfo.GetDrives();
                progressBar1.Maximum = drives.Length * 1000;

                foreach (var drive in drives)
                {
                    new Thread(() =>
                    {
                        foreach (var fi in EnumerateAllFilesAndDirectories(drive.Name, pattern))
                        {
                            ListViewItem listViewItem = new ListViewItem(fi);
                            listView1.Items.Add(listViewItem);
                        }
                        lock (progressBar1)
                        {
                            progressBar1.Value += 1000;
                        }
                        lock (label4)
                        {
                            label4.Text = $"Found {listView1.Items.Count} matches";
                        }
                    }).Start();
                }
            }
            else
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = 10000;
                new Thread(() =>
                {
                    foreach (var fi in EnumerateAllFilesAndDirectories(cb_Disk.Text, pattern))
                    {
                        ListViewItem listViewItem = new ListViewItem(fi);
                        listView1.Items.Add(listViewItem);
                    }
                    progressBar1.Value = 10000;
                    label4.Text = $"Found {listView1.Items.Count} matches";
                }).Start();
            }

        }
    }
}
