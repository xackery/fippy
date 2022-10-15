using EQEmu_Launcher.Manage;
using Microsoft.Win32;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
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
using System.Windows.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
//using System.Windows.Shell;

namespace EQEmu_Launcher
{
    
    public partial class MainForm : Form
    {
        StatusType lastDescription;

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

            var skips = new List<StatusType> 
            { 
                StatusType.StatusBar,
                StatusType.Lua,
                StatusType.SQL,
                StatusType.Zone,
            };
            int i = 0;
            foreach (StatusType status in Enum.GetValues(typeof(StatusType)))
            {
                if (skips.Contains(status))
                {
                    continue;
                }
                i++;

                var group = new GroupBox
                {
                    Width = grpLua.Width,
                    Height = grpLua.Height,
                    Left = grpLua.Left,
                    Top = i * grpLua.Height + grpLua.Top,
                    Text = status.ToString(),
                    Anchor = grpLua.Anchor,
                    Parent = tabCheckup,
                    Tag = status,
                };
                group.MouseMove += new System.Windows.Forms.MouseEventHandler(MouseMoveDescription);

                var label = new System.Windows.Forms.Label
                {
                    Width = lblLua.Width,
                    Height = lblLua.Height,
                    Left = lblLua.Left,
                    Top = lblLua.Top,
                    Anchor = lblLua.Anchor,
                    Parent = group,
                    Tag = status,
                };
                label.MouseMove += new System.Windows.Forms.MouseEventHandler(MouseMoveDescription);

                var buttonFix = new Button
                {
                    Width = btnLuaFix.Width,
                    Height = btnLuaFix.Height,
                    Left = btnLuaFix.Left,
                    Top = btnLuaFix.Top,
                    Anchor = btnLuaFix.Anchor,
                    Text = "Fix",
                    Parent = group,
                    Tag = status,
                };
                buttonFix.MouseMove += new System.Windows.Forms.MouseEventHandler(MouseMoveDescription);
                buttonFix.Click += new EventHandler(FixClick);

                var buttonFixAll = new Button
                {
                    Width = btnLuaFixAll.Width,
                    Height = btnLuaFixAll.Height,
                    Left = btnLuaFixAll.Left,
                    Top = btnLuaFixAll.Top,
                    Anchor = btnLuaFixAll.Anchor,
                    Text = "Fix All",
                    Parent = group,
                    Tag = status,
                };
                buttonFixAll.MouseMove += new System.Windows.Forms.MouseEventHandler(MouseMoveDescription);
                buttonFixAll.Click += new EventHandler(FixAllClick);


                var progress = new ProgressBar
                {
                    Width = prgLua.Width,
                    Height = prgLua.Height,
                    Left = prgLua.Left,
                    Top = prgLua.Top,
                    Anchor = prgLua.Anchor,
                    Parent = group,
                    Visible = false,
                    Tag = status,
                };
                progress.MouseMove += new System.Windows.Forms.MouseEventHandler(MouseMoveDescription);
                
                StatusLibrary.SubscribeText(status, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { label.Text = value; }); }));
                StatusLibrary.SubscribeIsFixNeeded(status, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { buttonFix.Visible = value; buttonFixAll.Visible = value; label.ForeColor = (value == true ? Color.Red : Color.Black); }); }));
                StatusLibrary.SubscribeStage(status, new EventHandler<int>((object src, int value) => { Invoke((MethodInvoker)delegate { progress.Visible = (value != 100); progress.Value = value; }); }));

                Type t = Type.GetType($"EQEmu_Launcher.{status}");
                if (t == null)
                {
                    MessageBox.Show($"failed to find check (class {status} does not exist)", "Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var method = t.GetMethod("Check", BindingFlags.Static | BindingFlags.Public);
                if (method == null)
                {
                    MessageBox.Show($"failed to find check (class {status} has no method Check)", "Check", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                method.Invoke(sender, null);
            }

            StatusType lua = StatusType.Lua;
            lblLua.Tag = lua;
            btnLuaFix.Tag = lua;
            prgLua.Tag = lua;
            btnLuaFix.Click += new EventHandler(FixClick);
            StatusLibrary.SubscribeText(lua, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblLua.Text = value; }); }));
            StatusLibrary.SubscribeIsFixNeeded(lua, new EventHandler<bool>((object src, bool value) => { Invoke((MethodInvoker)delegate { btnLuaFix.Visible = value; lblLua.ForeColor = (value == true ? Color.Red : Color.Black); }); }));
            StatusLibrary.SubscribeStage(lua, new EventHandler<int>((object src, int value) => { Invoke((MethodInvoker)delegate { prgLua.Visible = (value != 100); prgLua.Value = value; }); }));
            Lua.Check();

            StatusLibrary.SubscribeText(StatusType.SQL, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblSQL.Text = value; }); }));
            SQL.Check();

            StatusLibrary.SubscribeText(StatusType.Zone, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblZone.Text = value; }); }));
            Zone.Check();

            StatusLibrary.SubscribeText(StatusType.StatusBar, new EventHandler<string>((object src, string value) => { Invoke((MethodInvoker)delegate { lblStatusBar.Text = value; Console.WriteLine("StatusBar: "+value); }); }));

            ConfigLoad();
            Text = $"Emu Launcher v{Assembly.GetEntryAssembly().GetName().Version}";
            menuLauncher.Text = Text;
        }

        private void FixClick(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control == null)
            {
                MessageBox.Show($"failed to fix click due to unknown control {sender} (expected Control)", "Fix Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StatusType? status = control.Tag as StatusType?;
            if (status == null)
            {
                MessageBox.Show($"failed to fix click (no tag)", "Fix Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            Type t = Type.GetType($"EQEmu_Launcher.{status}");
            if (t == null)
            {
                MessageBox.Show($"failed to fix click (class {status} does not exist)", "Fix Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var method = t.GetMethod("FixCheck", BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                MessageBox.Show($"failed to fix click (class {status} has no method FixCheck)", "Fix Click", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            method.Invoke(sender, null);
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

        private void MouseMoveDescription(object sender, MouseEventArgs e)
        {

            Control control = sender as Control;
            if (control == null)
            {
                return;
            }

            StatusType? status = control.Tag as StatusType?;
            if (status == null)
            {
                return;
            }

            if (lastDescription == status)
            {
                return;
            }

            lastDescription = status ?? StatusType.Database;

            lblDescription.Text = StatusLibrary.Description(status ?? StatusType.Database);
            if (lblDescription.Text == "")
            {
                lblDescription.Text = "Rawr";
            }
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

                StatusLibrary.SetStatusBar("switching to Heidi");
                WinLibrary.SetForegroundWindow(processes[0].Handle);
                return;
            }
            string path = $"{Application.StartupPath}\\db\\heidi\\heidisql.exe";
            if (!File.Exists(path))
            {
                string result = $"heidi was not found at {path}. Go to the Checkup tab and press the fix database button to download the portable version";
                StatusLibrary.SetStatusBar(result);
                MessageBox.Show(result, "Start Heidi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StatusLibrary.SetStatusBar("starting heidi");
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
                if (processes.Length > 0)
                {
                    var response = MessageBox.Show("SQL is currently running and you want to change the root password.\nRestart SQL to apply it?", "Restart SQL", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                    if (response == DialogResult.Cancel)
                    {
                        return;
                    }
                    if (response == DialogResult.Yes)
                    {
                        if (!SQL.Stop())
                        {
                            return;
                        }
                        StatusLibrary.SetStatusBar("setting password");
                        string path = $"{Application.StartupPath}\\db\\mariadb-5.5.29-winx64\\bin\\mysqladmin.exe";
                        var proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = path,
                                Arguments = $"-u root flush-privileges password {txtPassword.Text}",
                                UseShellExecute = false,
                                RedirectStandardOutput = false,
                                CreateNoWindow = true
                            }
                        };
                        proc.Start();
                        StatusLibrary.SetStatusBar("password changed");
                        SQL.Start();
                    }

                }
            }

            Config.Data["server"]["world"]["key"] = txtKey.Text;
            Config.Data["server"]["world"]["longname"] = txtLongName.Text;
            Config.Data["server"]["world"]["shortname"] = txtShortName.Text;
            Config.Data["server"]["database"]["username"] = txtUsername.Text;
            Config.Data["server"]["database"]["password"] = txtPassword.Text;
            Config.Data["server"]["database"]["port"] = txtPort.Text;
            Config.Data["server"]["database"]["host"] = txtHost.Text;
            Config.Data["server"]["database"]["db"] = txtDatabase.Text;
            Config.Save();
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
    }
}



