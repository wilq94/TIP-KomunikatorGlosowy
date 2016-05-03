namespace KomunikatorGlosowySerwer
{
    partial class VoiceCommunicatorServer
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
            this.richTextBoxChatMessages = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // richTextBoxChatMessages
            // 
            this.richTextBoxChatMessages.Location = new System.Drawing.Point(12, 12);
            this.richTextBoxChatMessages.Name = "richTextBoxChatMessages";
            this.richTextBoxChatMessages.ReadOnly = true;
            this.richTextBoxChatMessages.Size = new System.Drawing.Size(327, 238);
            this.richTextBoxChatMessages.TabIndex = 0;
            this.richTextBoxChatMessages.Text = "";
            // 
            // VoiceCommunicatorServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 262);
            this.Controls.Add(this.richTextBoxChatMessages);
            this.Name = "VoiceCommunicatorServer";
            this.Text = "VoiceCommunicatorServer";
            this.Load += new System.EventHandler(this.VoiceCommunicatorServer_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxChatMessages;
    }
}

