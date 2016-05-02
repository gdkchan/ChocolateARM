namespace ChocolateARM
{
    partial class FrmUARTOutput
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
            this.Tabs = new System.Windows.Forms.TabControl();
            this.TabMiniUART = new System.Windows.Forms.TabPage();
            this.TabUART = new System.Windows.Forms.TabPage();
            this.BtnOk = new System.Windows.Forms.Button();
            this.TxtMiniUART = new System.Windows.Forms.TextBox();
            this.TxtUART = new System.Windows.Forms.TextBox();
            this.BtnClear = new System.Windows.Forms.Button();
            this.BtnClearAll = new System.Windows.Forms.Button();
            this.Tabs.SuspendLayout();
            this.TabMiniUART.SuspendLayout();
            this.TabUART.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.TabMiniUART);
            this.Tabs.Controls.Add(this.TabUART);
            this.Tabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.Tabs.Location = new System.Drawing.Point(0, 0);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(584, 319);
            this.Tabs.TabIndex = 0;
            // 
            // TabMiniUART
            // 
            this.TabMiniUART.Controls.Add(this.TxtMiniUART);
            this.TabMiniUART.Location = new System.Drawing.Point(4, 22);
            this.TabMiniUART.Name = "TabMiniUART";
            this.TabMiniUART.Padding = new System.Windows.Forms.Padding(3);
            this.TabMiniUART.Size = new System.Drawing.Size(576, 293);
            this.TabMiniUART.TabIndex = 0;
            this.TabMiniUART.Text = "Mini UART";
            this.TabMiniUART.UseVisualStyleBackColor = true;
            // 
            // TabUART
            // 
            this.TabUART.Controls.Add(this.TxtUART);
            this.TabUART.Location = new System.Drawing.Point(4, 22);
            this.TabUART.Name = "TabUART";
            this.TabUART.Padding = new System.Windows.Forms.Padding(3);
            this.TabUART.Size = new System.Drawing.Size(576, 293);
            this.TabUART.TabIndex = 1;
            this.TabUART.Text = "UART (PL011)";
            this.TabUART.UseVisualStyleBackColor = true;
            // 
            // BtnOk
            // 
            this.BtnOk.Location = new System.Drawing.Point(481, 325);
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Size = new System.Drawing.Size(96, 24);
            this.BtnOk.TabIndex = 1;
            this.BtnOk.Text = "&OK";
            this.BtnOk.UseVisualStyleBackColor = true;
            this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // TxtMiniUART
            // 
            this.TxtMiniUART.BackColor = System.Drawing.Color.Black;
            this.TxtMiniUART.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TxtMiniUART.ForeColor = System.Drawing.Color.White;
            this.TxtMiniUART.Location = new System.Drawing.Point(3, 3);
            this.TxtMiniUART.Multiline = true;
            this.TxtMiniUART.Name = "TxtMiniUART";
            this.TxtMiniUART.ReadOnly = true;
            this.TxtMiniUART.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TxtMiniUART.Size = new System.Drawing.Size(570, 287);
            this.TxtMiniUART.TabIndex = 0;
            // 
            // TxtUART
            // 
            this.TxtUART.BackColor = System.Drawing.Color.Black;
            this.TxtUART.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TxtUART.ForeColor = System.Drawing.Color.White;
            this.TxtUART.Location = new System.Drawing.Point(3, 3);
            this.TxtUART.Multiline = true;
            this.TxtUART.Name = "TxtUART";
            this.TxtUART.ReadOnly = true;
            this.TxtUART.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TxtUART.Size = new System.Drawing.Size(570, 287);
            this.TxtUART.TabIndex = 1;
            // 
            // BtnClear
            // 
            this.BtnClear.Location = new System.Drawing.Point(7, 325);
            this.BtnClear.Name = "BtnClear";
            this.BtnClear.Size = new System.Drawing.Size(96, 24);
            this.BtnClear.TabIndex = 2;
            this.BtnClear.Text = "&Clear";
            this.BtnClear.UseVisualStyleBackColor = true;
            this.BtnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // BtnClearAll
            // 
            this.BtnClearAll.Location = new System.Drawing.Point(109, 325);
            this.BtnClearAll.Name = "BtnClearAll";
            this.BtnClearAll.Size = new System.Drawing.Size(96, 24);
            this.BtnClearAll.TabIndex = 3;
            this.BtnClearAll.Text = "&Clear all";
            this.BtnClearAll.UseVisualStyleBackColor = true;
            this.BtnClearAll.Click += new System.EventHandler(this.BtnClearAll_Click);
            // 
            // FrmUARTOutput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.BtnClearAll);
            this.Controls.Add(this.BtnClear);
            this.Controls.Add(this.BtnOk);
            this.Controls.Add(this.Tabs);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmUARTOutput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UART Output";
            this.Load += new System.EventHandler(this.FrmUARTOutput_Load);
            this.Tabs.ResumeLayout(false);
            this.TabMiniUART.ResumeLayout(false);
            this.TabMiniUART.PerformLayout();
            this.TabUART.ResumeLayout(false);
            this.TabUART.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage TabMiniUART;
        private System.Windows.Forms.TextBox TxtMiniUART;
        private System.Windows.Forms.TabPage TabUART;
        private System.Windows.Forms.TextBox TxtUART;
        private System.Windows.Forms.Button BtnOk;
        private System.Windows.Forms.Button BtnClear;
        private System.Windows.Forms.Button BtnClearAll;
    }
}