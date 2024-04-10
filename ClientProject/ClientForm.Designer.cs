namespace ClientProject
{
    partial class ClientForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnConnect = new Button();
            txtIpAddress = new TextBox();
            txtPort = new TextBox();
            label1 = new Label();
            label2 = new Label();
            lblStatus = new Label();
            btnTestMessage = new Button();
            lstClientMessages = new ListBox();
            txtMessage = new TextBox();
            btnDisconnect = new Button();
            btnSendFile = new Button();
            SuspendLayout();
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(93, 71);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(103, 23);
            btnConnect.TabIndex = 0;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // txtIpAddress
            // 
            txtIpAddress.Location = new Point(93, 13);
            txtIpAddress.Name = "txtIpAddress";
            txtIpAddress.Size = new Size(207, 23);
            txtIpAddress.TabIndex = 1;
            // 
            // txtPort
            // 
            txtPort.Location = new Point(93, 42);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(207, 23);
            txtPort.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(38, 16);
            label1.Name = "label1";
            label1.Size = new Size(52, 15);
            label1.TabIndex = 2;
            label1.Text = "Address:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(55, 45);
            label2.Name = "label2";
            label2.Size = new Size(32, 15);
            label2.TabIndex = 2;
            label2.Text = "Port:";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(327, 16);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(88, 15);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "Not Connected";
            // 
            // btnTestMessage
            // 
            btnTestMessage.Location = new Point(438, 12);
            btnTestMessage.Name = "btnTestMessage";
            btnTestMessage.Size = new Size(216, 23);
            btnTestMessage.TabIndex = 5;
            btnTestMessage.Text = "Test Message";
            btnTestMessage.UseVisualStyleBackColor = true;
            btnTestMessage.Click += btnTestMessage_Click;
            // 
            // lstClientMessages
            // 
            lstClientMessages.FormattingEnabled = true;
            lstClientMessages.ItemHeight = 15;
            lstClientMessages.Location = new Point(38, 104);
            lstClientMessages.Name = "lstClientMessages";
            lstClientMessages.Size = new Size(300, 334);
            lstClientMessages.TabIndex = 6;
            // 
            // txtMessage
            // 
            txtMessage.Location = new Point(438, 45);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(216, 75);
            txtMessage.TabIndex = 7;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Location = new Point(202, 71);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new Size(98, 23);
            btnDisconnect.TabIndex = 8;
            btnDisconnect.Text = "Disconnect";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // btnSendFile
            // 
            btnSendFile.Location = new Point(438, 156);
            btnSendFile.Name = "btnSendFile";
            btnSendFile.Size = new Size(216, 23);
            btnSendFile.TabIndex = 9;
            btnSendFile.Text = "Send File";
            btnSendFile.UseVisualStyleBackColor = true;
            btnSendFile.Click += btnSendFile_Click;
            // 
            // ClientForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnSendFile);
            Controls.Add(btnDisconnect);
            Controls.Add(txtMessage);
            Controls.Add(lstClientMessages);
            Controls.Add(btnTestMessage);
            Controls.Add(lblStatus);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtPort);
            Controls.Add(txtIpAddress);
            Controls.Add(btnConnect);
            Name = "ClientForm";
            Text = "Form1";
            FormClosing += ClientForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnConnect;
        private TextBox txtIpAddress;
        private TextBox txtPort;
        private Label label1;
        private Label label2;
        private Label lblStatus;
        private Button btnTestMessage;
        private ListBox lstClientMessages;
        private TextBox txtMessage;
        private Button btnDisconnect;
        private Button btnSendFile;
    }
}