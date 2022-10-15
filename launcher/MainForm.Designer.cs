namespace EQEmu_Launcher
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatusBar = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabGM = new System.Windows.Forms.TabPage();
            this.tabMonitor = new System.Windows.Forms.TabPage();
            this.tabManage = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button18 = new System.Windows.Forms.Button();
            this.button19 = new System.Windows.Forms.Button();
            this.button20 = new System.Windows.Forms.Button();
            this.button21 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button14 = new System.Windows.Forms.Button();
            this.button15 = new System.Windows.Forms.Button();
            this.button16 = new System.Windows.Forms.Button();
            this.button17 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button12 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.btnZoneStop = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.btnZoneStart = new System.Windows.Forms.Button();
            this.lblZone = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button13 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.grpSQL = new System.Windows.Forms.GroupBox();
            this.button22 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.btnSQLStop = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSQLStart = new System.Windows.Forms.Button();
            this.lblSQL = new System.Windows.Forms.Label();
            this.tabCheckup = new System.Windows.Forms.TabPage();
            this.grpLua = new System.Windows.Forms.GroupBox();
            this.btnLuaFixAll = new System.Windows.Forms.Button();
            this.prgLua = new System.Windows.Forms.ProgressBar();
            this.btnLuaFix = new System.Windows.Forms.Button();
            this.lblLua = new System.Windows.Forms.Label();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.lblDescription = new System.Windows.Forms.Label();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextSystrayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuLauncher = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuSQL = new System.Windows.Forms.ToolStripMenuItem();
            this.menuWorld = new System.Windows.Forms.ToolStripMenuItem();
            this.menuZone = new System.Windows.Forms.ToolStripMenuItem();
            this.menuUCS = new System.Windows.Forms.ToolStripMenuItem();
            this.menuQueryServ = new System.Windows.Forms.ToolStripMenuItem();
            this.manageTimer = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            this.tabManage.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grpSQL.SuspendLayout();
            this.tabCheckup.SuspendLayout();
            this.grpLua.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.contextSystrayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 550);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(577, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatusBar
            // 
            this.lblStatusBar.Name = "lblStatusBar";
            this.lblStatusBar.Size = new System.Drawing.Size(0, 17);
            // 
            // tabGM
            // 
            this.tabGM.Location = new System.Drawing.Point(4, 22);
            this.tabGM.Name = "tabGM";
            this.tabGM.Size = new System.Drawing.Size(545, 419);
            this.tabGM.TabIndex = 4;
            this.tabGM.Text = "GM";
            this.tabGM.UseVisualStyleBackColor = true;
            // 
            // tabMonitor
            // 
            this.tabMonitor.Location = new System.Drawing.Point(4, 22);
            this.tabMonitor.Name = "tabMonitor";
            this.tabMonitor.Size = new System.Drawing.Size(545, 419);
            this.tabMonitor.TabIndex = 3;
            this.tabMonitor.Text = "Monitor";
            this.tabMonitor.UseVisualStyleBackColor = true;
            // 
            // tabManage
            // 
            this.tabManage.Controls.Add(this.groupBox4);
            this.tabManage.Controls.Add(this.groupBox3);
            this.tabManage.Controls.Add(this.groupBox2);
            this.tabManage.Controls.Add(this.groupBox1);
            this.tabManage.Controls.Add(this.grpSQL);
            this.tabManage.Location = new System.Drawing.Point(4, 22);
            this.tabManage.Name = "tabManage";
            this.tabManage.Size = new System.Drawing.Size(545, 419);
            this.tabManage.TabIndex = 2;
            this.tabManage.Text = "Manage";
            this.tabManage.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.button18);
            this.groupBox4.Controls.Add(this.button19);
            this.groupBox4.Controls.Add(this.button20);
            this.groupBox4.Controls.Add(this.button21);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Location = new System.Drawing.Point(3, 219);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(539, 48);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "QueryServ";
            // 
            // button18
            // 
            this.button18.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button18.Location = new System.Drawing.Point(463, 15);
            this.button18.Name = "button18";
            this.button18.Size = new System.Drawing.Size(53, 23);
            this.button18.TabIndex = 10;
            this.button18.Text = "Logs";
            this.button18.UseVisualStyleBackColor = true;
            // 
            // button19
            // 
            this.button19.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button19.Location = new System.Drawing.Point(244, 15);
            this.button19.Name = "button19";
            this.button19.Size = new System.Drawing.Size(42, 23);
            this.button19.TabIndex = 5;
            this.button19.Text = "Stop";
            this.button19.UseVisualStyleBackColor = true;
            // 
            // button20
            // 
            this.button20.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button20.Location = new System.Drawing.Point(292, 15);
            this.button20.Name = "button20";
            this.button20.Size = new System.Drawing.Size(53, 23);
            this.button20.TabIndex = 4;
            this.button20.Text = "Restart";
            this.button20.UseVisualStyleBackColor = true;
            // 
            // button21
            // 
            this.button21.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button21.Location = new System.Drawing.Point(196, 15);
            this.button21.Name = "button21";
            this.button21.Size = new System.Drawing.Size(42, 23);
            this.button21.TabIndex = 1;
            this.button21.Text = "Start";
            this.button21.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "queryserv is running";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.button14);
            this.groupBox3.Controls.Add(this.button15);
            this.groupBox3.Controls.Add(this.button16);
            this.groupBox3.Controls.Add(this.button17);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(3, 165);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(539, 48);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "UCS";
            // 
            // button14
            // 
            this.button14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button14.Location = new System.Drawing.Point(463, 15);
            this.button14.Name = "button14";
            this.button14.Size = new System.Drawing.Size(53, 23);
            this.button14.TabIndex = 10;
            this.button14.Text = "Logs";
            this.button14.UseVisualStyleBackColor = true;
            // 
            // button15
            // 
            this.button15.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button15.Location = new System.Drawing.Point(244, 15);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(42, 23);
            this.button15.TabIndex = 5;
            this.button15.Text = "Stop";
            this.button15.UseVisualStyleBackColor = true;
            // 
            // button16
            // 
            this.button16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button16.Location = new System.Drawing.Point(292, 15);
            this.button16.Name = "button16";
            this.button16.Size = new System.Drawing.Size(53, 23);
            this.button16.TabIndex = 4;
            this.button16.Text = "Restart";
            this.button16.UseVisualStyleBackColor = true;
            // 
            // button17
            // 
            this.button17.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button17.Location = new System.Drawing.Point(196, 15);
            this.button17.Name = "button17";
            this.button17.Size = new System.Drawing.Size(42, 23);
            this.button17.TabIndex = 1;
            this.button17.Text = "Start";
            this.button17.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "ucs is running";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.button12);
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Controls.Add(this.btnZoneStop);
            this.groupBox2.Controls.Add(this.button7);
            this.groupBox2.Controls.Add(this.btnZoneStart);
            this.groupBox2.Controls.Add(this.lblZone);
            this.groupBox2.Location = new System.Drawing.Point(3, 57);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(539, 48);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Zone";
            // 
            // button12
            // 
            this.button12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button12.Location = new System.Drawing.Point(463, 15);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(53, 23);
            this.button12.TabIndex = 10;
            this.button12.Text = "Logs";
            this.button12.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(35, 17);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(20, 20);
            this.textBox2.TabIndex = 8;
            this.textBox2.Text = "3";
            // 
            // btnZoneStop
            // 
            this.btnZoneStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZoneStop.Location = new System.Drawing.Point(244, 15);
            this.btnZoneStop.Name = "btnZoneStop";
            this.btnZoneStop.Size = new System.Drawing.Size(42, 23);
            this.btnZoneStop.TabIndex = 5;
            this.btnZoneStop.Text = "Stop";
            this.btnZoneStop.UseVisualStyleBackColor = true;
            this.btnZoneStop.Click += new System.EventHandler(this.btnZoneStop_Click);
            // 
            // button7
            // 
            this.button7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button7.Location = new System.Drawing.Point(292, 15);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(53, 23);
            this.button7.TabIndex = 4;
            this.button7.Text = "Restart";
            this.button7.UseVisualStyleBackColor = true;
            // 
            // btnZoneStart
            // 
            this.btnZoneStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZoneStart.Location = new System.Drawing.Point(196, 15);
            this.btnZoneStart.Name = "btnZoneStart";
            this.btnZoneStart.Size = new System.Drawing.Size(42, 23);
            this.btnZoneStart.TabIndex = 1;
            this.btnZoneStart.Text = "Start";
            this.btnZoneStart.UseVisualStyleBackColor = true;
            this.btnZoneStart.Click += new System.EventHandler(this.btnZoneStart_Click);
            // 
            // lblZone
            // 
            this.lblZone.AutoSize = true;
            this.lblZone.Location = new System.Drawing.Point(7, 20);
            this.lblZone.Name = "lblZone";
            this.lblZone.Size = new System.Drawing.Size(164, 13);
            this.lblZone.TabIndex = 0;
            this.lblZone.Text = "3 of          zone instances running";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button13);
            this.groupBox1.Controls.Add(this.button8);
            this.groupBox1.Controls.Add(this.button9);
            this.groupBox1.Controls.Add(this.button10);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(3, 111);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(539, 48);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "World";
            // 
            // button13
            // 
            this.button13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button13.Location = new System.Drawing.Point(463, 15);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(53, 23);
            this.button13.TabIndex = 10;
            this.button13.Text = "Logs";
            this.button13.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button8.Location = new System.Drawing.Point(244, 15);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(42, 23);
            this.button8.TabIndex = 5;
            this.button8.Text = "Stop";
            this.button8.UseVisualStyleBackColor = true;
            // 
            // button9
            // 
            this.button9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button9.Location = new System.Drawing.Point(292, 15);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(53, 23);
            this.button9.TabIndex = 4;
            this.button9.Text = "Restart";
            this.button9.UseVisualStyleBackColor = true;
            // 
            // button10
            // 
            this.button10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button10.Location = new System.Drawing.Point(196, 15);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(42, 23);
            this.button10.TabIndex = 1;
            this.button10.Text = "Start";
            this.button10.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "world is running";
            // 
            // grpSQL
            // 
            this.grpSQL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSQL.Controls.Add(this.button22);
            this.grpSQL.Controls.Add(this.button5);
            this.grpSQL.Controls.Add(this.button4);
            this.grpSQL.Controls.Add(this.btnSQLStop);
            this.grpSQL.Controls.Add(this.button1);
            this.grpSQL.Controls.Add(this.btnSQLStart);
            this.grpSQL.Controls.Add(this.lblSQL);
            this.grpSQL.Location = new System.Drawing.Point(3, 3);
            this.grpSQL.Name = "grpSQL";
            this.grpSQL.Size = new System.Drawing.Size(539, 48);
            this.grpSQL.TabIndex = 1;
            this.grpSQL.TabStop = false;
            this.grpSQL.Text = "SQL";
            // 
            // button22
            // 
            this.button22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button22.Location = new System.Drawing.Point(351, 15);
            this.button22.Name = "button22";
            this.button22.Size = new System.Drawing.Size(42, 23);
            this.button22.TabIndex = 8;
            this.button22.Text = "Heidi";
            this.button22.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Location = new System.Drawing.Point(404, 15);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(53, 23);
            this.button5.TabIndex = 7;
            this.button5.Text = "Restore";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(463, 15);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(53, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Backup";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // btnSQLStop
            // 
            this.btnSQLStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSQLStop.Location = new System.Drawing.Point(244, 15);
            this.btnSQLStop.Name = "btnSQLStop";
            this.btnSQLStop.Size = new System.Drawing.Size(42, 23);
            this.btnSQLStop.TabIndex = 5;
            this.btnSQLStop.Text = "Stop";
            this.btnSQLStop.UseVisualStyleBackColor = true;
            this.btnSQLStop.Click += new System.EventHandler(this.btnSQLStop_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(292, 15);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(53, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Restart";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnSQLStart
            // 
            this.btnSQLStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSQLStart.Location = new System.Drawing.Point(196, 15);
            this.btnSQLStart.Name = "btnSQLStart";
            this.btnSQLStart.Size = new System.Drawing.Size(42, 23);
            this.btnSQLStart.TabIndex = 1;
            this.btnSQLStart.Text = "Start";
            this.btnSQLStart.UseVisualStyleBackColor = true;
            this.btnSQLStart.Click += new System.EventHandler(this.btnSQLStart_Click);
            // 
            // lblSQL
            // 
            this.lblSQL.AutoSize = true;
            this.lblSQL.Location = new System.Drawing.Point(7, 20);
            this.lblSQL.Name = "lblSQL";
            this.lblSQL.Size = new System.Drawing.Size(76, 13);
            this.lblSQL.TabIndex = 0;
            this.lblSQL.Text = "SQL is running";
            // 
            // tabCheckup
            // 
            this.tabCheckup.Controls.Add(this.grpLua);
            this.tabCheckup.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabCheckup.Location = new System.Drawing.Point(4, 22);
            this.tabCheckup.Name = "tabCheckup";
            this.tabCheckup.Padding = new System.Windows.Forms.Padding(3);
            this.tabCheckup.Size = new System.Drawing.Size(545, 419);
            this.tabCheckup.TabIndex = 0;
            this.tabCheckup.Text = "Checkup";
            this.tabCheckup.UseVisualStyleBackColor = true;
            // 
            // grpLua
            // 
            this.grpLua.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLua.Controls.Add(this.btnLuaFixAll);
            this.grpLua.Controls.Add(this.prgLua);
            this.grpLua.Controls.Add(this.btnLuaFix);
            this.grpLua.Controls.Add(this.lblLua);
            this.grpLua.Location = new System.Drawing.Point(6, 6);
            this.grpLua.Name = "grpLua";
            this.grpLua.Size = new System.Drawing.Size(533, 48);
            this.grpLua.TabIndex = 0;
            this.grpLua.TabStop = false;
            this.grpLua.Text = "Lua";
            // 
            // btnLuaFixAll
            // 
            this.btnLuaFixAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLuaFixAll.Location = new System.Drawing.Point(485, 15);
            this.btnLuaFixAll.Name = "btnLuaFixAll";
            this.btnLuaFixAll.Size = new System.Drawing.Size(42, 23);
            this.btnLuaFixAll.TabIndex = 4;
            this.btnLuaFixAll.Text = "Fix All";
            this.btnLuaFixAll.UseVisualStyleBackColor = true;
            // 
            // prgLua
            // 
            this.prgLua.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.prgLua.Location = new System.Drawing.Point(241, 15);
            this.prgLua.Name = "prgLua";
            this.prgLua.Size = new System.Drawing.Size(197, 23);
            this.prgLua.TabIndex = 3;
            this.prgLua.Visible = false;
            // 
            // btnLuaFix
            // 
            this.btnLuaFix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLuaFix.Location = new System.Drawing.Point(444, 15);
            this.btnLuaFix.Name = "btnLuaFix";
            this.btnLuaFix.Size = new System.Drawing.Size(35, 23);
            this.btnLuaFix.TabIndex = 1;
            this.btnLuaFix.Text = "Fix";
            this.btnLuaFix.UseVisualStyleBackColor = true;
            // 
            // lblLua
            // 
            this.lblLua.AutoSize = true;
            this.lblLua.Location = new System.Drawing.Point(7, 20);
            this.lblLua.Name = "lblLua";
            this.lblLua.Size = new System.Drawing.Size(94, 13);
            this.lblLua.TabIndex = 0;
            this.lblLua.Text = "Lua is not installed";
            // 
            // tabControlMain
            // 
            this.tabControlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlMain.Controls.Add(this.tabCheckup);
            this.tabControlMain.Controls.Add(this.tabManage);
            this.tabControlMain.Controls.Add(this.tabMonitor);
            this.tabControlMain.Controls.Add(this.tabGM);
            this.tabControlMain.Location = new System.Drawing.Point(12, 12);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(553, 445);
            this.tabControlMain.TabIndex = 9;
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(29, 481);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(520, 69);
            this.lblDescription.TabIndex = 11;
            this.lblDescription.Text = "UCS stands for Universal Chat Server, this is the in game channel system and is o" +
    "ptional";
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextSystrayMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // contextSystrayMenu
            // 
            this.contextSystrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuLauncher,
            this.toolStripSeparator1,
            this.menuSQL,
            this.menuWorld,
            this.menuZone,
            this.menuUCS,
            this.menuQueryServ});
            this.contextSystrayMenu.Name = "contextSystrayMenu";
            this.contextSystrayMenu.Size = new System.Drawing.Size(203, 142);
            this.contextSystrayMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextSystrayMenu_Opening);
            // 
            // menuLauncher
            // 
            this.menuLauncher.Name = "menuLauncher";
            this.menuLauncher.Size = new System.Drawing.Size(202, 22);
            this.menuLauncher.Text = "EQEmu Launcher v1234";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(199, 6);
            // 
            // menuSQL
            // 
            this.menuSQL.Name = "menuSQL";
            this.menuSQL.Size = new System.Drawing.Size(202, 22);
            this.menuSQL.Text = "SQL is running";
            // 
            // menuWorld
            // 
            this.menuWorld.Name = "menuWorld";
            this.menuWorld.Size = new System.Drawing.Size(202, 22);
            this.menuWorld.Text = "World is running";
            // 
            // menuZone
            // 
            this.menuZone.Name = "menuZone";
            this.menuZone.Size = new System.Drawing.Size(202, 22);
            this.menuZone.Text = "3 of 3 Zones are running";
            // 
            // menuUCS
            // 
            this.menuUCS.Name = "menuUCS";
            this.menuUCS.Size = new System.Drawing.Size(202, 22);
            this.menuUCS.Text = "UCS is running";
            // 
            // menuQueryServ
            // 
            this.menuQueryServ.Name = "menuQueryServ";
            this.menuQueryServ.Size = new System.Drawing.Size(202, 22);
            this.menuQueryServ.Text = "QueryServ is running";
            // 
            // manageTimer
            // 
            this.manageTimer.Enabled = true;
            this.manageTimer.Interval = 6000;
            this.manageTimer.Tick += new System.EventHandler(this.manageTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 572);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControlMain);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(305, 371);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EQEmu Launcher";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabManage.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpSQL.ResumeLayout(false);
            this.grpSQL.PerformLayout();
            this.tabCheckup.ResumeLayout(false);
            this.grpLua.ResumeLayout(false);
            this.grpLua.PerformLayout();
            this.tabControlMain.ResumeLayout(false);
            this.contextSystrayMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusBar;
        private System.Windows.Forms.TabPage tabGM;
        private System.Windows.Forms.TabPage tabMonitor;
        private System.Windows.Forms.TabPage tabManage;
        private System.Windows.Forms.TabPage tabCheckup;
        private System.Windows.Forms.GroupBox grpLua;
        private System.Windows.Forms.ProgressBar prgLua;
        private System.Windows.Forms.Button btnLuaFix;
        private System.Windows.Forms.Label lblLua;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.Button btnLuaFixAll;
        private System.Windows.Forms.GroupBox grpSQL;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button btnSQLStop;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnSQLStart;
        private System.Windows.Forms.Label lblSQL;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button18;
        private System.Windows.Forms.Button button19;
        private System.Windows.Forms.Button button20;
        private System.Windows.Forms.Button button21;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button14;
        private System.Windows.Forms.Button button15;
        private System.Windows.Forms.Button button16;
        private System.Windows.Forms.Button button17;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button btnZoneStop;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button btnZoneStart;
        private System.Windows.Forms.Label lblZone;
        private System.Windows.Forms.Button button13;
        private System.Windows.Forms.Button button22;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextSystrayMenu;
        private System.Windows.Forms.ToolStripMenuItem menuSQL;
        private System.Windows.Forms.ToolStripMenuItem menuWorld;
        private System.Windows.Forms.ToolStripMenuItem menuZone;
        private System.Windows.Forms.ToolStripMenuItem menuUCS;
        private System.Windows.Forms.ToolStripMenuItem menuQueryServ;
        private System.Windows.Forms.ToolStripMenuItem menuLauncher;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Timer manageTimer;
    }
}

