using EQEmu_Launcher.Manage;
using Microsoft.Win32;
using MySqlConnector;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Shell;

namespace EQEmu_Launcher
{

    public partial class MainForm : Form
    {
        Regex descriptionLinkRegex = new Regex(@"(.*)\[(.*)\]\((.*)\)(.*)");
        string lastDescription;
        bool isRulesPopulated;
        bool isLastGMAccountRefreshing;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            StatusLibrary.InitLog();
            int darkThemeValue = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1);
            bool isDarkTheme = darkThemeValue == 0;
            if (isDarkTheme)
            {
                var preference = Convert.ToInt32(true);
                WinLibrary.DwmSetWindowAttribute(this.Handle,
                                      WinLibrary.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                                      ref preference, sizeof(uint));
                ChangeTheme(this.Controls, true);
            }

            StatusType context;

            // Content
            context = StatusType.Server;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblServer.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate {
                    picContent.BackColor = value ? Color.Red : Color.Lime;
                    picServer.BackColor = value ? Color.Red : Color.Lime;
                    //btnContentDownloadAll.Enabled = value;
            });
            }));

            context = StatusType.QueryServ;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblQueryServ.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picQueryServ.BackColor = value ? Color.Red : Color.Lime; }); }));
            QueryServ.Check();

            context = StatusType.Quest;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblQuest.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picQuest.BackColor = value ? Color.Red : Color.Lime; }); }));

            context = StatusType.Map;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblMap.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picMap.BackColor = value ? Color.Red : Color.Lime; }); }));


            context = StatusType.Database;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblDatabase.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picDatabase.BackColor = value ? Color.Red : Color.Lime; }); }));

            Server.Check();
            Database.Check();

            // Manage
            context = StatusType.SQL;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblSQL.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { 
                picSQL.BackColor = value ? Color.Red : Color.Lime;
                btnHeidi.Enabled = !value;
                btnSQLBackup.Enabled = !value;
                btnSQLRestore.Enabled = !value;
            }); }));
            SQL.Check();

            context = StatusType.Zone;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblZone.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { 
                picZone.BackColor = value ? Color.Red : Color.Lime;
            }); }));
            Zones.Check();

            context = StatusType.World;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblWorld.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picWorld.BackColor = value ? Color.Red : Color.Lime; }); }));
            World.Check();

            context = StatusType.UCS;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblUCS.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picUCS.BackColor = value ? Color.Red : Color.Lime; }); }));
            UCS.Check();


            context = StatusType.StatusBar;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate 
            { 
                lblStatusBar.Text = value;
                StatusLibrary.Log($"StatusBar: {value}");
            }); }));

            StatusLibrary.SubscribeProgress(new StatusLibrary.ProgressHandler((int value) => { Invoke((MethodInvoker)delegate { 
                prgStatus.Visible = (value != 100);
                btnCancel.Visible = (value != 100);
                lblDescription.Visible = (value == 100);
                tabControlMain.Enabled = (value == 100);
                prgStatus.Value = value; 
            }); }));
            StatusLibrary.SubscribeDescription(new StatusLibrary.DescriptionHandler((string value) => { Invoke((MethodInvoker)delegate {
                if (lastDescription == value) {
                    return;
                }
                lastDescription = value;
                lblDescription.Tag = "";
                var area = new LinkArea();
                MatchCollection matches = descriptionLinkRegex.Matches(value);
                if (matches.Count == 0)
                {
                    lblDescription.LinkArea = area;
                    lblDescription.Text = value;
                    return;
                }

                lblDescription.Text = matches[0].Groups[1].Value;
                area.Start = lblDescription.Text.Length;
                area.Length = matches[0].Groups[2].Value.Length;
                lblDescription.Text += matches[0].Groups[2].Value;
                lblDescription.Tag = matches[0].Groups[3].Value;
                lblDescription.Text += matches[0].Groups[4].Value;
                lblDescription.LinkArea = area;
            }); }));

            context = StatusType.SharedMemory;
            StatusLibrary.SubscribeIsEnabled(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate {
                grpSharedMemory.Enabled = value;
                grpWorld.Enabled = value;
                bool isRunning = World.IsRunning();
                grpZone.Enabled = isRunning;
                grpUCS.Enabled = isRunning;
                grpQueryServ.Enabled = isRunning;
            }); }));

            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => {
                Invoke((MethodInvoker)delegate {
                    picSharedMemory.BackColor = value ? Color.Red : Color.Lime;
                    picSharedMemory.Visible = value;
                });
            }));


            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate {
                    lblSharedMemory.Text = value;
                });
            }));

            context = StatusType.World;
            StatusLibrary.SubscribeIsEnabled(context, new EventHandler<bool>((object src, bool value) => {
                Invoke((MethodInvoker)delegate {
                    grpWorld.Enabled = value;
                    bool isRunning = World.IsRunning();
                    grpZone.Enabled = isRunning;
                    grpUCS.Enabled = isRunning;
                    grpQueryServ.Enabled = isRunning;
                });
            }));

            Config.SubscribeOnLoad(new Config.NullHandler(() => { Invoke((MethodInvoker)delegate {
                    txtKey.Text = Config.Data?["server"]?["world"]?["key"];
                    txtLongName.Text = Config.Data?["server"]?["world"]?["longname"];
                    txtShortName.Text = Config.Data?["server"]?["world"]?["shortname"];
                    txtUsername.Text = Config.Data?["server"]?["database"]?["username"];
                    txtPassword.Text = Config.Data?["server"]?["database"]?["password"];
                    txtPort.Text = Config.Data?["server"]?["database"]?["port"];
                    txtHost.Text = Config.Data?["server"]?["database"]?["host"];
                    txtDatabase.Text = Config.Data?["server"]?["database"]?["db"];
                    chkTelnet.Checked = (Config.Data?["server"]?["world"]?["telnet"]?["enabled"] == "true");
                }); 
            }));
            Config.Load();

            SharedMemory.Check();
            cmbQuest.SelectedIndex = 0;
            cmbDatabase.SelectedIndex = 0;
            cmbServer.SelectedIndex = 0;
            cmbMap.SelectedIndex = 0;
            string dirName = new DirectoryInfo($"{Application.StartupPath}").Name;
            Text = $"Fippy Darklauncher v{Assembly.GetEntryAssembly().GetName().Version} ({dirName} Folder)";
            if (Assembly.GetEntryAssembly().GetName().Version.ToString().Equals("1.0.0.0"))
            {
                Text = $"Fippy Darklauncher Dev Build ({dirName} Folder)";
            }
            menuLauncher.Text = Text;

            try
            {
                MakeSubfolders();
            } catch (Exception ex)
            {
                MessageBox.Show($"Failed to make subfolders: {ex.Message}", "Make Subfolders", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            SQL.Check();
            StatusLibrary.SetStatusBar("Ready");

        }

        private void MakeSubfolders()
        {
            string[] paths =
            {
                Application.StartupPath + "\\db",
                Application.StartupPath + "\\bin",
                Application.StartupPath + "\\cache",
                Application.StartupPath + "\\server",
                Application.StartupPath + "\\server\\logs",
                Application.StartupPath + "\\server\\updates_staged",
                Application.StartupPath + "\\server\\shared",
                Application.StartupPath + "\\server\\quests",
                Application.StartupPath + "\\server\\maps",
            };
            foreach (string path in paths) {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }

        private void FixClick(object sender, EventArgs e)
        {

        }

        private void FixAllClick(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control == null)
            {
                MessageBox.Show($"failed to fix all click due to unknown control {sender} (expected Control)", "FixAll Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StatusType? status = control.Tag as StatusType?;
            if (status == null)
            {
                MessageBox.Show($"failed to fix all click (no tag)", "FixAll Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Type t = Type.GetType($"EQEmu_Launcher.{status}");
            if (t == null)
            {
                MessageBox.Show($"failed to fix all click (class {status} does not exist)", "FixAll Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var method = t.GetMethod("FixAll", BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                MessageBox.Show($"failed to fix all click (class {status} has no method FixAll)", "FixAll Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            method.Invoke(sender, null);
        }


        private void btnSQLStart_Click(object sender, EventArgs e)
        {
            SQL.Start();
            SQL.Check();
        }

        public void ChangeTheme(Control.ControlCollection container, bool isDarkMode)
        {
            
            foreach (Control component in container)
            {
                
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                //notifyIcon.Visible = true;
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!Visible)
            {
                Show();
            }
            this.WindowState = FormWindowState.Normal;
            //notifyIcon.Visible = false;
        }

        private void contextSystrayMenu_Opening(object sender, CancelEventArgs e)
        {

        }

        private void btnSQLStop_Click(object sender, EventArgs e)
        {
            SQL.Stop();
            SQL.Check();
        }

        private void manageTimer_Tick(object sender, EventArgs e)
        {
            SQL.Check();
            Zones.Check();
        }

        private void btnZoneStart_Click(object sender, EventArgs e)
        {
            Zones.Start();
            Zones.Check();
        }

        private void btnZoneStop_Click(object sender, EventArgs e)
        {
            Zones.Stop();
            Zones.Check();
        }

        private void btnHeidi_Click(object sender, EventArgs e)
        {
            
            Process[] processes = Process.GetProcessesByName("heidisql");
            if (processes.Length > 0)
            {

                //StatusLibrary.SetStatusBar("switching to Heidi");
                //WinLibrary.SetForegroundWindow(processes[0].Handle);
                //return;
            }
            string path = $"{Application.StartupPath}\\db\\heidi\\heidisql.exe";
            if (!File.Exists(path))
            {
                string result = $"heidi was not found at {path}. Go to the Checkup tab and press the fix database button to download the portable version";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "Start Heidi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StatusLibrary.SetStatusBar($"Starting heidi via {path}");
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = $"-h {Config.Data?["server"]?["database"]?["host"]} -u {Config.Data?["server"]?["database"]?["username"]} -p {Config.Data?["server"]?["database"]?["password"]} -P {Config.Data?["server"]?["database"]?["port"]}",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true
                }
            };
            proc.Start();
        }

        private void btnConfigLoad_Click(object sender, EventArgs e)
        {
            Config.Load();
        }

        private void btnConfigSave_Click(object sender, EventArgs e)
        {

            if (Config.Data["server"]["database"]["username"] == "root" && !Config.Data["server"]["database"]["password"].Equals(txtPassword.Text))
            {
                
                Process[] processes = Process.GetProcessesByName("mysqld");
                bool wasSQLRunning = (processes.Length > 0);
                if (wasSQLRunning)
                {
                    var response = MessageBox.Show("SQL is currently running and you want to change the root password.\nRestart SQL to apply it?", "Apply root password", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                    if (response == DialogResult.Cancel)
                    {
                        StatusLibrary.SetStatusBar("cancelled saving settings");
                        return;
                    }

                    if (response == DialogResult.No)
                    {
                        StatusLibrary.SetStatusBar("cancelled saving settings");
                        return;
                    }

                    if (response == DialogResult.Yes)
                    {
                        SQL.Stop();
                    }
                }

                string rootPath = $"{Application.StartupPath}\\cache\\reset-root.txt";
                File.WriteAllText(rootPath, $"UPDATE mysql.user SET Password=PASSWORD('{txtPassword.Text}') WHERE User='root';\nFLUSH PRIVILEGES;");

                // Start SQL with no root password
                string path = $"{Application.StartupPath}\\db\\mariadb-10.6.10-winx64\\bin\\mysqld.exe";
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = $"--init-file=\"{rootPath}\" --sql-mode=\"NO_ZERO_DATE\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                proc.OutputDataReceived += new DataReceivedEventHandler((object src, DataReceivedEventArgs earg) =>
                {
                    string line = earg.Data;
                    if (line == null)
                    {
                        return;
                    }
                    StatusLibrary.Log($"sql: {line}");
                });
                proc.ErrorDataReceived += new DataReceivedEventHandler((object src, DataReceivedEventArgs earg) =>
                {
                    string line = earg.Data;
                    if (line == null)
                    {
                        return;
                    }
                    StatusLibrary.Log($"sql error: {line}");
                });
                
                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                Task.Run(() => { 
                    Thread.Sleep(3000);
                    try
                    {
                        File.Delete(rootPath);
                    } catch (Exception ex)
                    {
                        StatusLibrary.Log($"failing silently: {ex.Message}");
                    }
                });
                StatusLibrary.SetStatusBar("password changed");

            }

            Config.Data["server"]["world"]["key"] = txtKey.Text;
            Config.Data["server"]["world"]["longname"] = txtLongName.Text;
            Config.Data["server"]["world"]["shortname"] = txtShortName.Text;
            Config.Data["server"]["database"]["username"] = txtUsername.Text;
            Config.Data["server"]["database"]["password"] = txtPassword.Text;
            Config.Data["server"]["database"]["port"] = txtPort.Text;
            Config.Data["server"]["database"]["host"] = txtHost.Text;
            Config.Data["server"]["database"]["db"] = txtDatabase.Text;
            Config.Data["server"]["qsdatabase"]["username"] = txtUsername.Text;
            Config.Data["server"]["qsdatabase"]["password"] = txtPassword.Text;
            Config.Data["server"]["qsdatabase"]["port"] = txtPort.Text;
            Config.Data["server"]["qsdatabase"]["host"] = txtHost.Text;
            Config.Data["server"]["qsdatabase"]["db"] = txtDatabase.Text;
            Config.Data["server"]["world"]["telnet"]["enabled"] = (chkTelnet.Checked ? "true" : "false");
            Config.Save();
            StatusLibrary.SetStatusBar("eqemu_config.json saved");
        }

        private void txtShortName_MouseMove(object sender, MouseEventArgs e)
        {
            StatusLibrary.SetDescription("Short Name is used for a prefix on profiles of players when they create characters on your server.");
        }

        private void lblDatabase_Click(object sender, EventArgs e)
        {

        }

        private void btnRandomizePassword_Click(object sender, EventArgs e)
        {
            
            txtPassword.Text = WinLibrary.RandomString(32);
            StatusLibrary.SetStatusBar("random password generated");
        }

        private void btnRandomize_Click(object sender, EventArgs e)
        {
            txtKey.Text = WinLibrary.RandomString(64);
            StatusLibrary.SetStatusBar("random key generated");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void lblSQL_Click(object sender, EventArgs e)
        {

        }

        private void btnWorldStart_Click(object sender, EventArgs e)
        {
            World.Start();
            World.Check();
        }

        private void btnWorldStop_Click(object sender, EventArgs e)
        {
            World.Stop();
            World.Check();
        }

        private void btnWorldRestart_Click(object sender, EventArgs e)
        {
            World.Stop();
            World.Start();
            World.Check();
        }

        private void btnZoneRestart_Click(object sender, EventArgs e)
        {
            Zones.Stop();
            Zones.Start();
            Zones.Check();
        }

        private void btnUCSStart_Click(object sender, EventArgs e)
        {
            UCS.Start();
            UCS.Check();
        }

        private void btnUCSStop_Click(object sender, EventArgs e)
        {
            UCS.Stop();
            UCS.Check();
        }

        private void btnUCSRestart_Click(object sender, EventArgs e)
        {
            UCS.Stop();
            UCS.Start();
            UCS.Check();
        }

        private void btnQueryServStart_Click(object sender, EventArgs e)
        {
            QueryServ.Start();
            QueryServ.Check();
        }

        private void btnQueryServStop_Click(object sender, EventArgs e)
        {
            QueryServ.Stop();
            QueryServ.Check();
        }

        private void btnQueryServRestart_Click(object sender, EventArgs e)
        {
            QueryServ.Stop();
            QueryServ.Start();
            QueryServ.Check();
        }

        private void btnLuaFix_Click(object sender, EventArgs e)
        {

        }

        private void lblConfigLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", $"\"{Application.StartupPath}\\server\\eqemu_config.json\"");
        }

        private void chkContentAdvanced_CheckedChanged(object sender, EventArgs e)
        {

            if (chkContentAdvanced.Checked)
            {
                chkContentAdvanced.Checked = false;
                MessageBox.Show("The advanced content area is not yet ready, and is just a placeholder for now\nDownload via the big button on top and move to next step", "Not yet available");
            }
            
            //grpContentAdvanced.Enabled = chkContentAdvanced.Checked;
        }

        private void chkContentAdvanced_MouseMove(object sender, MouseEventArgs e)
        {
            StatusLibrary.SetDescription("Advanced allows you to customize where to get database, quest, and server content from.");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            StatusLibrary.UnlockUI();
        }

        private void btnContentDownloadAll_Click(object sender, EventArgs e)
        {
            Server.FixAll();
        }

        private void txtLongName_MouseMove(object sender, MouseEventArgs e)
        {
           StatusLibrary.SetDescription("This is how your server is displayed on [server select](https://google.com)");
        }


        private void btnContentDownloadAll_MouseMove(object sender, MouseEventArgs e)
        {
            StatusLibrary.SetDescription("Downloads all out of date content, and installs it to the portable copy emu launcher is handling");
        }

        private void lblDescription_Click(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (lblDescription.Tag == null)
            {
                return;
            }
            if (lblDescription.Tag.ToString() == "")
            {
                return;
            }
            Process.Start("explorer.exe", lblDescription.Tag.ToString());
        }

        private void chkConfigAdvanced_CheckedChanged(object sender, EventArgs e)
        {
            grpConfigAdvanced.Enabled = chkConfigAdvanced.Checked;
        }

        private void txtZoneCount_TextChanged(object sender, EventArgs e)
        {
            
            Regex regex = new Regex(@"([^0-9])");
            MatchCollection matches = regex.Matches(txtZoneCount.Text);
            if (matches.Count > 0)
            {
                txtZoneCount.Text = matches[0].Groups[0].Value;
            }
        }

        private void btnZoneLogs_Click(object sender, EventArgs e)
        {
            // I won't bother with figuring out a specific zone file, and let a player find it (or maybe a selection window later to grab a specific instance?)
            Process.Start("explorer.exe", $"{Application.StartupPath}\\server\\logs\\zone");
        }

        private void btnWorldLogs_Click(object sender, EventArgs e)
        {
            string path = $"{Application.StartupPath}\\server\\logs";
            Process[] processes = Process.GetProcessesByName("world");
            foreach (Process process in processes)
            {
                if (!process.MainModule.FileName.Equals($"{Application.StartupPath}\\server\\world.exe"))
                {
                    continue;
                }
                string logPath = $"{path}\\world_{process.Id}.log";
                StatusLibrary.Log($"checking for {process.MainModule.FileName}");
                if (File.Exists(logPath))
                {
                    Process.Start("explorer.exe", logPath);
                    return;
                }
            }

            Process.Start("explorer.exe", path);
        }

        private void btnUCSLogs_Click(object sender, EventArgs e)
        {
            string path = $"{Application.StartupPath}\\server\\logs";
            Process[] processes = Process.GetProcessesByName("ucs");
            foreach (Process process in processes)
            {
                if (!process.MainModule.FileName.Equals($"{Application.StartupPath}\\server\\ucs.exe")) {
                    continue;
                }
                string logPath = $"{path}\\ucs_{process.Id}.log";
                StatusLibrary.Log($"checking for {process.MainModule.FileName}");
                if (File.Exists(logPath))
                {
                    Process.Start("explorer.exe", logPath);
                    return;
                }
            }            

            Process.Start("explorer.exe", path);
        }

        private void btnQueryServLogs_Click(object sender, EventArgs e)
        {
            string path = $"{Application.StartupPath}\\server\\logs";
            Process[] processes = Process.GetProcessesByName("queryserv");
            foreach (Process process in processes)
            {
                if (!process.MainModule.FileName.Equals($"{Application.StartupPath}\\server\\queryserv.exe"))
                {
                    continue;
                }
                string logPath = $"{path}\\query_server_{process.Id}.log";
                StatusLibrary.Log($"Checking for {process.MainModule.FileName}");
                if (File.Exists(logPath))
                {
                    Process.Start("explorer.exe", logPath);
                    return;
                }
            }

            Process.Start("explorer.exe", path);
        }

        private void tabControlMain_Selected(object sender, TabControlEventArgs e)
        {

            StatusLibrary.Log($"Tab changed to {e.TabPage.Text}");
            if (e.TabPage.Text.Equals("Rules"))
            {
                PopulateRules();
            }

            if (e.TabPage.Text.Equals("GM"))
            {
                RefreshGM();
            }
        }

        private void PopulateRules()
        {
            if (isRulesPopulated)
            {
                return;
            }

            StatusLibrary.Log("Populating rules...");
            isRulesPopulated = true;
            var table = new DataTable();
            var col = table.Columns.Add("Category", typeof(string));
            col.ReadOnly = true;
            col = table.Columns.Add("Name", typeof(string));
            col.ReadOnly = true;
            table.Columns.Add("Value", typeof(string));
            col = table.Columns.Add("Notes", typeof(string));
            col.ReadOnly = true;
            Task.Run(async () =>
            {
                try
                {
                    string user = "root";
                    string password = Config.Data?["server"]?["database"]?["password"];
                    string dbName = Config.Data?["server"]?["database"]?["db"];
                    using (MySqlConnection connection = new MySqlConnection($"Server=localhost;User ID={user};Password={password};Database={dbName}"))
                    {
                        await connection.OpenAsync(StatusLibrary.CancelToken());

                        MySqlCommand command = new MySqlCommand("SELECT rule_name, rule_value, notes FROM rule_values WHERE ruleset_id = 1;", connection);

                        using (var reader = await command.ExecuteReaderAsync(StatusLibrary.CancelToken()))
                        {
                            while (await reader.ReadAsync(StatusLibrary.CancelToken()))
                            {
                                var names = reader.GetString(0).Split(':');
                                table.Rows.Add(names[0], names[1], reader.GetString(1), reader.GetString(2));
                            }
                        }
                    }
                    Invoke((MethodInvoker)delegate
                    {
                        gridRules.DataSource = table;
                    });
                } catch (Exception ex)
                {
                    StatusLibrary.Log($"Failed to populate rules grid: {ex.Message}");
                }
            });
        }

        private void gridRules_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string ruleName = $"{gridRules.Rows[e.RowIndex].Cells[0].Value}:{gridRules.Rows[e.RowIndex].Cells[1].Value}";
            
            gridRules.Rows[e.RowIndex].ErrorText = $"Testing {ruleName}";
        }

        private void lblConfigLink_MouseMove(object sender, MouseEventArgs e)
        {
            StatusLibrary.SetDescription("Open eqemu_config.json in a text editor.\nNOTE: Press Reload File if you do this, and don't edit any fields listed above");
        }

        private void chkConfigAdvanced_MouseMove(object sender, MouseEventArgs e)
        {
            StatusLibrary.SetDescription("Advanced allows you to customize additional config options.\nSome of these fields are dangerous and can break Fippy, use at your own risk!");
        }

        private void RefreshGM() 
        {
            if (Config.Data == null)
            {
                return;
            } 
            if (isLastGMAccountRefreshing)
            {
                return;
            }
            isLastGMAccountRefreshing = true;
            Task.Run(async () =>
            {
                using (MySqlConnection connection = new MySqlConnection($"Server=localhost;User ID={Config.Data?["server"]?["database"]?["username"]};Password={Config.Data?["server"]?["database"]?["password"]};Database={Config.Data?["server"]?["database"]?["db"]}"))
                {
                    await connection.OpenAsync(StatusLibrary.CancelToken());
                    StatusLibrary.Log("Looking for new GM accounts");
                    MySqlCommand command = new MySqlCommand("SELECT id, name FROM account WHERE status = 0 ORDER BY id DESC LIMIT 1;", connection);

                    using (var reader = await command.ExecuteReaderAsync(StatusLibrary.CancelToken()))
                    {
                        while (await reader.ReadAsync(StatusLibrary.CancelToken()))
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                lblMakeGM.Text = $"Most recent created non-GM account is named {reader.GetString(1)}.";
                                btnMakeGM.Tag = reader.GetInt32(0);
                                btnMakeGM.Enabled = true;
                                isLastGMAccountRefreshing = false;
                            });
                            return;
                        }
                        Invoke((MethodInvoker)delegate
                        {
                            lblMakeGM.Text = $"No accounts have been created yet that are non-GM.";
                            btnMakeGM.Tag = 0;
                            btnMakeGM.Enabled = false;
                            isLastGMAccountRefreshing = false;
                        });
                    }
                }
            });
        }

        private void btnMakeGMRefresh_Click(object sender, EventArgs e)
        {
            if (Config.Data == null)
            {
                return;
            }
            RefreshGM();
        }

        private void btnMakeGM_Click(object sender, EventArgs e)
        {
            if (Config.Data == null)
            {
                return;
            }
            if (btnMakeGM.Tag == null || (int)btnMakeGM.Tag < 1)
            {
                return;
            }
            Task.Run(async () =>
            {
                try
                {
                    
                    string connString = $"Server=localhost;User ID={Config.Data?["server"]?["database"]?["username"]};Password={Config.Data?["server"]?["database"]?["password"]};Database={Config.Data?["server"]?["database"]?["db"]}";
                    StatusLibrary.SetStatusBar($"Setting user ID {btnMakeGM.Tag} to GM using {connString}");
                    using (MySqlConnection connection = new MySqlConnection(connString))
                    {
                        await connection.OpenAsync(StatusLibrary.CancelToken());

                        MySqlCommand command = new MySqlCommand("UPDATE account SET status = 255 WHERE id = @id", connection);
                        command.Parameters.AddWithValue("@id", (int)btnMakeGM.Tag);
                        command.ExecuteNonQuery();

                        RefreshGM();
                        StatusLibrary.SetStatusBar($"Finished setting GM");
                    }
                } catch (Exception ex)
                {
                    StatusLibrary.SetStatusBar($"Failed to set GM: {ex.Message}");
                    RefreshGM();
                }
            });
        }

        private void btnSQLRestore_Click(object sender, EventArgs e)
        {
            MessageBox.Show("not yet implemented");
        }

        private void btnSQLBackup_Click(object sender, EventArgs e)
        {
            MessageBox.Show("not yet implemented");
        }

        private void btnSQLRestart_Click(object sender, EventArgs e)
        {
            if (SQL.Stop())
            {
                SQL.Start();
            }
        }

        private void btnSharedMemory_Click(object sender, EventArgs e)
        {
            if (World.IsRunning() || Zones.IsRunning())
            {
                StatusLibrary.Log("Asking if user wants to run shared memory with world/zone running");
                var response = MessageBox.Show("Running Shared Memory while World or Zone is up can cause negative side effects.\nRun anyways?", "Run Shared Memory", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (response == DialogResult.No)
                {
                    return;
                }
            }
            Task.Run(async () => {
                StatusLibrary.SetProgress(1);
                secondTimer.Enabled = true;
                StatusLibrary.LockUI();                
                StatusLibrary.SetProgress(1);
                secondTimer.Enabled = true;
                SharedMemory.Stop();
                await SharedMemory.Start();
                StatusLibrary.UnlockUI();
                secondTimer.Enabled = false;
            });
        }

        private void secondTimer_Tick(object sender, EventArgs e)
        {
            int value = StatusLibrary.Progress()+5;
            if (value > 100)
            {
                value = 100;
                secondTimer.Enabled = false;
            }
            StatusLibrary.SetProgress(value);
        }

        private void lblSQL_MouseMove(object sender, MouseEventArgs e)
        {
            StatusLibrary.SetDescription(StatusLibrary.Description(StatusType.SQL));
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
    }
}




