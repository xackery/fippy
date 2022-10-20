using EQEmu_Launcher.Manage;
using Microsoft.Win32;
using MS.WindowsAPICodePack.Internal;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using System.Windows.Shell;

namespace EQEmu_Launcher
{
    
    public partial class MainForm : Form
    {
        Regex descriptionLinkRegex = new Regex(@"(.*)\[(.*)\]\((.*)\)(.*)");
        string lastDescription;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
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
            StatusLibrary.Initialize();

            StatusType context;

            // Content
            context = StatusType.Server;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblServer.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picServer.BackColor = value ? Color.Red : Color.Lime; }); }));
            Server.Check();

            context = StatusType.Database;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblDatabase.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picDatabase.BackColor = value ? Color.Red : Color.Lime; }); }));
            Database.Check();

            context = StatusType.Quest;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblQuest.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picQuest.BackColor = value ? Color.Red : Color.Lime; }); }));
            Quest.Check();

            context = StatusType.Map;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblMap.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picMap.BackColor = value ? Color.Red : Color.Lime; }); }));
            Map.Check();

            // Manage
            context = StatusType.SQL;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblSQL.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picSQL.BackColor = value ? Color.Red : Color.Lime; }); }));
            SQL.Check();

            context = StatusType.Zone;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblZone.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picZone.BackColor = value ? Color.Red : Color.Lime; }); }));
            Zone.Check();

            context = StatusType.World;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblWorld.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picWorld.BackColor = value ? Color.Red : Color.Lime; }); }));
            World.Check();

            context = StatusType.UCS;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblUCS.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picUCS.BackColor = value ? Color.Red : Color.Lime; }); }));
            UCS.Check();


            context = StatusType.QueryServ;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblQueryServ.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(context, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { picQueryServ.BackColor = value ? Color.Red : Color.Lime; }); }));
            QueryServ.Check();

            context = StatusType.StatusBar;
            StatusLibrary.SubscribeText(context, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblStatusBar.Text = value; Console.WriteLine("StatusBar: "+value); }); }));

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
                if (matches.Count > 0)
                {
                    Console.WriteLine(matches[0]);
                    lblDescription.Text = matches[0].Groups[1].Value;
                    area.Start = lblDescription.Text.Length;
                    area.Length = matches[0].Groups[2].Value.Length;
                    lblDescription.Text += matches[0].Groups[2].Value;
                    lblDescription.Tag = matches[0].Groups[3].Value;
                    lblDescription.Text += matches[0].Groups[4].Value;
                }
                lblDescription.LinkArea = area;
            }); }));

            ConfigLoad();

            cmbQuest.SelectedIndex = 0;
            cmbDatabase.SelectedIndex = 0;
            cmbServer.SelectedIndex = 0;
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
            Zone.Check();
        }

        private void btnZoneStart_Click(object sender, EventArgs e)
        {
            Zone.Start();
            Zone.Check();
        }

        private void btnZoneStop_Click(object sender, EventArgs e)
        {
            Zone.Stop();
            Zone.Check();
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

            StatusLibrary.SetStatusBar($"starting heidi via {path}");
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = $"-h {Config.Data?["server"]?["database"]?["host"]} -u {Config.Data?["server"]?["database"]?["username"]} -p {Config.Data?["server"]?["database"]?["password"]} -P {Config.Data?["server"]?["database"]?["port"]}",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                }
            };
            proc.Start();
        }

        public void ConfigLoad()
        {
            Config.Load();
            txtKey.Text = Config.Data?["server"]?["world"]?["key"];
            txtLongName.Text = Config.Data?["server"]?["world"]?["longname"];
            txtShortName.Text = Config.Data?["server"]?["world"]?["shortname"];
            txtUsername.Text = Config.Data?["server"]?["database"]?["username"];
            txtPassword.Text = Config.Data?["server"]?["database"]?["password"];
            txtPort.Text = Config.Data?["server"]?["database"]?["port"];
            txtHost.Text = Config.Data?["server"]?["database"]?["host"];
            txtDatabase.Text = Config.Data?["server"]?["database"]?["db"];
        }

        private void btnConfigLoad_Click(object sender, EventArgs e)
        {
            ConfigLoad();
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
                string path = $"{Application.StartupPath}\\db\\mariadb-5.5.29-winx64\\bin\\mysqld.exe";
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = $"--init-file=\"{rootPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                Task.Run(() => { 
                    Thread.Sleep(3000);
                    try
                    {
                        File.Delete(rootPath);
                    } catch (Exception ex)
                    {
                        Console.WriteLine($"failing silently: {ex.Message}");
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
            Config.Save();
            StatusLibrary.SetStatusBar("eqemu_config.json saved");
        }

        private void txtShortName_MouseMove(object sender, MouseEventArgs e)
        {
            lblDescription.Text = "Short Name is used for a prefix on profiles of players when they create characters on your server.";
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
            Zone.Stop();
            Zone.Start();
            Zone.Check();
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
            grpContentAdvanced.Enabled = chkContentAdvanced.Checked;
        }

        private void chkContentAdvanced_MouseMove(object sender, MouseEventArgs e)
        {
            lblDescription.Text = "Advanced options allows you to customize where to get database, quest, and server content from";
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

        private void lblContent_MouseMove(object sender, MouseEventArgs e)
        {
            StatusLibrary.SetDescription("Test");
        }

        private void btnContentDownloadAll_MouseMove(object sender, MouseEventArgs e)
        {
            StatusLibrary.SetDescription("Downloads all out of date content, and installs it to the portable copy emu launcher is handling");
        }

        private void lblDescription_Click(object sender, LinkLabelLinkClickedEventArgs e)
        {
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
    }
}



