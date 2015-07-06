// Decompiled with JetBrains decompiler
// Type: Loader.Forms.Updater
// Assembly: Loader, Version=0.1.5611.35443, Culture=neutral, PublicKeyToken=null
// MVID: 767D8978-23D8-4AB7-BA8A-78DBFB5F0780
// Assembly location: E:\Downloads\ensage\Dumps\Loader_fix.exe

using Loader;
using Loader.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Windows.Forms;

namespace Loader.Forms
{
    public class Updater : Form
    {
        private IContainer components;
        private ProgressBar progressBar;
        private Label statusLabel;
        private BackgroundWorker updateWorker;
        private System.Windows.Forms.Timer closeTimer;

        public Updater()
        {
            InitializeComponent();
            if (!System.IO.File.Exists("LibGit2Sharp.dll") || !(FileVersionInfo.GetVersionInfo("LibGit2Sharp.dll").FileVersion != "0.18.1"))
                return;
            System.IO.File.Delete("LibGit2Sharp.dll");
            foreach (string path in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                string fileName = Path.GetFileName(path);
                if (fileName != null && fileName.StartsWith("git2-"))
                    System.IO.File.Delete(path);
            }
        }

        private void Update_Shown(object sender, EventArgs e)
        {
            updateWorker.RunWorkerAsync();
        }

        private byte[] CheckMD5Version(string parameter)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(parameter);
            WebRequest webRequest = WebRequest.Create("http://www.zynox.net/ensage/md5.php");
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = bytes.Length;
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Flush();
            }
            using (WebResponse response = webRequest.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream == null)
                        throw new Exception("Can't get response stream");
                    using (BinaryReader binaryReader = new BinaryReader(responseStream))
                        return binaryReader.ReadBytes(4);
                }
            }
        }

        private void DecryptFile(string name)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(name);
            for (int index = 0; index < bytes.Length; ++index)
                bytes[index] ^= 39;
            System.IO.File.WriteAllBytes(name, bytes);
        }

        private void closeTimer_Tick(object sender, EventArgs e)
        {
            closeTimer.Stop();
            Program.StartLoginForm = true;
            Close();
        }

        private void updateWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                if (e.UserState is Exception)
                    statusLabel.Text = Resources.ErrorException + (e.UserState as Exception).Message;
                else if (e.UserState is string)
                {
                    string str = e.UserState as string;
                    if (str == "exit")
                    {
                        Close();
                        return;
                    }
                    statusLabel.Text = str;
                }
            }
            progressBar.Value = e.ProgressPercentage;
            if (progressBar.Value != 100)
                return;
            closeTimer.Start();
        }

        private void updateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (Dns.GetHostEntry("www.zynox.net").AddressList[0].ToString() == "127.0.0.1")
                {
                    updateWorker.ReportProgress(100, "exit");
                }
                else
                {
                    updateWorker.ReportProgress(10, "Checking version...");
                    StringBuilder stringBuilder = new StringBuilder();
                    using (Md5 md5 = new Md5())
                    {
                        try
                        {
                            string hash = md5.GetHash("Ensage.dll");
                            stringBuilder.Append("dll=" + hash);
                        }
                        catch
                        {
                        }
                        try
                        {
                            System.IO.File.Copy("ELoader.exe", "tmpl", true);
                            string hash = md5.GetHash("tmpl");
                            if (stringBuilder.Length > 0)
                                stringBuilder.Append("&");
                            stringBuilder.Append("loader=" + hash);
                            System.IO.File.Delete("tmpl");
                        }
                        catch
                        {
                        }
                        try
                        {
                            string hash = md5.GetHash("LoaderDLL.dll");
                            if (stringBuilder.Length > 0)
                                stringBuilder.Append("&");
                            stringBuilder.Append("loaderdll=" + hash);
                        }
                        catch
                        {
                        }
                        try
                        {
                            if (System.IO.File.Exists("EnsageV2.dll"))
                            {
                                string hash = md5.GetHash("EnsageV2.dll");
                                if (stringBuilder.Length > 0)
                                    stringBuilder.Append("&");
                                stringBuilder.Append("dll2=" + hash);
                            }
                            else
                            {
                                if (stringBuilder.Length > 0)
                                    stringBuilder.Append("&");
                                stringBuilder.Append("dll2=1234");
                            }
                        }
                        catch
                        {
                        }
                    }
                    if (stringBuilder.Length == 0)
                    {
                        updateWorker.ReportProgress(100, "Can't access dll and loader!");
                    }
                    else
                    {
                        Program.VersionString = stringBuilder.ToString();
                        using (WebClient webClient = new WebClient())
                        {
                            webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                            updateWorker.ReportProgress(30);
                            if (!System.IO.File.Exists("git2-90befde.dll"))
                            {
                                updateWorker.ReportProgress(31, "Downloading git libraries");
                                webClient.DownloadFile("http://www.zynox.net/ensage/my_git", "git2-90befde.dll");
                            }
                            updateWorker.ReportProgress(40);
                            if (!System.IO.File.Exists("LibGit2Sharp.dll"))
                                webClient.DownloadFile("http://www.zynox.net/ensage/my_gitsharp", "LibGit2Sharp.dll");
                            updateWorker.ReportProgress(50);
                            if (!System.IO.File.Exists("NetIrc2.dll"))
                                webClient.DownloadFile("http://www.zynox.net/ensage/my_netirc", "NetIrc2.dll");
                            updateWorker.ReportProgress(55);
                            byte[] numArray = CheckMD5Version(stringBuilder.ToString());
                            if (numArray[0] != 48 && numArray[0] != 49 && (numArray[1] != 48 && numArray[1] != 49) && (numArray[2] != 48 && numArray[2] != 49))
                                throw new Exception("Can't get a valid server response");
                            if (numArray[0] == 48)
                            {
                                updateWorker.ReportProgress(51, "Updating!");
                                webClient.DownloadFile("http://www.zynox.net/ensage/my_ensagedll", "Ensage.dll");
                                DecryptFile("Ensage.dll");
                                updateWorker.ReportProgress(65);
                            }
                            if (numArray[2] == 48)
                            {
                                updateWorker.ReportProgress(71, "Updating!");
                                webClient.DownloadFile("http://www.zynox.net/ensage/my_loaderdll", "LoaderDLL.dll");
                                DecryptFile("LoaderDLL.dll");
                                updateWorker.ReportProgress(80);
                            }
                            if (numArray.Length >= 4 && numArray[3] == 48)
                            {
                                updateWorker.ReportProgress(81, "Updating!");
                                webClient.DownloadFile("http://www.zynox.net/ensage/my_ensagedll2", "EnsageV2.dll");
                                DecryptFile("EnsageV2.dll");
                                updateWorker.ReportProgress(85);
                            }

                            numArray[1] = 0; // AFFSD BEGIN AFFSD END
                            if (numArray[1] == 48)
                            {
                                updateWorker.ReportProgress(86, "Updating!");
                                webClient.DownloadFile("http://www.zynox.net/ensage/my_loader", "ELoader2.exe");
                                DecryptFile("ELoader2.exe");
                                updateWorker.ReportProgress(90, "Restarting loader");
                                Process.Start("ELoader2.exe", "-u");
                                updateWorker.ReportProgress(100, "exit");
                            }
                            else
                                updateWorker.ReportProgress(100, "Success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                updateWorker.ReportProgress(100, ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Updater));
            progressBar = new ProgressBar();
            statusLabel = new Label();
            updateWorker = new BackgroundWorker();
            closeTimer = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            progressBar.Location = new Point(12, 12);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(434, 23);
            progressBar.TabIndex = 0;
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(12, 38);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(52, 13);
            statusLabel.TabIndex = 1;
            statusLabel.Text = "Starting...";
            updateWorker.WorkerReportsProgress = true;
            updateWorker.DoWork += new DoWorkEventHandler(updateWorker_DoWork);
            updateWorker.ProgressChanged += new ProgressChangedEventHandler(updateWorker_ProgressChanged);
            closeTimer.Interval = 2500;
            closeTimer.Tick += new EventHandler(closeTimer_Tick);
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(458, 54);
            Controls.Add(statusLabel);
            Controls.Add(progressBar);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Updater";
            Text = "Ensage - Updater";
            Shown += new EventHandler(Update_Shown);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
