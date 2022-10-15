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
            StatusLibrary.Initialize();

            int i = 0;
            foreach (StatusType status in Enum.GetValues(typeof(StatusType)))
            {
                if (status == StatusType.StatusBar)
                {
                    continue;
                }
                if (status == StatusType.Lua)
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
                
                StatusLibrary.SubscribeText(status, new EventHandler<string>((object src, string value) => { label.Text = value; }));
                StatusLibrary.SubscribeIsFixNeeded(status, new EventHandler<bool>((object src, bool value) => { buttonFix.Visible = value; buttonFixAll.Visible = value; label.ForeColor = (value == true ? Color.Red : Color.Black); }));
                StatusLibrary.SubscribeStage(status, new EventHandler<int>((object src, int value) => { progress.Value = value; }));

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
            StatusLibrary.SubscribeText(lua, new EventHandler<string>((object src, string value) => { lblLua.Text = value; }));
            StatusLibrary.SubscribeIsFixNeeded(lua, new EventHandler<bool>((object src, bool value) => { btnLuaFix.Visible = value; lblLua.ForeColor = (value == true ? Color.Red : Color.Black); }));
            StatusLibrary.SubscribeStage(lua, new EventHandler<int>((object src, int value) => { prgLua.Value = value; }));
            Lua.Check();

            StatusLibrary.SubscribeText(StatusType.StatusBar, new EventHandler<string>((object src, string value) => { lblStatusBar.Text = value; }));
                       

            Text = $"Emu Launcher v{Assembly.GetEntryAssembly().GetName().Version}";
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
        }

    }
}


