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
            BtnPortOpen = new Button();
            BtnPortClose = new Button();
            LblConnectionStatus = new Label();
            SuspendLayout();
            // 
            // TxtBoxQRread
            // 
            TxtBoxQRread.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TxtBoxQRread.ImeMode = ImeMode.Disable;
            TxtBoxQRread.Location = new Point(16, 86);
            TxtBoxQRread.Multiline = true;
            TxtBoxQRread.Name = "TxtBoxQRread";
            TxtBoxQRread.Size = new Size(455, 352);
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
            // BtnPortOpen
            // 
            BtnPortOpen.Location = new Point(16, 41);
            BtnPortOpen.Name = "BtnPortOpen";
            BtnPortOpen.Size = new Size(133, 30);
            BtnPortOpen.TabIndex = 1;
            BtnPortOpen.Text = "ポートを開く";
            BtnPortOpen.UseVisualStyleBackColor = true;
            BtnPortOpen.Click += BtnPortOpen_Click;
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
            // FrmQRread
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(529, 501);
            Controls.Add(LblConnectionStatus);
            Controls.Add(CmbBoxPortNum);
            Controls.Add(BtnPortClose);
            Controls.Add(BtnPortOpen);
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
        private Button BtnPortOpen;
        private Button BtnPortClose;
        private Label LblConnectionStatus;
    }
}