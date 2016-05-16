namespace KomunikatorGlosowyKlient
{
    partial class VoiceCommunicatorClient
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.polaczeniaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ustawieniaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pomocToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textBoxNickname = new System.Windows.Forms.TextBox();
            this.textBoxIPAddress = new System.Windows.Forms.TextBox();
            this.listBoxChatters = new System.Windows.Forms.ListBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.textBoxSend = new System.Windows.Forms.TextBox();
            this.buttonSend = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.polaczeniaToolStripMenuItem,
            this.ustawieniaToolStripMenuItem,
            this.pomocToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(834, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // polaczeniaToolStripMenuItem
            // 
            this.polaczeniaToolStripMenuItem.Name = "polaczeniaToolStripMenuItem";
            this.polaczeniaToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.polaczeniaToolStripMenuItem.Text = "Polaczenia";
            // 
            // ustawieniaToolStripMenuItem
            // 
            this.ustawieniaToolStripMenuItem.Name = "ustawieniaToolStripMenuItem";
            this.ustawieniaToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.ustawieniaToolStripMenuItem.Text = "Ustawienia";
            // 
            // pomocToolStripMenuItem
            // 
            this.pomocToolStripMenuItem.Name = "pomocToolStripMenuItem";
            this.pomocToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.pomocToolStripMenuItem.Text = "Pomoc";
            // 
            // textBoxNickname
            // 
            this.textBoxNickname.AccessibleName = "";
            this.textBoxNickname.Location = new System.Drawing.Point(12, 27);
            this.textBoxNickname.Name = "textBoxNickname";
            this.textBoxNickname.Size = new System.Drawing.Size(159, 20);
            this.textBoxNickname.TabIndex = 1;
            this.textBoxNickname.TextChanged += new System.EventHandler(this.textBoxNickname_TextChanged);
            // 
            // textBoxIPAddress
            // 
            this.textBoxIPAddress.Location = new System.Drawing.Point(12, 53);
            this.textBoxIPAddress.Name = "textBoxIPAddress";
            this.textBoxIPAddress.Size = new System.Drawing.Size(159, 20);
            this.textBoxIPAddress.TabIndex = 2;
            this.textBoxIPAddress.TextChanged += new System.EventHandler(this.textBoxIPAddress_TextChanged);
            // 
            // listBoxChatters
            // 
            this.listBoxChatters.FormattingEnabled = true;
            this.listBoxChatters.Location = new System.Drawing.Point(12, 79);
            this.listBoxChatters.Name = "listBoxChatters";
            this.listBoxChatters.Size = new System.Drawing.Size(240, 186);
            this.listBoxChatters.TabIndex = 3;
            this.listBoxChatters.SelectedIndexChanged += new System.EventHandler(this.listBoxChatters_SelectedIndexChanged);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Enabled = false;
            this.buttonConnect.Location = new System.Drawing.Point(177, 51);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 4;
            this.buttonConnect.Text = "POŁĄCZ";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Location = new System.Drawing.Point(91, 271);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(75, 23);
            this.buttonDisconnect.TabIndex = 5;
            this.buttonDisconnect.Text = "ROZŁĄCZ";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            // 
            // textBoxSend
            // 
            this.textBoxSend.Location = new System.Drawing.Point(273, 271);
            this.textBoxSend.Name = "textBoxSend";
            this.textBoxSend.Size = new System.Drawing.Size(468, 20);
            this.textBoxSend.TabIndex = 7;
            this.textBoxSend.TextChanged += new System.EventHandler(this.textBoxSend_TextChanged);
            this.textBoxSend.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSend_KeyDown);
            // 
            // buttonSend
            // 
            this.buttonSend.Location = new System.Drawing.Point(747, 269);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(75, 23);
            this.buttonSend.TabIndex = 8;
            this.buttonSend.Text = "WYŚLIJ";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click_1);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(273, 27);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(549, 238);
            this.textBox1.TabIndex = 9;
            // 
            // VoiceCommunicatorClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 313);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.buttonSend);
            this.Controls.Add(this.textBoxSend);
            this.Controls.Add(this.buttonDisconnect);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.listBoxChatters);
            this.Controls.Add(this.textBoxIPAddress);
            this.Controls.Add(this.textBoxNickname);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "VoiceCommunicatorClient";
            this.Text = "VoiceCommunicatorClient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KomunikatorGlosowyKlient_FormClosing);
            this.Load += new System.EventHandler(this.VoiceCommunicatorClient_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem polaczeniaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ustawieniaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pomocToolStripMenuItem;
        private System.Windows.Forms.TextBox textBoxNickname;
        private System.Windows.Forms.TextBox textBoxIPAddress;
        private System.Windows.Forms.ListBox listBoxChatters;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.TextBox textBoxSend;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.TextBox textBox1;
    }
}

