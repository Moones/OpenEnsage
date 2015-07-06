// Decompiled with JetBrains decompiler
// Type: Loader.Forms.Main
// Assembly: Loader, Version=0.1.5611.35443, Culture=neutral, PublicKeyToken=null
// MVID: 767D8978-23D8-4AB7-BA8A-78DBFB5F0780
// Assembly location: E:\Downloads\ensage\Dumps\Loader_fix.exe

using LibGit2Sharp;
using Loader.Properties;
using NetIrc2;
using NetIrc2.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Loader.Forms
{
    public class Main : Form
    {
        private readonly SharedRingBuffer _sharedMemoryOut = new SharedRingBuffer("ensCommandBuffer1", 4096);
        private readonly SharedRingBuffer _sharedMemoryIn = new SharedRingBuffer("ensCommandBuffer2", 4096);
        private readonly SharedRingBuffer _securityMemoryOutSteam = new SharedRingBuffer("EnSecData2", 1024);
        private readonly SharedRingBuffer _securityMemoryOutService = new SharedRingBuffer("Global\\EnSecData1", 1024);
        private readonly SharedRingBuffer _securityMemoryIn = new SharedRingBuffer("EnSecData3", 1024);
        private readonly CustomClass _scriptConfigClass = new CustomClass();
        private string _ircCurrentChannel = "#zynox";
        private Config _config;
        private bool _injecting;
        private uint _dotaLastPid;
        private bool _updateFound;
        private string _path;
        private readonly string _userPath;
        private bool _steamOnStart;
        private IrcClient _ircClient;
        private IdentServer _identServer;
        private string _ircNickname;
        private Main.SafetyInfo _safety;
        private int _scriptListChecksum;
        private IContainer components;
        private TabControl tabControl1;
        private TabPage mainPage;
        private TabPage scriptsPage;
        private DataGridView scriptsDataGrid;
        private TabPage configPage;
        private PropertyGrid configGrid;
        private BindingSource listBinding;
        private System.Windows.Forms.Timer dotaFindTimer;
        private NotifyIcon taskbarIcon;
        private Label label1;
        private Button reloadButton;
        private BackgroundWorker injectWorker;
        private Button startButton;
        private Label hackVersionLabel;
        private Label loaderVersionLabel;
        private BackgroundWorker pipeWorker;
        private BackgroundWorker serverWorker;
        private Label userGroupLabel;
        private GroupBox groupBox1;
        private TabPage helpPage;
        private LinkLabel changelogLink;
        private LinkLabel bugLink;
        private LinkLabel scriptsLink;
        private LinkLabel installLink;
        private Label newsBox;
        private System.Windows.Forms.Timer updateTimer;
        private TabPage repository;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private Button addRepo;
        private Button removeRepo;
        private TextBox repoName;
        private TreeView repoScripts;
        private BackgroundWorker loadWorker;
        private TabPage scriptConfigPage;
        private PropertyGrid scriptConfigGrid;
        private CheckBox newVersionCheck;
        private DataGridViewCheckBoxColumn LoadScript;
        private DataGridViewTextBoxColumn ScriptName;
        private DataGridViewTextBoxColumn ScriptDescription;
        private DataGridViewImageColumn State;
        private DataGridViewImageColumn ConfigColumn;
        private TabPage chatPage;
        private Label label2;
        private CheckBox autoEnterChatCheckBox;
        private Button enterChatButton;
        private Label label3;
        private ListBox statusBox;
        private TabControl ircTabControl;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TextBox ircChatText;
        private Button ircSendButton;
        private Button ircCloseButton;
        private ListBox ircUserList;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private SplitContainer splitContainer3;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label label4;
        private ComboBox ircLanguage;
        private RichTextBox ircMessageBox;
        private CheckBox DisableVAC;

        public Main()
        {
            InitializeComponent();
            installLink.Links.Add(0, -1, "http://wiki.zynox.net/Requirements");
            scriptsLink.Links.Add(0, -1, "http://www.zynox.net/forum/forums/4-Scripts");
            changelogLink.Links.Add(0, -1, "http://wiki.zynox.net/Changelog");
            bugLink.Links.Add(0, -1, "http://www.zynox.net/forum/forums/3-Bug-reports");
            Main.SetDoubleBuffered(ircUserList);
            _userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ensage");
            if (!Directory.Exists(_userPath))
                Directory.CreateDirectory(_userPath);
            if (!Directory.Exists("Scripts"))
            {
                try
                {
                    Directory.CreateDirectory("Scripts\\libs");
                    Directory.CreateDirectory("Scripts\\config");
                }
                catch (Exception ex)
                {
                    int num = (int)MessageBox.Show(Resources.ErrorNoScriptsDir + Environment.NewLine + ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
            }
            else
            {
                if (!Directory.Exists("Scripts\\libs"))
                {
                    try
                    {
                        Directory.CreateDirectory("Scripts\\libs");
                    }
                    catch (Exception ex)
                    {
                        int num = (int)MessageBox.Show(Resources.ErrorNoScriptsDir + Environment.NewLine + ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }
                }
                if (!Directory.Exists("Scripts\\config"))
                {
                    try
                    {
                        Directory.CreateDirectory("Scripts\\config");
                    }
                    catch (Exception ex)
                    {
                        int num = (int)MessageBox.Show(Resources.ErrorNoScriptsDir + Environment.NewLine + ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        return;
                    }
                }
            }
            scriptConfigGrid.SelectedObject = _scriptConfigClass;
            loadWorker.RunWorkerAsync();
            if (Program.SessionKey != null)
                serverWorker.RunWorkerAsync();
            pipeWorker.RunWorkerAsync();
            updateTimer.Start();
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            if (Program.IsContributor())
            {
                newVersionCheck.Visible = true;
                userGroupLabel.Text = "Private";
            }
            else
                userGroupLabel.Text = "Public";
            _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (_path == null)
            {
                int num = (int)MessageBox.Show("Can't get directory path.", Resources.Main_Main_Shown_Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Application.Exit();
            }
            else
            {
                _safety.ensagePath = new char[256];
                _path.ToCharArray().CopyTo(_safety.ensagePath, 0);
                try
                {
                    Config.Load(out _config);
                }
                catch (Exception)
                {
                    _config = new Config();
                }
                if (System.IO.File.Exists("script.list"))
                {
                    try
                    {
                        List<LuaScript> list;
                        using (FileStream fileStream = System.IO.File.Open("script.list", FileMode.Open))
                            list = (List<LuaScript>)new BinaryFormatter().Deserialize(fileStream);
                        foreach (object obj in list)
                            listBinding.Add(obj);
                    }
                    catch (Exception)
                    {
                        System.IO.File.Delete("script.list");
                    }
                }
                WinAPI.EnableDebugPrivileges();
                configGrid.SelectedObject = _config;
                if (_config.AutoConnectToChat)
                {
                    autoEnterChatCheckBox.Checked = true;
                    InitIrc();
                }
                _ircNickname = Program.AccountName;
                ircLanguage.SelectedItem = _config.ChatLanguage;
                dotaFindTimer.Start();
                loaderVersionLabel.Text += Assembly.GetExecutingAssembly().GetName().Version.ToString();
                try
                {
                    hackVersionLabel.Text += FileVersionInfo.GetVersionInfo("Ensage.dll").FileVersion;
                }
                catch (Exception)
                {
                    hackVersionLabel.Text += "UNKNOWN";
                }
            }
        }

        private void UpdateNews()
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    if (Program.IsContributor())
                        newsBox.Text = webClient.DownloadString("http://zynox.net/ensage/news2.php");
                    else
                        newsBox.Text = webClient.DownloadString("http://zynox.net/ensage/news.php");
                }
            }
            catch (Exception)
            {
                if (!(newsBox.Text == string.Empty))
                    return;
                newsBox.Text = "Can't download news content.";
            }
        }

        private void ClearBindingSource()
        {
            listBinding.Clear();
        }

        private void AddListBindingEntry(LuaScript entry)
        {
            listBinding.Add(entry);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                Task.Run(() =>
               {
                   string[] files1 = Directory.GetFiles("Scripts\\", "*.lua");
                   string[] files2 = Directory.GetFiles("Scripts\\libs", "*.lua");
                   int num = Enumerable.Aggregate<string, int>(Enumerable.Select<string, string>(files1, new Func<string, string>(Path.GetFileNameWithoutExtension)), 0, (current, scriptName) => current ^ scriptName.GetHashCode()) ^ Enumerable.Aggregate<string, int>(Enumerable.Select<string, string>(files2, new Func<string, string>(Path.GetFileNameWithoutExtension)), 0, (current, scriptName) => current ^ scriptName.GetHashCode());
                   if (num == _scriptListChecksum)
                       return;
                   _scriptListChecksum = num;
                   Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
                   foreach (LuaScript luaScript in listBinding)
                       dictionary[luaScript.ScriptName] = luaScript.LoadScript;
                   Invoke(new Action(ClearBindingSource));
                   foreach (string name in Enumerable.Select<string, string>(files1, new Func<string, string>(Path.GetFileNameWithoutExtension)))
                   {
                       LuaScript luaScript = new LuaScript(name);
                       if (dictionary.ContainsKey(luaScript.ScriptName))
                           luaScript.LoadScript = dictionary[luaScript.ScriptName];
                       Invoke(new Action<LuaScript>(AddListBindingEntry), luaScript);
                   }
               });
            }
            else
            {
                if (tabControl1.SelectedIndex != 3)
                    return;
                _scriptConfigClass.Clear();
                foreach (string category in Enumerable.Select<string, string>(Directory.GetFiles("Scripts\\Config\\", "*.txt"), new Func<string, string>(Path.GetFileNameWithoutExtension)))
                {
                    string[] strArray = System.IO.File.ReadAllLines(Path.Combine("Scripts\\Config\\", category + ".txt"));
                    Regex regex = new Regex("([\\w|-]*)[\\W]*=[\\W]*([\\w|-]*)", RegexOptions.Compiled);
                    foreach (string input in strArray)
                    {
                        if (regex.IsMatch(input))
                        {
                            Match match = regex.Match(input);
                            object obj = string.Compare(match.Groups[2].Value, "true", StringComparison.OrdinalIgnoreCase) != 0 ? (string.Compare(match.Groups[2].Value, "false", StringComparison.OrdinalIgnoreCase) != 0 ? match.Groups[2].Value : (object)false) : true;
                            _scriptConfigClass.Add(new CustomProperty(category, match.Groups[1].Value, obj, false, true));
                        }
                    }
                }
                scriptConfigGrid.Refresh();
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            taskbarIcon.Visible = false;
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (_config == null || !_config.SystemTray || WindowState != FormWindowState.Minimized)
                return;
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            taskbarIcon.Visible = true;
        }

        private bool IsDotaOpen()
        {
            return Process.GetProcessesByName("dota").Length != 0;
        }

        private bool IsSteamOpen()
        {
            return Process.GetProcessesByName("Steam").Length != 0;
        }

        private bool IsServiceOpen()
        {
            return Process.GetProcessesByName("SteamService").Length != 0;
        }

        private bool IsHackLoaded()
        {
            if ((int)_dotaLastPid != 0)
                return (int)_dotaLastPid == (int)WinAPI.GetPidByName("dota.exe");
            return false;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!IsDotaOpen() || !IsHackLoaded())
                return;
            if (MessageBox.Show(Resources.WarningClose, Resources.Warning, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            else
            {
                uint pidByName = WinAPI.GetPidByName("dota.exe");
                if ((int)pidByName == 0)
                    return;
                IntPtr hProcess = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.All, false, (int)pidByName);
                if (hProcess == IntPtr.Zero)
                    return;
                WinAPI.TerminateProcess(hProcess, 0U);
            }
        }

        private void serverWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int tickCount = Environment.TickCount;
            while (true)
            {
                do
                {
                    Thread.Sleep(133);
                }
                while (Environment.TickCount - tickCount < 20000);
                tickCount = Environment.TickCount;
                
                if (IsDotaOpen() && IsHackLoaded()) // AFFSD BEGIN
                {
                    _sharedMemoryOut.Write("tick");
                }
                continue; // AFFSD END

                /*
                try
                {
                    byte[] bytes = Encoding.ASCII.GetBytes("key=");
                    byte[] buffer = new byte[bytes.Length + Program.SessionKey.Length];
                    Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);
                    Buffer.BlockCopy(Program.SessionKey, 0, buffer, bytes.Length, Program.SessionKey.Length);
                    WebRequest webRequest = WebRequest.Create("http://www.zynox.net/ensage/ping.php");
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    webRequest.ContentLength = buffer.Length;
                    webRequest.Timeout = 5000;
                    using (Stream requestStream = webRequest.GetRequestStream())
                    {
                        requestStream.Write(buffer, 0, buffer.Length);
                        requestStream.Flush();
                    }
                    using (WebResponse response = webRequest.GetResponse())
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            if (responseStream == null)
                                break;
                            using (BinaryReader binaryReader = new BinaryReader(responseStream))
                            {
                                if (binaryReader.ReadByte() == 48)
                                {
                                    if (IsDotaOpen())
                                    {
                                        if (IsHackLoaded())
                                            _sharedMemoryOut.Write("tick");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
                */
            }
        }

        private void pipeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool flag = false;
            while (true)
            {
                while (_sharedMemoryIn.HasMsg())
                {
                    string[] strArray = _sharedMemoryIn.GetMsg().Split(' ');
                    if (strArray.Length == 2 && strArray[0] == "unload")
                    {
                        int length = strArray[1].IndexOf(".lua", StringComparison.Ordinal);
                        if (length != -1)
                            strArray[1] = strArray[1].Substring(0, length);
                        foreach (LuaScript luaScript in listBinding)
                        {
                            if (luaScript.ScriptName == strArray[1])
                            {
                                luaScript.LoadScript = false;
                                pipeWorker.ReportProgress(0);
                                break;
                            }
                        }
                    }
                }
                while (_securityMemoryIn.HasMsg())
                {
                    if (_securityMemoryIn.GetMsg() == "1")
                        flag = true;
                }
                if (flag && IsServiceOpen())
                {
                    flag = false;
                    int length = SecurityModule.Module.Length;
                    IntPtr num1 = Marshal.AllocHGlobal(length);
                    ToggleSecurityModuleEncryption();
                    Marshal.Copy(SecurityModule.Module, 0, num1, length);
                    if (!WinAPI.IsSecurityModServiceLoaded() && !WinAPI.LoadSecurityModService(out _safety.serviceHandle, out _safety.serviceSize, num1, length))
                    {
                        int num2 = (int)MessageBox.Show("Security Error!", "Error");
                    }
                    Marshal.FreeHGlobal(num1);
                    ToggleSecurityModuleEncryption();
                }
                Thread.Sleep(25);
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (_injecting || injectWorker.IsBusy)
                return;
            Process.Start("steam://rungameid/570", "-console -novid");
            injectWorker.RunWorkerAsync();
            _injecting = true;
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            _sharedMemoryOut.Write("reload-scripts");
        }

        private void injectWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            _injecting = false;
        }

        private void injectWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            _sharedMemoryIn.Reset();
            _sharedMemoryOut.Reset();
            _securityMemoryIn.Reset();
            foreach (LuaScript luaScript in listBinding)
            {
                if (luaScript.LoadScript)
                    _sharedMemoryOut.Write("load " + luaScript.ScriptName + ".lua");
            }
            _sharedMemoryOut.Write("group " + Program.UserGroup);
            while (stopwatch.ElapsedMilliseconds <= 15000L)
            {
                uint pidByName = WinAPI.GetPidByName("dota.exe");
                Thread.Sleep(125);
                if ((int)pidByName != 0)
                {
                    if ((int)pidByName == (int)_dotaLastPid)
                    {
                        _injecting = false;
                        return;
                    }
                    _dotaLastPid = pidByName;
                    if (!DisableVAC.Checked)
                    {
                        try
                        {
                            int length = SecurityModule.Module.Length;
                            IntPtr num = Marshal.AllocHGlobal(length);
                            ToggleSecurityModuleEncryption();
                            Marshal.Copy(SecurityModule.Module, 0, num, length);
                            bool flag = true;
                            if (IsServiceOpen() && !WinAPI.IsSecurityModServiceLoaded())
                            {
                                flag &= WinAPI.LoadSecurityModService(out _safety.serviceHandle, out _safety.serviceSize, num, length);
                                Marshal.FreeHGlobal(num);
                                num = IntPtr.Zero;
                            }
                            if (!WinAPI.IsSecurityModSteamLoaded())
                            {
                                if (num == IntPtr.Zero)
                                {
                                    num = Marshal.AllocHGlobal(length);
                                    Marshal.Copy(SecurityModule.Module, 0, num, length);
                                }
                                flag &= WinAPI.LoadSecurityModSteam(out _safety.steamHandle, out _safety.steamSize, num, length);
                            }
                            Marshal.FreeHGlobal(num);
                            ToggleSecurityModuleEncryption();
                            if (!flag)
                                throw new Exception("Injection failed.");
                        }
                        catch (Exception ex)
                        {
                            int num1 = (int)MessageBox.Show("Security error!\n" + ex.Message);
                            IntPtr num2 = WinAPI.OpenProcess(WinAPI.ProcessAccessFlags.All, false, (int)pidByName);
                            if (num2 == IntPtr.Zero)
                                return;
                            WinAPI.TerminateProcess(num2, 0U);
                            WinAPI.CloseHandle(num2);
                            return;
                        }
                    }
                    if (!(!Program.IsContributor() || !newVersionCheck.Checked ? WinAPI.LoadEnsage(pidByName, out _safety.ensageHandle, out _safety.ensageSize) : WinAPI.LoadEnsage2(pidByName, out _safety.ensageHandle, out _safety.ensageSize)) || (int)_safety.ensageHandle == 0 || (int)_safety.ensageSize == 0)
                    {
                        _injecting = false;
                        int num = (int)MessageBox.Show("Ensage injection error!");
                        return;
                    }
                    int length1 = Marshal.SizeOf(_safety);
                    byte[] numArray = new byte[length1];
                    IntPtr num3 = Marshal.AllocHGlobal(length1);
                    Marshal.StructureToPtr(_safety, num3, true);
                    Marshal.Copy(num3, numArray, 0, length1);
                    Marshal.FreeHGlobal(num3);
                    _securityMemoryOutService.Reset();
                    _securityMemoryOutService.Write(numArray);
                    _securityMemoryOutSteam.Reset();
                    _securityMemoryOutSteam.Write(numArray);
                    _injecting = false;
                    return;
                }
            }
            _injecting = false;
            int num4 = (int)MessageBox.Show("Couldn't find Dota2 after launching it!", Resources.Main_Main_Shown_Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void dotaFindTimer_Tick(object sender, EventArgs e)
        {
            bool flag1 = IsDotaOpen();
            bool flag2 = IsHackLoaded();
            bool flag3 = IsSteamOpen();
            if (_steamOnStart && !flag3)
                _steamOnStart = false;
            reloadButton.Enabled = flag1 && flag2;
            startButton.Enabled = !_injecting && !flag1 && (!flag2 && flag3) && !_steamOnStart;
            if (_steamOnStart)
                label1.Text = "Restart Steam first";
            else if (flag1)
                label1.Text = flag2 ? "Status: Hack already injected" : "Status: Dota 2 is running";
            else
                label1.Text = !flag3 ? "Status: Start Steam first" : Resources.StatusNotStartedYet;
        }

        private void scriptsDataGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            scriptsDataGrid.EndEdit();
            if (e.ColumnIndex != 0)
                return;
            bool flag = (bool)scriptsDataGrid.Rows[e.RowIndex].Cells[0].Value;
            string str = (string)scriptsDataGrid.Rows[e.RowIndex].Cells[1].Value;
            if (flag)
                _sharedMemoryOut.Write("load " + str + ".lua");
            else
                _sharedMemoryOut.Write("unload " + str + ".lua");
            List<LuaScript> list = Enumerable.ToList<LuaScript>(Enumerable.Cast<LuaScript>(listBinding));
            using (FileStream fileStream = System.IO.File.Open("script.list", FileMode.Create))
                new BinaryFormatter().Serialize(fileStream, list);
        }

        private void configGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            try
            {
                Config.Save(_config);
                _sharedMemoryOut.Write("config-reloaded");
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(Resources.ErrorOnConfigSave + ex.Message);
            }
        }

        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string fileName = e.Link.LinkData as string;
            if (fileName == null)
                return;
            Process.Start(fileName);
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            UpdateNews();
            if (_updateFound)
                return;
            if (Program.VersionString == string.Empty)
                return;
            try
            {
                byte[] numArray = CheckMD5Version(Program.VersionString);
                if (numArray[0] != 48 && numArray[0] != 49 && (numArray[1] != 48 && numArray[1] != 49) && (numArray[2] != 48 && numArray[2] != 49 && (numArray.Length >= 4 && numArray[3] != 48)) && numArray[3] != 49)
                    throw new Exception("Can't get a valid server response");
                if (numArray[0] != 48 && numArray[1] != 48 && numArray[2] != 48 && (numArray.Length < 4 || numArray[3] != 48))
                    return;
                _updateFound = true;
                int num;
                new Thread(() => num = (int)MessageBox.Show("Update available.\nPlease restart to download the update!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Asterisk)).Start();
            }
            catch (Exception)
            {
            }
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

        private void pipeWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            scriptsDataGrid.Refresh();
        }

        private void repoName_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
                return;
            addRepo.Enabled = textBox.Text != string.Empty;
        }

        private void DeleteDirectory(string target)
        {
            string[] files = Directory.GetFiles(target);
            string[] directories = Directory.GetDirectories(target);
            foreach (string path in files)
            {
                System.IO.File.SetAttributes(path, FileAttributes.Normal);
                System.IO.File.Delete(path);
            }
            foreach (string target1 in directories)
                DeleteDirectory(target1);
            Directory.Delete(target, false);
        }

        private void removeRepo_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = repoScripts.SelectedNode;
            if (selectedNode == null)
                return;
            foreach (TreeNode treeNode1 in selectedNode.Nodes)
            {
                foreach (TreeNode treeNode2 in treeNode1.Nodes)
                {
                    string path = treeNode1.Text == "Libraries" ? Path.Combine("Scripts", "libs", treeNode2.Text) : Path.Combine("Scripts", treeNode2.Text);
                    try
                    {
                        if (System.IO.File.Exists(path))
                            System.IO.File.Delete(path);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            string str = Path.Combine(_userPath, selectedNode.Text);
            try
            {
                if (Directory.Exists(str))
                    DeleteDirectory(str);
                string path = str.Substring(0, str.LastIndexOf("\\", StringComparison.Ordinal));
                if (Directory.GetDirectories(path).Length == 0)
                    Directory.Delete(path);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
            }
            repoScripts.Nodes.RemoveAt(repoScripts.SelectedNode.Index);
        }

        private void addRepo_Click(object sender, EventArgs e)
        {
            string text = repoName.Text;
            if (text == string.Empty)
                return;
            try
            {
                Loader.Repository repository = new Loader.Repository(text, _userPath);
                TreeNode treeNode1 = repoScripts.Nodes.Add(repository.Name, repository.Name);
                if (repository.Scripts.Count > 0)
                {
                    TreeNode treeNode2 = treeNode1.Nodes.Add("Scripts", "Scripts");
                    foreach (LuaScript luaScript in repository.Scripts)
                        treeNode2.Nodes.Add(luaScript.ToString());
                }
                if (repository.Libs.Count > 0)
                {
                    TreeNode treeNode2 = treeNode1.Nodes.Add("Libraries", "Libraries");
                    foreach (LuaScript luaScript in repository.Libs)
                        treeNode2.Nodes.Add(luaScript.ToString());
                }
            }
            catch (RepositoryNotFoundException)
            {
            }
            repoName.Text = string.Empty;
        }

        private bool IsScriptNode(TreeNode node)
        {
            if (node.Nodes.Count != 0)
                return false;
            newsBox.Text = node.Level.ToString();
            if (node.Level < 2)
                return false;
            TreeNode parent = node.Parent;
            if (parent == null)
                return false;
            TreeNode grandparent = parent.Parent;
            if (grandparent == null)
                return false;
            return true;
        }

        private bool DeleteScript(TreeNode node)
        {
            if (!IsScriptNode(node))
                return false;
            try
            {
                TreeNode parent = node.Parent, grandparent = parent.Parent;
                string path = parent.Text == "Scripts" ? Path.Combine("Scripts", node.Text) : Path.Combine("Scripts", "libs", node.Text);
                if (!System.IO.File.Exists(path))
                    return false;
                System.IO.File.Delete(path);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        private bool AddScript(TreeNode node)
        {
            if (!IsScriptNode(node))
                return false;
            try
            {
                TreeNode parent = node.Parent, grandparent = parent.Parent;
                string destFileName = parent.Text == "Scripts" ? Path.Combine("Scripts", node.Text) : Path.Combine("Scripts", "libs", node.Text);
                string str = Path.Combine(Path.Combine(_userPath, grandparent.Text), parent.Text == "Scripts" ? "Scripts" : "Libraries", node.Text);
                if (!System.IO.File.Exists(str))
                    return false;
                System.IO.File.Copy(str, destFileName, true);
            }
            catch (System.IO.IOException)
            {
                return false;
            }
            return true;
        }

        private void UpdateParentNodes(TreeNode node)
        {
            TreeNode parent = node.Parent;
            if (parent != null)
            {
                parent.Checked = Enumerable.Any(Enumerable.Cast<TreeNode>(parent.Nodes), child => child.Checked);
                if (!parent.Checked)
                    parent.Collapse();
                UpdateParentNodes(parent);
            }
        }

        private void UpdateChildNodes(TreeNode node, bool state)
        {
            if (state)
            {
                AddScript(node);
                node.ExpandAll();
            }
            else
            {
                DeleteScript(node);
                node.Collapse();
            }
            foreach (TreeNode child in node.Nodes) // propogate down
            {
                child.Checked = state;
                UpdateChildNodes(child, state);
            }
        }

        private void repoScripts_AfterCheck(object sender, TreeViewEventArgs e)
        {
            repoScripts.AfterCheck -= new TreeViewEventHandler(repoScripts_AfterCheck);
            UpdateParentNodes(e.Node);
            UpdateChildNodes(e.Node, e.Node.Checked);
            repoScripts.AfterCheck += new TreeViewEventHandler(repoScripts_AfterCheck);
        }

        private void repoScripts_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            removeRepo.Enabled = e.Node.Parent == null;
        }

        private void loadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (System.IO.File.Exists("ensage.log"))
                {
                    string[] strArray = System.IO.File.ReadAllLines("ensage.log");
                    int count = strArray.Length - 1000;
                    if (count > 0)
                        System.IO.File.WriteAllLines("ensage.log", Enumerable.Skip<string>(strArray, count));
                }
            }
            catch (Exception)
            {
            }
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    if (Program.IsContributor())
                        newsBox.Text = webClient.DownloadString("http://zynox.net/ensage/news2.php");
                    else
                        loadWorker.ReportProgress(1, webClient.DownloadString("http://zynox.net/ensage/news.php"));
                }
            }
            catch (Exception)
            {
                if (newsBox.Text == string.Empty)
                    loadWorker.ReportProgress(1, "Can't download news content.");
            }
            if (!Directory.Exists(_userPath))
                return;
            string[] directories = Directory.GetDirectories(_userPath);
            if (directories.Length == 0)
            {
                loadWorker.ReportProgress(9);
            }
            else
            {
                foreach (string path1 in directories)
                {
                    foreach (string path2 in Directory.GetDirectories(path1))
                    {
                        try
                        {
                            LibGit2Sharp.Signature merger = new LibGit2Sharp.Signature("local", "localhost", new DateTimeOffset());
                            using (LibGit2Sharp.Repository repository = new LibGit2Sharp.Repository(path2, null))
                            {
                                MergeOptions mergeOptions = new MergeOptions()
                                {
                                    MergeFileFavor = MergeFileFavor.Theirs
                                };
                                repository.Network.Pull(merger, new PullOptions()
                                {
                                    MergeOptions = mergeOptions
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            int num = (int)MessageBox.Show(ex.Message);
                            continue;
                        }
                        string str1 = path2.Substring(_userPath.Length + 1);
                        loadWorker.ReportProgress(2, str1);
                        foreach (string path3 in Directory.GetDirectories(path2))
                        {
                            string str2 = path3.Substring(path3.LastIndexOf('\\') + 1);
                            bool flag;
                            if (str2 == "Libraries")
                            {
                                flag = true;
                                loadWorker.ReportProgress(3, str1);
                            }
                            else if (str2 == "Scripts")
                            {
                                flag = false;
                                loadWorker.ReportProgress(4, str1);
                            }
                            else
                                continue;
                            foreach (string str3 in Directory.GetFiles(path3))
                            {
                                string str4 = str3.Substring(str3.LastIndexOf('\\') + 1);
                                string path4 = flag ? Path.Combine("Scripts", "libs", str4) : Path.Combine("Scripts", str4);
                                loadWorker.ReportProgress(flag ? 5 : 6, new string[2]
                                {
                  str1,
                  str4
                                });
                                if (System.IO.File.Exists(path4))
                                    loadWorker.ReportProgress(flag ? 7 : 8, new string[2]
                                    {
                    str1,
                    str4
                                    });
                            }
                        }
                    }
                }
            }
        }

        private void loadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 1:
                    newsBox.Text = e.UserState as string;
                    break;
                case 2:
                    string str = e.UserState as string;
                    if (str == null)
                        break;
                    repoScripts.Nodes.Add(str, str);
                    break;
                case 3:
                    string index = e.UserState as string;
                    if (index == null)
                        break;
                    repoScripts.Nodes[index].Nodes.Add("Libraries", "Libraries");
                    break;
                case 4:
                    if (!(e.UserState is string))
                        break;
                    repoScripts.Nodes[e.UserState as string].Nodes.Add("Scripts", "Scripts");
                    break;
                case 5:
                    string[] strArray1 = e.UserState as string[];
                    if (strArray1 == null)
                        break;
                    repoScripts.Nodes[strArray1[0]].Nodes["Libraries"].Nodes.Add(strArray1[1], strArray1[1]);
                    break;
                case 6:
                    string[] strArray2 = e.UserState as string[];
                    if (strArray2 == null)
                        break;
                    repoScripts.Nodes[strArray2[0]].Nodes["Scripts"].Nodes.Add(strArray2[1], strArray2[1]);
                    break;
                case 7:
                    string[] strArray3 = e.UserState as string[];
                    if (strArray3 == null)
                        break;
                    repoScripts.Nodes[strArray3[0]].Nodes["Libraries"].Nodes[strArray3[1]].Checked = true;
                    break;
                case 8:
                    string[] strArray4 = e.UserState as string[];
                    if (strArray4 == null)
                        break;
                    repoScripts.Nodes[strArray4[0]].Nodes["Scripts"].Nodes[strArray4[1]].Checked = true;
                    break;
                case 9:
                    repoName.Text = "https://github.com/Rulfy/ensage-scripts.git";
                    addRepo_Click(addRepo, new EventArgs());
                    break;
            }
        }

        private void scriptsDataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (scriptsDataGrid.Columns[e.ColumnIndex].Name == "State")
            {
                DataGridViewCell dataGridViewCell = scriptsDataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                LuaScript luaScript = (LuaScript)listBinding[e.RowIndex];
                switch (luaScript.State)
                {
                    case LuaScript.RequirementState.StateOkay:
                        e.Value = new Bitmap(Resources.okay);
                        break;
                    case LuaScript.RequirementState.StateError:
                        e.Value = new Bitmap(Resources.exclamation);
                        StringBuilder stringBuilder = new StringBuilder();
                        foreach (KeyValuePair<string, LuaScript.RequirementState> keyValuePair in luaScript.Requirements)
                        {
                            if (keyValuePair.Value == LuaScript.RequirementState.StateError)
                            {
                                if (stringBuilder.Length != 0)
                                    stringBuilder.Append(", ");
                                stringBuilder.Append(keyValuePair.Key);
                            }
                        }
                        dataGridViewCell.ToolTipText = "Missing: " + stringBuilder;
                        break;
                    default:
                        e.Value = new Bitmap(Resources.question);
                        break;
                }
            }
            else if (scriptsDataGrid.Columns[e.ColumnIndex].Name == "ConfigColumn")
            {
                LuaScript luaScript = (LuaScript)listBinding[e.RowIndex];
                e.Value = luaScript.HasConfig ? new Bitmap(Resources.good) : new Bitmap(Resources.bad);
            }
            else
            {
                if (e.ColumnIndex != 0)
                    return;
                LuaScript luaScript = (LuaScript)listBinding[e.RowIndex];
                DataGridViewCheckBoxCell viewCheckBoxCell = scriptsDataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;
                if (luaScript.State != LuaScript.RequirementState.StateOkay)
                {
                    if (viewCheckBoxCell.ReadOnly)
                        return;
                    bool flag = (bool)viewCheckBoxCell.Value;
                    viewCheckBoxCell.Value = false;
                    viewCheckBoxCell.ReadOnly = true;
                    viewCheckBoxCell.FlatStyle = FlatStyle.Flat;
                    viewCheckBoxCell.Style.ForeColor = Color.DarkGray;
                    if (!flag)
                        return;
                    List<LuaScript> list = Enumerable.ToList<LuaScript>(Enumerable.Cast<LuaScript>(listBinding));
                    using (FileStream fileStream = System.IO.File.Open("script.list", FileMode.Create))
                        new BinaryFormatter().Serialize(fileStream, list);
                }
                else
                {
                    viewCheckBoxCell.ReadOnly = false;
                    viewCheckBoxCell.FlatStyle = FlatStyle.Standard;
                    viewCheckBoxCell.Style.ForeColor = Color.White;
                }
            }
        }

        private void ToggleSecurityModuleEncryption()
        {
            byte num1 = 39;
            for (int index = 0; index < SecurityModule.Module.Length; ++index)
            {
                byte num2 = (byte)(SecurityModule.Module[index] ^ (uint)num1);
                SecurityModule.Module[index] = num2;
                num1 = (byte)(num1 + 1 & byte.MaxValue);
            }
        }

        private void scriptConfigGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            string label1 = e.ChangedItem.Parent.Label;
            string label2 = e.ChangedItem.Label;
            object obj = e.ChangedItem.Value;
            try
            {
                string path = Path.Combine("Scripts\\Config\\", label1 + ".txt");
                string[] contents = System.IO.File.ReadAllLines(path);
                Regex regex = new Regex("([\\w|-]*)[\\W]*=[\\W]*([\\w|-]*)", RegexOptions.Compiled);
                for (int index1 = 0; index1 < contents.Length; ++index1)
                {
                    string input = contents[index1];
                    if (regex.IsMatch(input))
                    {
                        Match match = regex.Match(input);
                        if (match.Groups[1].Value == label2 && string.Compare(match.Groups[2].Value, e.OldValue.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            contents[index1] = label2 + " = ";
                            if (obj is bool) // AFFSD BEGIN
                            {
                                contents[index1] = contents[index1] + obj.ToString();
                            }
                            else
                            {
                                contents[index1] = contents[index1] + obj.ToString();
                            } // AFFSD END
                            System.IO.File.WriteAllLines(path, contents);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
            }
        }

        private void autoEnterChatCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _config.AutoConnectToChat = (sender as CheckBox).Checked;
            Config.Save(_config);
        }

        private void enterChatButton_Click(object send, EventArgs e)
        {
            InitIrc();
        }

        private bool ValidateSSL(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void InitIrc()
        {
            statusBox.Items.Clear();
            if (_ircClient == null)
            {
                _ircClient = new IrcClient();
                _ircClient.Connected += new EventHandler(_ircClient_Connected);
                _ircClient.Closed += new EventHandler(_ircClient_Closed);
                _ircClient.GotJoinChannel += new EventHandler<JoinLeaveEventArgs>(_ircClient_GotJoinChannel);
                _ircClient.GotNotice += new EventHandler<ChatMessageEventArgs>(_ircClient_GotNotice);
                _ircClient.GotMessage += new EventHandler<ChatMessageEventArgs>(_ircClient_GotMessage);
                _ircClient.GotIrcError += new EventHandler<IrcErrorEventArgs>(_ircClient_GotIrcError);
                _ircClient.GotMotdEnd += new EventHandler(_ircClient_GotMotdEnd);
                _ircClient.GotNameListReply += new EventHandler<NameListReplyEventArgs>(_ircClient_GotNameListReply);
                _ircClient.GotLeaveChannel += new EventHandler<JoinLeaveEventArgs>(_ircClient_GotLeaveChannel);
                _ircClient.GotUserKicked += new EventHandler<KickEventArgs>(_ircClient_GotUserKicked);
            }
            if (_identServer == null)
            {
                try
                {
                    _identServer = new IdentServer()
                    {
                        UserID = _ircNickname
                    };
                    _identServer.Start(113);
                }
                catch (Exception)
                {
                    statusBox.Items.Add("Couldn't start the ident server, this might be because you are already running another client");
                }
            }
            if (_ircClient.IsConnected)
                return;
            try
            {
                enterChatButton.Enabled = false;
                statusBox.Items.Add("Connecting...");
                _ircClient.Connect("irc.rizon.net", 6697, new IrcClientConnectionOptions()
                {
                    Ssl = true,
                    SslHostname = "irc.rizon.net",
                    SslCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateSSL)
                });
            }
            catch (Exception ex)
            {
                enterChatButton.Enabled = true;
                statusBox.Items.Add("Connect error: " + ex.Message);
            }
        }

        private void _ircClient_GotUserKicked(object sender, KickEventArgs e)
        {
        }

        private void _ircClient_GotLeaveChannel(object sender, JoinLeaveEventArgs e)
        {
            ircUserList.Items.Remove(e.Identity.Nickname);
        }

        private void _ircClient_GotNameListReply(object sender, NameListReplyEventArgs e)
        {
            foreach (IrcString ircString in e.GetNameList())
                ircUserList.Items.Add(Encoding.UTF8.GetString(ircString));
        }

        private void _ircClient_GotMotdEnd(object sender, EventArgs e)
        {
            statusBox.Items.Add("Joining channel");
            switch (ircLanguage.Text)
            {
                case "English":
                    _ircCurrentChannel = "#zynox";
                    break;
                case "Russian":
                    _ircCurrentChannel = "#zynox-ru";
                    break;
                case "German":
                    _ircCurrentChannel = "#zynox-de";
                    break;
            }
            _config.ChatLanguage = ircLanguage.Text;
            Config.Save(_config);
            ircUserList.Items.Clear();
            _ircClient.Join(_ircCurrentChannel, null);
        }

        private void _ircClient_GotIrcError(object sender, IrcErrorEventArgs e)
        {
            statusBox.Items.Add("IRC error: " + e.Error);
            if (e.Data.ReplyCode != IrcReplyCode.NicknameInUse)
                return;
            _ircNickname = Program.AccountName + "_" + (new Random().Next(0, 1000)).ToString(); // AFFSD BEGIN AFFSD END
            statusBox.Items.Add("Retrying with a new nickname \"" + _ircNickname + "\" ...");
            _ircClient.ChangeName(_ircNickname);
        }

        private void _ircClient_GotMessage(object sender, ChatMessageEventArgs e)
        {
            string string1 = Encoding.UTF8.GetString(e.Sender.Nickname);
            Color color = Color.Blue;
            foreach (string str in ircUserList.Items)
            {
                if (!(str.Substring(1) != string1))
                {
                    switch (Enumerable.ElementAt(str, 0))
                    {
                        case '%':
                            color = Color.DodgerBlue;
                            goto label_11;
                        case '+':
                            color = Color.ForestGreen;
                            goto label_11;
                        case '@':
                            color = Color.Red;
                            goto label_11;
                        default:
                            goto label_11;
                    }
                }
            }
            label_11:
            RichTextBoxExtensions.AppendText(ircMessageBox, Environment.NewLine + e.Sender.Nickname + ": ", color);
            string string2 = Encoding.UTF8.GetString(e.Message);
            RichTextBoxExtensions.AppendText(ircMessageBox, string2, string2.ToUpper().Contains(_ircNickname.ToUpper()) ? Color.Red : Color.Black);
            RichTextBoxExtensions.ScrollToEnd(ircMessageBox);
        }

        private void _ircClient_GotJoinChannel(object sender, JoinLeaveEventArgs e)
        {
            if (ircTabControl.SelectedTab.TabIndex != 1)
            {
                statusBox.Items.Add("Channel joined");
                ircMessageBox.AppendText("Welcome to the chat.");
                ircTabControl.SelectTab(1);
            }
            else
                ircUserList.Items.Add(Encoding.UTF8.GetString(e.Identity.Nickname));
        }

        private void _ircClient_Closed(object sender, EventArgs e)
        {
            enterChatButton.Enabled = true;
            statusBox.Items.Add("Disconnected");
            if (!_ircClient.IsConnected)
                return;
            _ircClient.Close();
        }

        private void _ircClient_Connected(object sender, EventArgs e)
        {
            statusBox.Items.Add("Connected to server");
            _ircClient.LogIn(_ircNickname, _ircNickname, _ircNickname, "ensageLoader1", "ensageLoader2", null);
        }

        private void _ircClient_GotNotice(object sender, ChatMessageEventArgs e)
        {
            statusBox.Items.Add("Notice: " + e.Message);
            if (!(e.Message == "please choose a different nick."))
                return;
            ircUserList.Items.Remove(_ircNickname);
            _ircNickname = Program.AccountName + "_" + (new Random().Next(0, 1000)).ToString(); // AFFSD BEGIN AFFSD END
            statusBox.Items.Add("Retrying with a new nickname \"" + _ircNickname + "\" ...");
            _ircClient.ChangeName(_ircNickname);
            ircUserList.Items.Add(_ircNickname);
        }

        private void ircCloseButton_Click(object sender, EventArgs e)
        {
            if (_ircClient.IsConnected)
                _ircClient.Close();
            ircTabControl.SelectTab(0);
        }

        private void ircSendButton_Click(object sender, EventArgs e)
        {
            if (!_ircClient.IsConnected)
                return;
            _ircClient.Message(_ircCurrentChannel, ircChatText.Text);
            RichTextBoxExtensions.AppendText(ircMessageBox, Environment.NewLine + _ircNickname + ": ", Color.Orchid);
            RichTextBoxExtensions.AppendText(ircMessageBox, ircChatText.Text, Color.Black);
            RichTextBoxExtensions.ScrollToEnd(ircMessageBox);
            ircChatText.Text = string.Empty;
        }

        private void ircChatText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                ircSendButton_Click(null, null);
                e.Handled = true;
            }
            else
            {
                if (e.KeyChar != 9)
                    return;
                int num = ircChatText.Text.LastIndexOf(' ');
                string str1 = ircChatText.Text.Substring(num + 1);
                if (str1 != string.Empty)
                {
                    foreach (object obj in ircUserList.Items)
                    {
                        string str2 = obj as string;
                        if (Enumerable.ElementAt(str2, 0) == 37 || Enumerable.ElementAt(str2, 0) == 43 || Enumerable.ElementAt(str2, 0) == 64)
                            str2 = str2.Substring(1);
                        if (str2.StartsWith(str1, StringComparison.CurrentCultureIgnoreCase) && !(str2 == _ircNickname))
                        {
                            if (num == -1)
                                ircChatText.Text = str2 + ": ";
                            else
                                ircChatText.Text = ircChatText.Text.Substring(0, num + 1) + str2;
                            ircChatText.Select(ircChatText.TextLength, 0);
                            break;
                        }
                    }
                }
                e.Handled = true;
            }
        }

        private void flowLayoutPanel1_Resize(object sender, EventArgs e)
        {
            ircChatText.Width = flowLayoutPanel1.Width - ircSendButton.Width - ircCloseButton.Width - 20;
        }

        private void ircChatText_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control || e.KeyCode != Keys.A)
                return;
            ircChatText.Select(0, ircChatText.TextLength);
            e.SuppressKeyPress = true;
        }

        private void ircUserList_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            string s = ircUserList.Items[e.Index] as string;
            if (e.Index == ircUserList.SelectedIndex)
                e.Graphics.DrawString(s, e.Font, Brushes.White, e.Bounds);
            else if (s == _ircNickname)
            {
                e.Graphics.DrawString(s, e.Font, Brushes.Orchid, e.Bounds);
            }
            else
            {
                switch (Enumerable.ElementAt(s, 0))
                {
                    case '%':
                        e.Graphics.DrawString(s, e.Font, Brushes.DodgerBlue, e.Bounds);
                        break;
                    case '+':
                        e.Graphics.DrawString(s, e.Font, Brushes.ForestGreen, e.Bounds);
                        break;
                    case '@':
                        e.Graphics.DrawString(s, e.Font, Brushes.Red, e.Bounds);
                        break;
                    default:
                        e.Graphics.DrawString(s, e.Font, Brushes.Black, e.Bounds);
                        break;
                }
            }
        }

        private void ircUserList_Resize(object sender, EventArgs e)
        {
            ircUserList.Invalidate();
        }

        private void ircUserList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ircUserList.Invalidate();
        }

        private static void SetDoubleBuffered(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(control, true, null);
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
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Main));
            tabControl1 = new TabControl();
            mainPage = new TabPage();
            newVersionCheck = new CheckBox();
            groupBox1 = new GroupBox();
            newsBox = new Label();
            userGroupLabel = new Label();
            hackVersionLabel = new Label();
            loaderVersionLabel = new Label();
            startButton = new Button();
            reloadButton = new Button();
            label1 = new Label();
            chatPage = new TabPage();
            ircTabControl = new TabControl();
            tabPage1 = new TabPage();
            splitContainer1 = new SplitContainer();
            label4 = new Label();
            ircLanguage = new ComboBox();
            label2 = new Label();
            label3 = new Label();
            enterChatButton = new Button();
            autoEnterChatCheckBox = new CheckBox();
            statusBox = new ListBox();
            tabPage2 = new TabPage();
            splitContainer2 = new SplitContainer();
            splitContainer3 = new SplitContainer();
            ircMessageBox = new RichTextBox();
            ircUserList = new ListBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            ircSendButton = new Button();
            ircChatText = new TextBox();
            ircCloseButton = new Button();
            scriptsPage = new TabPage();
            scriptsDataGrid = new DataGridView();
            LoadScript = new DataGridViewCheckBoxColumn();
            ScriptName = new DataGridViewTextBoxColumn();
            ScriptDescription = new DataGridViewTextBoxColumn();
            State = new DataGridViewImageColumn();
            ConfigColumn = new DataGridViewImageColumn();
            listBinding = new BindingSource(components);
            scriptConfigPage = new TabPage();
            scriptConfigGrid = new PropertyGrid();
            repository = new TabPage();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            repoName = new TextBox();
            addRepo = new Button();
            removeRepo = new Button();
            repoScripts = new TreeView();
            configPage = new TabPage();
            configGrid = new PropertyGrid();
            helpPage = new TabPage();
            changelogLink = new LinkLabel();
            bugLink = new LinkLabel();
            scriptsLink = new LinkLabel();
            installLink = new LinkLabel();
            dotaFindTimer = new System.Windows.Forms.Timer(components);
            taskbarIcon = new NotifyIcon(components);
            injectWorker = new BackgroundWorker();
            pipeWorker = new BackgroundWorker();
            serverWorker = new BackgroundWorker();
            updateTimer = new System.Windows.Forms.Timer(components);
            loadWorker = new BackgroundWorker();
            DisableVAC = new CheckBox();
            tabControl1.SuspendLayout();
            mainPage.SuspendLayout();
            groupBox1.SuspendLayout();
            chatPage.SuspendLayout();
            ircTabControl.SuspendLayout();
            tabPage1.SuspendLayout();
            splitContainer1.BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabPage2.SuspendLayout();
            splitContainer2.BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            splitContainer3.BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            scriptsPage.SuspendLayout();
            ((ISupportInitialize)scriptsDataGrid).BeginInit();
            ((ISupportInitialize)listBinding).BeginInit();
            scriptConfigPage.SuspendLayout();
            repository.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            configPage.SuspendLayout();
            helpPage.SuspendLayout();
            SuspendLayout();
            tabControl1.Controls.Add(mainPage);
            tabControl1.Controls.Add(chatPage);
            tabControl1.Controls.Add(scriptsPage);
            tabControl1.Controls.Add(scriptConfigPage);
            tabControl1.Controls.Add(repository);
            tabControl1.Controls.Add(configPage);
            tabControl1.Controls.Add(helpPage);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(683, 428);
            tabControl1.TabIndex = 0;
            tabControl1.SelectedIndexChanged += new EventHandler(tabControl1_SelectedIndexChanged);
            mainPage.Controls.Add(DisableVAC);
            mainPage.Controls.Add(newVersionCheck);
            mainPage.Controls.Add(groupBox1);
            mainPage.Controls.Add(userGroupLabel);
            mainPage.Controls.Add(hackVersionLabel);
            mainPage.Controls.Add(loaderVersionLabel);
            mainPage.Controls.Add(startButton);
            mainPage.Controls.Add(reloadButton);
            mainPage.Controls.Add(label1);
            mainPage.Location = new Point(4, 22);
            mainPage.Name = "mainPage";
            mainPage.Padding = new Padding(3);
            mainPage.Size = new Size(675, 402);
            mainPage.TabIndex = 0;
            mainPage.Text = "Main";
            mainPage.UseVisualStyleBackColor = true;
            newVersionCheck.AutoSize = true;
            newVersionCheck.Location = new Point(9, 94);
            newVersionCheck.Name = "newVersionCheck";
            newVersionCheck.Size = new Size(105, 17);
            newVersionCheck.TabIndex = 8;
            newVersionCheck.Text = "Use new version";
            newVersionCheck.UseVisualStyleBackColor = true;
            newVersionCheck.Visible = false;
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(newsBox);
            groupBox1.Location = new Point(194, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(462, 358);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "News";
            newsBox.Dock = DockStyle.Fill;
            newsBox.Location = new Point(3, 16);
            newsBox.Name = "newsBox";
            newsBox.Size = new Size(456, 339);
            newsBox.TabIndex = 0;
            userGroupLabel.AutoSize = true;
            userGroupLabel.Dock = DockStyle.Bottom;
            userGroupLabel.Location = new Point(3, 360);
            userGroupLabel.Name = "userGroupLabel";
            userGroupLabel.Size = new Size(86, 13);
            userGroupLabel.TabIndex = 6;
            userGroupLabel.Text = "Account Status: ";
            hackVersionLabel.AutoSize = true;
            hackVersionLabel.Dock = DockStyle.Bottom;
            hackVersionLabel.Location = new Point(3, 373);
            hackVersionLabel.Name = "hackVersionLabel";
            hackVersionLabel.Size = new Size(76, 13);
            hackVersionLabel.TabIndex = 5;
            hackVersionLabel.Text = "Hack version: ";
            loaderVersionLabel.AutoSize = true;
            loaderVersionLabel.Dock = DockStyle.Bottom;
            loaderVersionLabel.Location = new Point(3, 386);
            loaderVersionLabel.Name = "loaderVersionLabel";
            loaderVersionLabel.Size = new Size(83, 13);
            loaderVersionLabel.TabIndex = 4;
            loaderVersionLabel.Text = "Loader version: ";
            startButton.Enabled = false;
            startButton.Location = new Point(9, 28);
            startButton.Name = "startButton";
            startButton.Size = new Size(121, 23);
            startButton.TabIndex = 3;
            startButton.Text = "Start Dota2";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += new EventHandler(startButton_Click);
            reloadButton.Location = new Point(9, 57);
            reloadButton.Name = "reloadButton";
            reloadButton.Size = new Size(121, 21);
            reloadButton.TabIndex = 2;
            reloadButton.Text = "Reload Scripts";
            reloadButton.UseVisualStyleBackColor = true;
            reloadButton.Click += new EventHandler(reloadButton_Click);
            label1.AutoSize = true;
            label1.Location = new Point(8, 3);
            label1.Name = "label1";
            label1.Size = new Size(40, 13);
            label1.TabIndex = 1;
            label1.Text = "Status:";
            chatPage.Controls.Add(ircTabControl);
            chatPage.Location = new Point(4, 22);
            chatPage.Name = "chatPage";
            chatPage.Size = new Size(675, 402);
            chatPage.TabIndex = 6;
            chatPage.Text = "Chat";
            chatPage.UseVisualStyleBackColor = true;
            ircTabControl.Appearance = TabAppearance.Buttons;
            ircTabControl.Controls.Add(tabPage1);
            ircTabControl.Controls.Add(tabPage2);
            ircTabControl.Dock = DockStyle.Fill;
            ircTabControl.ItemSize = new Size(0, 1);
            ircTabControl.Location = new Point(0, 0);
            ircTabControl.Name = "ircTabControl";
            ircTabControl.SelectedIndex = 0;
            ircTabControl.Size = new Size(675, 402);
            ircTabControl.SizeMode = TabSizeMode.Fixed;
            ircTabControl.TabIndex = 7;
            tabPage1.Controls.Add(splitContainer1);
            tabPage1.Location = new Point(4, 5);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(667, 393);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            splitContainer1.Panel1.Controls.Add(label4);
            splitContainer1.Panel1.Controls.Add(ircLanguage);
            splitContainer1.Panel1.Controls.Add(label2);
            splitContainer1.Panel1.Controls.Add(label3);
            splitContainer1.Panel1.Controls.Add(enterChatButton);
            splitContainer1.Panel1.Controls.Add(autoEnterChatCheckBox);
            splitContainer1.Panel2.Controls.Add(statusBox);
            splitContainer1.Size = new Size(661, 387);
            splitContainer1.SplitterDistance = 70;
            splitContainer1.TabIndex = 5;
            label4.AutoSize = true;
            label4.Location = new Point(270, 20);
            label4.Name = "label4";
            label4.Size = new Size(58, 13);
            label4.TabIndex = 6;
            label4.Text = "Language:";
            ircLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            ircLanguage.FormattingEnabled = true;
            ircLanguage.Items.AddRange(new object[3]
            {
         "English",
         "Russian",
         "German"
            });
            ircLanguage.Location = new Point(334, 16);
            ircLanguage.Name = "ircLanguage";
            ircLanguage.Size = new Size(121, 21);
            ircLanguage.TabIndex = 5;
            label2.AutoSize = true;
            label2.Location = new Point(3, 0);
            label2.Name = "label2";
            label2.Size = new Size(385, 13);
            label2.TabIndex = 2;
            label2.Text = "The chat will connect to irc.rizon.net:6697/zynox with your forum account name.";
            label3.AutoSize = true;
            label3.Location = new Point(3, 53);
            label3.Name = "label3";
            label3.Size = new Size(40, 13);
            label3.TabIndex = 4;
            label3.Text = "Status:";
            enterChatButton.Location = new Point(6, 16);
            enterChatButton.Name = "enterChatButton";
            enterChatButton.Size = new Size(75, 22);
            enterChatButton.TabIndex = 0;
            enterChatButton.Text = "Enter chat";
            enterChatButton.UseVisualStyleBackColor = true;
            enterChatButton.Click += new EventHandler(enterChatButton_Click);
            autoEnterChatCheckBox.AutoSize = true;
            autoEnterChatCheckBox.Location = new Point(87, 20);
            autoEnterChatCheckBox.Name = "autoEnterChatCheckBox";
            autoEnterChatCheckBox.Size = new Size(125, 17);
            autoEnterChatCheckBox.TabIndex = 1;
            autoEnterChatCheckBox.Text = "Enter chat on startup";
            autoEnterChatCheckBox.UseVisualStyleBackColor = true;
            autoEnterChatCheckBox.CheckedChanged += new EventHandler(autoEnterChatCheckBox_CheckedChanged);
            statusBox.Dock = DockStyle.Fill;
            statusBox.FormattingEnabled = true;
            statusBox.Location = new Point(0, 0);
            statusBox.Name = "statusBox";
            statusBox.Size = new Size(661, 313);
            statusBox.TabIndex = 3;
            tabPage2.Controls.Add(splitContainer2);
            tabPage2.Location = new Point(4, 5);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(667, 393);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.FixedPanel = FixedPanel.Panel2;
            splitContainer2.IsSplitterFixed = true;
            splitContainer2.Location = new Point(3, 3);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            splitContainer2.Panel1.Controls.Add(splitContainer3);
            splitContainer2.Panel2.Controls.Add(flowLayoutPanel1);
            splitContainer2.Size = new Size(661, 387);
            splitContainer2.SplitterDistance = 354;
            splitContainer2.TabIndex = 5;
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Panel1.Controls.Add(ircMessageBox);
            splitContainer3.Panel2.Controls.Add(ircUserList);
            splitContainer3.Size = new Size(661, 354);
            splitContainer3.SplitterDistance = 509;
            splitContainer3.TabIndex = 0;
            ircMessageBox.Dock = DockStyle.Fill;
            ircMessageBox.Location = new Point(0, 0);
            ircMessageBox.Name = "ircMessageBox";
            ircMessageBox.ReadOnly = true;
            ircMessageBox.Size = new Size(509, 354);
            ircMessageBox.TabIndex = 4;
            ircMessageBox.Text = "";
            ircUserList.Dock = DockStyle.Fill;
            ircUserList.DrawMode = DrawMode.OwnerDrawVariable;
            ircUserList.FormattingEnabled = true;
            ircUserList.Location = new Point(0, 0);
            ircUserList.Name = "ircUserList";
            ircUserList.Size = new Size(148, 354);
            ircUserList.TabIndex = 4;
            ircUserList.DrawItem += new DrawItemEventHandler(ircUserList_DrawItem);
            ircUserList.SelectedIndexChanged += new EventHandler(ircUserList_SelectedIndexChanged);
            ircUserList.Resize += new EventHandler(ircUserList_Resize);
            flowLayoutPanel1.Controls.Add(ircSendButton);
            flowLayoutPanel1.Controls.Add(ircChatText);
            flowLayoutPanel1.Controls.Add(ircCloseButton);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(661, 29);
            flowLayoutPanel1.TabIndex = 4;
            flowLayoutPanel1.Resize += new EventHandler(flowLayoutPanel1_Resize);
            ircSendButton.Location = new Point(3, 3);
            ircSendButton.Name = "ircSendButton";
            ircSendButton.Size = new Size(75, 20);
            ircSendButton.TabIndex = 1;
            ircSendButton.Text = "Send";
            ircSendButton.UseVisualStyleBackColor = true;
            ircSendButton.Click += new EventHandler(ircSendButton_Click);
            ircChatText.AcceptsTab = true;
            ircChatText.Location = new Point(84, 3);
            ircChatText.Multiline = true;
            ircChatText.Name = "ircChatText";
            ircChatText.Size = new Size(492, 20);
            ircChatText.TabIndex = 2;
            ircChatText.KeyDown += new KeyEventHandler(ircChatText_KeyDown);
            ircChatText.KeyPress += new KeyPressEventHandler(ircChatText_KeyPress);
            ircCloseButton.Location = new Point(582, 3);
            ircCloseButton.Name = "ircCloseButton";
            ircCloseButton.Size = new Size(74, 20);
            ircCloseButton.TabIndex = 0;
            ircCloseButton.Text = "Quit Chat";
            ircCloseButton.UseVisualStyleBackColor = true;
            ircCloseButton.Click += new EventHandler(ircCloseButton_Click);
            scriptsPage.Controls.Add(scriptsDataGrid);
            scriptsPage.Location = new Point(4, 22);
            scriptsPage.Name = "scriptsPage";
            scriptsPage.Padding = new Padding(3);
            scriptsPage.Size = new Size(675, 402);
            scriptsPage.TabIndex = 1;
            scriptsPage.Text = "Scripts";
            scriptsPage.UseVisualStyleBackColor = true;
            scriptsDataGrid.AllowUserToAddRows = false;
            scriptsDataGrid.AllowUserToDeleteRows = false;
            scriptsDataGrid.AllowUserToResizeColumns = false;
            scriptsDataGrid.AllowUserToResizeRows = false;
            scriptsDataGrid.AutoGenerateColumns = false;
            scriptsDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            scriptsDataGrid.Columns.AddRange(LoadScript, ScriptName, ScriptDescription, State, ConfigColumn);
            scriptsDataGrid.DataSource = listBinding;
            scriptsDataGrid.Dock = DockStyle.Fill;
            scriptsDataGrid.Location = new Point(3, 3);
            scriptsDataGrid.Name = "scriptsDataGrid";
            scriptsDataGrid.Size = new Size(669, 396);
            scriptsDataGrid.TabIndex = 0;
            scriptsDataGrid.CellContentClick += new DataGridViewCellEventHandler(scriptsDataGrid_CellContentClick);
            scriptsDataGrid.CellFormatting += new DataGridViewCellFormattingEventHandler(scriptsDataGrid_CellFormatting);
            LoadScript.DataPropertyName = "LoadScript";
            LoadScript.HeaderText = "Load";
            LoadScript.Name = "LoadScript";
            LoadScript.Width = 38;
            ScriptName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            ScriptName.DataPropertyName = "ScriptName";
            ScriptName.HeaderText = "Name";
            ScriptName.Name = "ScriptName";
            ScriptName.ReadOnly = true;
            ScriptName.Width = 60;
            ScriptDescription.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ScriptDescription.DataPropertyName = "ScriptDescription";
            ScriptDescription.HeaderText = "Description";
            ScriptDescription.Name = "ScriptDescription";
            ScriptDescription.ReadOnly = true;
            State.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            State.HeaderText = "Libs";
            State.MinimumWidth = 16;
            State.Name = "State";
            State.ReadOnly = true;
            State.Width = 32;
            ConfigColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            ConfigColumn.HeaderText = "Config";
            ConfigColumn.Name = "ConfigColumn";
            ConfigColumn.ReadOnly = true;
            ConfigColumn.Width = 43;
            scriptConfigPage.Controls.Add(scriptConfigGrid);
            scriptConfigPage.Location = new Point(4, 22);
            scriptConfigPage.Name = "scriptConfigPage";
            scriptConfigPage.Size = new Size(675, 402);
            scriptConfigPage.TabIndex = 5;
            scriptConfigPage.Text = "Script-Config";
            scriptConfigPage.UseVisualStyleBackColor = true;
            scriptConfigGrid.Dock = DockStyle.Fill;
            scriptConfigGrid.HelpVisible = false;
            scriptConfigGrid.Location = new Point(0, 0);
            scriptConfigGrid.Name = "scriptConfigGrid";
            scriptConfigGrid.Size = new Size(675, 402);
            scriptConfigGrid.TabIndex = 0;
            scriptConfigGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(scriptConfigGrid_PropertyValueChanged);
            repository.Controls.Add(tableLayoutPanel1);
            repository.Location = new Point(4, 22);
            repository.Name = "repository";
            repository.Padding = new Padding(3);
            repository.Size = new Size(675, 402);
            repository.TabIndex = 4;
            repository.Text = "Repository";
            repository.UseVisualStyleBackColor = true;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32.61868f));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67.38132f));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(3, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 396f));
            tableLayoutPanel1.Size = new Size(669, 396);
            tableLayoutPanel1.TabIndex = 0;
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tableLayoutPanel2.Controls.Add(tableLayoutPanel3, 0, 2);
            tableLayoutPanel2.Controls.Add(repoScripts, 0, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(0, 0);
            tableLayoutPanel2.Margin = new Padding(0);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 3;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 25f));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 25f));
            tableLayoutPanel2.Size = new Size(669, 396);
            tableLayoutPanel2.TabIndex = 1;
            tableLayoutPanel3.ColumnCount = 4;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80f));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120f));
            tableLayoutPanel3.Controls.Add(repoName, 0, 0);
            tableLayoutPanel3.Controls.Add(addRepo, 1, 0);
            tableLayoutPanel3.Controls.Add(removeRepo, 3, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(0, 371);
            tableLayoutPanel3.Margin = new Padding(0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tableLayoutPanel3.Size = new Size(669, 25);
            tableLayoutPanel3.TabIndex = 0;
            repoName.Dock = DockStyle.Fill;
            repoName.Location = new Point(3, 3);
            repoName.Name = "repoName";
            repoName.Size = new Size(343, 20);
            repoName.TabIndex = 1;
            repoName.TextChanged += new EventHandler(repoName_TextChanged);
            addRepo.Dock = DockStyle.Fill;
            addRepo.Enabled = false;
            addRepo.Location = new Point(352, 3);
            addRepo.Name = "addRepo";
            addRepo.Size = new Size(114, 19);
            addRepo.TabIndex = 0;
            addRepo.Text = "Add";
            addRepo.UseVisualStyleBackColor = true;
            addRepo.Click += new EventHandler(addRepo_Click);
            removeRepo.Dock = DockStyle.Fill;
            removeRepo.Enabled = false;
            removeRepo.Location = new Point(552, 3);
            removeRepo.Name = "removeRepo";
            removeRepo.Size = new Size(114, 19);
            removeRepo.TabIndex = 1;
            removeRepo.Text = "Remove";
            removeRepo.UseVisualStyleBackColor = true;
            removeRepo.Click += new EventHandler(removeRepo_Click);
            repoScripts.CheckBoxes = true;
            repoScripts.Dock = DockStyle.Fill;
            repoScripts.Location = new Point(3, 3);
            repoScripts.Name = "repoScripts";
            repoScripts.Size = new Size(663, 340);
            repoScripts.TabIndex = 1;
            repoScripts.AfterCheck += new TreeViewEventHandler(repoScripts_AfterCheck);
            repoScripts.NodeMouseClick += new TreeNodeMouseClickEventHandler(repoScripts_NodeMouseClick);
            configPage.Controls.Add(configGrid);
            configPage.Location = new Point(4, 22);
            configPage.Name = "configPage";
            configPage.Size = new Size(675, 402);
            configPage.TabIndex = 2;
            configPage.Text = "Options";
            configPage.UseVisualStyleBackColor = true;
            configGrid.Dock = DockStyle.Fill;
            configGrid.Location = new Point(0, 0);
            configGrid.Name = "configGrid";
            configGrid.Size = new Size(675, 402);
            configGrid.TabIndex = 0;
            configGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(configGrid_PropertyValueChanged);
            helpPage.Controls.Add(changelogLink);
            helpPage.Controls.Add(bugLink);
            helpPage.Controls.Add(scriptsLink);
            helpPage.Controls.Add(installLink);
            helpPage.Location = new Point(4, 22);
            helpPage.Name = "helpPage";
            helpPage.Size = new Size(675, 402);
            helpPage.TabIndex = 3;
            helpPage.Text = "Help";
            helpPage.UseVisualStyleBackColor = true;
            changelogLink.AutoSize = true;
            changelogLink.Location = new Point(8, 55);
            changelogLink.Name = "changelogLink";
            changelogLink.Size = new Size(58, 13);
            changelogLink.TabIndex = 3;
            changelogLink.TabStop = true;
            changelogLink.Text = "Changelog";
            changelogLink.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkClicked);
            bugLink.AutoSize = true;
            bugLink.Location = new Point(8, 78);
            bugLink.Name = "bugLink";
            bugLink.Size = new Size(61, 13);
            bugLink.TabIndex = 2;
            bugLink.TabStop = true;
            bugLink.Text = "Bug reports";
            bugLink.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkClicked);
            scriptsLink.AutoSize = true;
            scriptsLink.Location = new Point(8, 34);
            scriptsLink.Name = "scriptsLink";
            scriptsLink.Size = new Size(68, 13);
            scriptsLink.TabIndex = 1;
            scriptsLink.TabStop = true;
            scriptsLink.Text = "Scripts forum";
            scriptsLink.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkClicked);
            installLink.AutoSize = true;
            installLink.Location = new Point(8, 12);
            installLink.Name = "installLink";
            installLink.Size = new Size(86, 13);
            installLink.TabIndex = 0;
            installLink.TabStop = true;
            installLink.Text = "Installation guide";
            installLink.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkClicked);
            dotaFindTimer.Interval = 125;
            dotaFindTimer.Tick += new EventHandler(dotaFindTimer_Tick);
            taskbarIcon.Icon = (Icon)componentResourceManager.GetObject("taskbarIcon.Icon");
            taskbarIcon.Text = "Ensage";
            taskbarIcon.Visible = true;
            taskbarIcon.Click += new EventHandler(notifyIcon1_Click);
            injectWorker.WorkerReportsProgress = true;
            injectWorker.DoWork += new DoWorkEventHandler(injectWorker_DoWork);
            injectWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(injectWorker_RunWorkerCompleted);
            pipeWorker.WorkerReportsProgress = true;
            pipeWorker.DoWork += new DoWorkEventHandler(pipeWorker_DoWork);
            pipeWorker.ProgressChanged += new ProgressChangedEventHandler(pipeWorker_ProgressChanged);
            serverWorker.DoWork += new DoWorkEventHandler(serverWorker_DoWork);
            updateTimer.Interval = 60000;
            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            loadWorker.WorkerReportsProgress = true;
            loadWorker.DoWork += new DoWorkEventHandler(loadWorker_DoWork);
            loadWorker.ProgressChanged += new ProgressChangedEventHandler(loadWorker_ProgressChanged);
            DisableVAC.AutoSize = true;
            DisableVAC.Location = new Point(11, 151);
            DisableVAC.Name = "DisableVAC";
            DisableVAC.Size = new Size(103, 17);
            DisableVAC.TabIndex = 9;
            DisableVAC.Text = "disable anti-VAC";
            DisableVAC.UseVisualStyleBackColor = true;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(683, 428);
            Controls.Add(tabControl1);
            Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            MinimumSize = new Size(683, 428);
            Name = "Main";
            Text = "Ensage - Main";
            FormClosing += new FormClosingEventHandler(Main_FormClosing);
            Shown += new EventHandler(Main_Shown);
            Resize += new EventHandler(Main_Resize);
            tabControl1.ResumeLayout(false);
            mainPage.ResumeLayout(false);
            mainPage.PerformLayout();
            groupBox1.ResumeLayout(false);
            chatPage.ResumeLayout(false);
            ircTabControl.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.EndInit();
            splitContainer1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.EndInit();
            splitContainer2.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            splitContainer3.EndInit();
            splitContainer3.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            scriptsPage.ResumeLayout(false);
            ((ISupportInitialize)scriptsDataGrid).EndInit();
            ((ISupportInitialize)listBinding).EndInit();
            scriptConfigPage.ResumeLayout(false);
            repository.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            configPage.ResumeLayout(false);
            helpPage.ResumeLayout(false);
            helpPage.PerformLayout();
            ResumeLayout(false);
        }

        [StructLayout(LayoutKind.Explicit, Size = 280)]
        private struct SafetyInfo
        {
            [FieldOffset(0)]
            public uint serviceHandle;
            [FieldOffset(4)]
            public uint serviceSize;
            [FieldOffset(8)]
            public uint steamHandle;
            [FieldOffset(12)]
            public uint steamSize;
            [FieldOffset(16)]
            public uint ensageHandle;
            [FieldOffset(20)]
            public uint ensageSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            [FieldOffset(24)]
            public char[] ensagePath;
        }
    }
}
