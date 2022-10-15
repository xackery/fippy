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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatusBar = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabGM = new System.Windows.Forms.TabPage();
            this.tabMonitor = new System.Windows.Forms.TabPage();
            this.tabManage = new System.Windows.Forms.TabPage();
            this.tabCheckup = new System.Windows.Forms.TabPage();
            this.lblDescription = new System.Windows.Forms.Label();
            this.grpLua = new System.Windows.Forms.GroupBox();
            this.btnLuaFixAll = new System.Windows.Forms.Button();
            this.prgLua = new System.Windows.Forms.ProgressBar();
            this.btnLuaFix = new System.Windows.Forms.Button();
            this.lblLua = new System.Windows.Forms.Label();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.statusStrip1.SuspendLayout();
            this.tabCheckup.SuspendLayout();
            this.grpLua.SuspendLayout();
            this.tabControlMain.SuspendLayout();
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
            this.tabGM.Size = new System.Drawing.Size(545, 509);
            this.tabGM.TabIndex = 4;
            this.tabGM.Text = "GM";
            this.tabGM.UseVisualStyleBackColor = true;
            // 
            // tabMonitor
            // 
            this.tabMonitor.Location = new System.Drawing.Point(4, 22);
            this.tabMonitor.Name = "tabMonitor";
            this.tabMonitor.Size = new System.Drawing.Size(545, 509);
            this.tabMonitor.TabIndex = 3;
            this.tabMonitor.Text = "Monitor";
            this.tabMonitor.UseVisualStyleBackColor = true;
            // 
            // tabManage
            // 
            this.tabManage.Location = new System.Drawing.Point(4, 22);
            this.tabManage.Name = "tabManage";
            this.tabManage.Size = new System.Drawing.Size(545, 509);
            this.tabManage.TabIndex = 2;
            this.tabManage.Text = "Manage";
            this.tabManage.UseVisualStyleBackColor = true;
            // 
            // tabCheckup
            // 
            this.tabCheckup.Controls.Add(this.lblDescription);
            this.tabCheckup.Controls.Add(this.grpLua);
            this.tabCheckup.Location = new System.Drawing.Point(4, 22);
            this.tabCheckup.Name = "tabCheckup";
            this.tabCheckup.Padding = new System.Windows.Forms.Padding(3);
            this.tabCheckup.Size = new System.Drawing.Size(545, 509);
            this.tabCheckup.TabIndex = 0;
            this.tabCheckup.Text = "Checkup";
            this.tabCheckup.UseVisualStyleBackColor = true;
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(13, 371);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(520, 69);
            this.lblDescription.TabIndex = 8;
            this.lblDescription.Text = "More information here";
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
            this.tabControlMain.Size = new System.Drawing.Size(553, 535);
            this.tabControlMain.TabIndex = 9;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(577, 572);
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
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabCheckup.ResumeLayout(false);
            this.grpLua.ResumeLayout(false);
            this.grpLua.PerformLayout();
            this.tabControlMain.ResumeLayout(false);
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
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Button btnLuaFixAll;
    }
}

