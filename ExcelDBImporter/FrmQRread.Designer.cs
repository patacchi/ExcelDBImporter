namespace ExcelDBImporter
{
    partial class FrmQRread
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
            TxtBoxQRread = new TextBox();
            BtnEditInput = new Button();
            CmbBoxPortNum = new ComboBox();
            BtnOpenPort = new Button();
            BtnPortClose = new Button();
            LblConnectionStatus = new Label();
            BtnRegistToTempDB = new Button();
            LblElsapedTime = new Label();
            SuspendLayout();
            // 
            // TxtBoxQRread
            // 
            TxtBoxQRread.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TxtBoxQRread.ImeMode = ImeMode.Disable;
            TxtBoxQRread.Location = new Point(16, 86);
            TxtBoxQRread.Multiline = true;
            TxtBoxQRread.Name = "TxtBoxQRread";
            TxtBoxQRread.Size = new Size(598, 352);
            TxtBoxQRread.TabIndex = 0;
            TxtBoxQRread.TextChanged += TextBoxQRread_TextChanged;
            // 
            // BtnEditInput
            // 
            BtnEditInput.Location = new Point(83, 459);
            BtnEditInput.Name = "BtnEditInput";
            BtnEditInput.Size = new Size(190, 30);
            BtnEditInput.TabIndex = 1;
            BtnEditInput.Text = "入力内容を修正する";
            BtnEditInput.UseVisualStyleBackColor = true;
            BtnEditInput.Click += BtnEditInput_Click;
            // 
            // CmbBoxPortNum
            // 
            CmbBoxPortNum.FormattingEnabled = true;
            CmbBoxPortNum.Location = new Point(16, 12);
            CmbBoxPortNum.Name = "CmbBoxPortNum";
            CmbBoxPortNum.Size = new Size(133, 23);
            CmbBoxPortNum.TabIndex = 2;
            // 
            // BtnOpenPort
            // 
            BtnOpenPort.Location = new Point(16, 41);
            BtnOpenPort.Name = "BtnOpenPort";
            BtnOpenPort.Size = new Size(133, 30);
            BtnOpenPort.TabIndex = 1;
            BtnOpenPort.Text = "ポートを開く";
            BtnOpenPort.UseVisualStyleBackColor = true;
            BtnOpenPort.Click += BtnPortOpen_Click;
            // 
            // BtnPortClose
            // 
            BtnPortClose.Location = new Point(155, 41);
            BtnPortClose.Name = "BtnPortClose";
            BtnPortClose.Size = new Size(133, 30);
            BtnPortClose.TabIndex = 1;
            BtnPortClose.Text = "ポートを閉じる";
            BtnPortClose.UseVisualStyleBackColor = true;
            BtnPortClose.Click += BtnPortClose_Click;
            // 
            // LblConnectionStatus
            // 
            LblConnectionStatus.AutoSize = true;
            LblConnectionStatus.Location = new Point(294, 49);
            LblConnectionStatus.Name = "LblConnectionStatus";
            LblConnectionStatus.Size = new Size(0, 15);
            LblConnectionStatus.TabIndex = 3;
            // 
            // BtnRegistToTempDB
            // 
            BtnRegistToTempDB.Enabled = false;
            BtnRegistToTempDB.Location = new Point(281, 459);
            BtnRegistToTempDB.Name = "BtnRegistToTempDB";
            BtnRegistToTempDB.Size = new Size(190, 30);
            BtnRegistToTempDB.TabIndex = 1;
            BtnRegistToTempDB.Text = "データベースに登録する";
            BtnRegistToTempDB.UseVisualStyleBackColor = true;
            BtnRegistToTempDB.Click += BtnRegistToTempDB_Click;
            // 
            // LblElsapedTime
            // 
            LblElsapedTime.AutoSize = true;
            LblElsapedTime.Location = new Point(483, 465);
            LblElsapedTime.Name = "LblElsapedTime";
            LblElsapedTime.Size = new Size(0, 15);
            LblElsapedTime.TabIndex = 4;
            // 
            // FrmQRread
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(672, 501);
            Controls.Add(LblElsapedTime);
            Controls.Add(LblConnectionStatus);
            Controls.Add(CmbBoxPortNum);
            Controls.Add(BtnPortClose);
            Controls.Add(BtnOpenPort);
            Controls.Add(BtnRegistToTempDB);
            Controls.Add(BtnEditInput);
            Controls.Add(TxtBoxQRread);
            Name = "FrmQRread";
            Text = "QRコード入力";
            Load += FrmQRread_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TxtBoxQRread;
        private Button BtnEditInput;
        private ComboBox CmbBoxPortNum;
        private Button BtnOpenPort;
        private Button BtnPortClose;
        private Label LblConnectionStatus;
        private Button BtnRegistToTempDB;
        private Label LblElsapedTime;
    }
}