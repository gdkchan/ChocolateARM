namespace ChocolateARM
{
    partial class FrmMain
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
            this.Screen = new System.Windows.Forms.PictureBox();
            this.MenuBar = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuLoadKernelImage = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuDumpSDRAM = new System.Windows.Forms.ToolStripMenuItem();
            this.MnuShowUART = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusBar = new System.Windows.Forms.StatusStrip();
            this.ScreenRefresh = new System.Windows.Forms.Timer(this.components);
            this.MnuARMDbg = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.Screen)).BeginInit();
            this.MenuBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // Screen
            // 
            this.Screen.BackColor = System.Drawing.Color.Black;
            this.Screen.Location = new System.Drawing.Point(0, 24);
            this.Screen.Name = "Screen";
            this.Screen.Size = new System.Drawing.Size(640, 480);
            this.Screen.TabIndex = 2;
            this.Screen.TabStop = false;
            // 
            // MenuBar
            // 
            this.MenuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.MnuTools});
            this.MenuBar.Location = new System.Drawing.Point(0, 0);
            this.MenuBar.Name = "MenuBar";
            this.MenuBar.Size = new System.Drawing.Size(640, 24);
            this.MenuBar.TabIndex = 3;
            this.MenuBar.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuLoadKernelImage});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // MnuLoadKernelImage
            // 
            this.MnuLoadKernelImage.Name = "MnuLoadKernelImage";
            this.MnuLoadKernelImage.Size = new System.Drawing.Size(172, 22);
            this.MnuLoadKernelImage.Text = "&Load Kernel image";
            this.MnuLoadKernelImage.Click += new System.EventHandler(this.MnuLoadKernelImage_Click);
            // 
            // MnuTools
            // 
            this.MnuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MnuDumpSDRAM,
            this.MnuShowUART,
            this.MnuARMDbg});
            this.MnuTools.Name = "MnuTools";
            this.MnuTools.Size = new System.Drawing.Size(47, 20);
            this.MnuTools.Text = "&Tools";
            // 
            // MnuDumpSDRAM
            // 
            this.MnuDumpSDRAM.Name = "MnuDumpSDRAM";
            this.MnuDumpSDRAM.Size = new System.Drawing.Size(152, 22);
            this.MnuDumpSDRAM.Text = "&Dump SDRAM";
            this.MnuDumpSDRAM.Click += new System.EventHandler(this.MnuDumpSDRAM_Click);
            // 
            // MnuShowUART
            // 
            this.MnuShowUART.Name = "MnuShowUART";
            this.MnuShowUART.Size = new System.Drawing.Size(152, 22);
            this.MnuShowUART.Text = "&UART output";
            this.MnuShowUART.Click += new System.EventHandler(this.MnuShowUART_Click);
            // 
            // StatusBar
            // 
            this.StatusBar.Location = new System.Drawing.Point(0, 504);
            this.StatusBar.Name = "StatusBar";
            this.StatusBar.Size = new System.Drawing.Size(640, 22);
            this.StatusBar.TabIndex = 4;
            this.StatusBar.Text = "statusStrip1";
            // 
            // ScreenRefresh
            // 
            this.ScreenRefresh.Interval = 16;
            this.ScreenRefresh.Tick += new System.EventHandler(this.ScreenRefresh_Tick);
            // 
            // MnuARMDbg
            // 
            this.MnuARMDbg.Name = "MnuARMDbg";
            this.MnuARMDbg.Size = new System.Drawing.Size(152, 22);
            this.MnuARMDbg.Text = "&ARM Debug";
            this.MnuARMDbg.Click += new System.EventHandler(this.MnuARMDbg_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 526);
            this.Controls.Add(this.StatusBar);
            this.Controls.Add(this.Screen);
            this.Controls.Add(this.MenuBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.MenuBar;
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ChocolatePi Alpha";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Screen)).EndInit();
            this.MenuBar.ResumeLayout(false);
            this.MenuBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox Screen;
        private System.Windows.Forms.MenuStrip MenuBar;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MnuLoadKernelImage;
        private System.Windows.Forms.StatusStrip StatusBar;
        private System.Windows.Forms.ToolStripMenuItem MnuTools;
        private System.Windows.Forms.ToolStripMenuItem MnuDumpSDRAM;
        private System.Windows.Forms.Timer ScreenRefresh;
        private System.Windows.Forms.ToolStripMenuItem MnuShowUART;
        private System.Windows.Forms.ToolStripMenuItem MnuARMDbg;
    }
}

