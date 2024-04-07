namespace ExcelDBImporter
{
    partial class FrmExcelImpoerter
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
            textBoxInputFilename = new TextBox();
            label1 = new Label();
            btnImputExcelFile = new Button();
            BtnOutpuToxlsx = new Button();
            DtpickStart = new DateTimePicker();
            DtpickEnd = new DateTimePicker();
            label2 = new Label();
            label3 = new Label();
            BtnFieldNamAlias = new Button();
            TextBoxOutputFileName = new TextBox();
            label4 = new Label();
            BtnOpenOutputDir = new Button();
            BtnUnsetOutputFlag = new Button();
            BtnShowQRForm = new Button();
            btnQRread = new Button();
            SuspendLayout();
            // 
            // textBoxInputFilename
            // 
            textBoxInputFilename.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxInputFilename.Font = new Font("BIZ UDゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            textBoxInputFilename.Location = new Point(158, 20);
            textBoxInputFilename.Multiline = true;
            textBoxInputFilename.Name = "textBoxInputFilename";
            textBoxInputFilename.ReadOnly = true;
            textBoxInputFilename.ScrollBars = ScrollBars.Horizontal;
            textBoxInputFilename.Size = new Size(685, 37);
            textBoxInputFilename.TabIndex = 0;
            textBoxInputFilename.WordWrap = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(75, 22);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 1;
            label1.Text = "入力ファイル名";
            // 
            // btnImputExcelFile
            // 
            btnImputExcelFile.Location = new Point(21, 192);
            btnImputExcelFile.Name = "btnImputExcelFile";
            btnImputExcelFile.Size = new Size(141, 48);
            btnImputExcelFile.TabIndex = 2;
            btnImputExcelFile.Text = "出荷物件予定表\r\n読み込み";
            btnImputExcelFile.UseVisualStyleBackColor = true;
            btnImputExcelFile.Click += BtnImputExcelFile_Click;
            // 
            // BtnOutpuToxlsx
            // 
            BtnOutpuToxlsx.Location = new Point(182, 192);
            BtnOutpuToxlsx.Name = "BtnOutpuToxlsx";
            BtnOutpuToxlsx.Size = new Size(141, 48);
            BtnOutpuToxlsx.TabIndex = 2;
            BtnOutpuToxlsx.Text = "マーシャリング実績\r\nxlsx出力(日付範囲)";
            BtnOutpuToxlsx.UseVisualStyleBackColor = true;
            BtnOutpuToxlsx.Click += BtnOutputToxlsx_Click;
            // 
            // DtpickStart
            // 
            DtpickStart.Format = DateTimePickerFormat.Custom;
            DtpickStart.Location = new Point(158, 103);
            DtpickStart.Name = "DtpickStart";
            DtpickStart.Size = new Size(231, 23);
            DtpickStart.TabIndex = 3;
            // 
            // DtpickEnd
            // 
            DtpickEnd.Format = DateTimePickerFormat.Custom;
            DtpickEnd.Location = new Point(158, 132);
            DtpickEnd.Name = "DtpickEnd";
            DtpickEnd.Size = new Size(231, 23);
            DtpickEnd.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(75, 109);
            label2.Name = "label2";
            label2.Size = new Size(67, 15);
            label2.TabIndex = 1;
            label2.Text = "出力開始日";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(75, 138);
            label3.Name = "label3";
            label3.Size = new Size(67, 15);
            label3.TabIndex = 1;
            label3.Text = "出力終了日";
            // 
            // BtnFieldNamAlias
            // 
            BtnFieldNamAlias.Location = new Point(344, 192);
            BtnFieldNamAlias.Name = "BtnFieldNamAlias";
            BtnFieldNamAlias.Size = new Size(141, 48);
            BtnFieldNamAlias.TabIndex = 2;
            BtnFieldNamAlias.Text = "テーブル列名設定";
            BtnFieldNamAlias.UseVisualStyleBackColor = true;
            BtnFieldNamAlias.Click += BtnFieldNamAlias_Click;
            // 
            // TextBoxOutputFileName
            // 
            TextBoxOutputFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TextBoxOutputFileName.Font = new Font("BIZ UDゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            TextBoxOutputFileName.Location = new Point(158, 63);
            TextBoxOutputFileName.Multiline = true;
            TextBoxOutputFileName.Name = "TextBoxOutputFileName";
            TextBoxOutputFileName.ReadOnly = true;
            TextBoxOutputFileName.ScrollBars = ScrollBars.Horizontal;
            TextBoxOutputFileName.Size = new Size(685, 37);
            TextBoxOutputFileName.TabIndex = 0;
            TextBoxOutputFileName.WordWrap = false;
            TextBoxOutputFileName.TextChanged += TextBoxOutputFileName_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(75, 65);
            label4.Name = "label4";
            label4.Size = new Size(77, 15);
            label4.TabIndex = 1;
            label4.Text = "出力ファイル名";
            // 
            // BtnOpenOutputDir
            // 
            BtnOpenOutputDir.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            BtnOpenOutputDir.Enabled = false;
            BtnOpenOutputDir.Location = new Point(724, 109);
            BtnOpenOutputDir.Name = "BtnOpenOutputDir";
            BtnOpenOutputDir.Size = new Size(119, 44);
            BtnOpenOutputDir.TabIndex = 4;
            BtnOpenOutputDir.Text = "出力ファイルの\r\nフォルダを開く";
            BtnOpenOutputDir.UseVisualStyleBackColor = true;
            BtnOpenOutputDir.Click += BtnOpenOutputDir_Click;
            // 
            // BtnUnsetOutputFlag
            // 
            BtnUnsetOutputFlag.Enabled = false;
            BtnUnsetOutputFlag.Location = new Point(589, 109);
            BtnUnsetOutputFlag.Name = "BtnUnsetOutputFlag";
            BtnUnsetOutputFlag.Size = new Size(129, 44);
            BtnUnsetOutputFlag.TabIndex = 5;
            BtnUnsetOutputFlag.Text = "表示日の出力済み\r\nフラグ解除(試験運用)";
            BtnUnsetOutputFlag.UseVisualStyleBackColor = true;
            BtnUnsetOutputFlag.Click += BtnUnsetOutputFlag_Click;
            // 
            // BtnShowQRForm
            // 
            BtnShowQRForm.Location = new Point(182, 264);
            BtnShowQRForm.Name = "BtnShowQRForm";
            BtnShowQRForm.Size = new Size(141, 48);
            BtnShowQRForm.TabIndex = 2;
            BtnShowQRForm.Text = "QRコード印刷";
            BtnShowQRForm.UseVisualStyleBackColor = true;
            BtnShowQRForm.Click += BtnShowQRForm_Click;
            // 
            // btnQRread
            // 
            btnQRread.Location = new Point(21, 264);
            btnQRread.Name = "btnQRread";
            btnQRread.Size = new Size(141, 48);
            btnQRread.TabIndex = 2;
            btnQRread.Text = "QRコード読み取り";
            btnQRread.UseVisualStyleBackColor = true;
            btnQRread.Click += BtnfrmShowQR_Read_Click;
            // 
            // FrmExcelImpoerter
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(863, 324);
            Controls.Add(BtnUnsetOutputFlag);
            Controls.Add(BtnOpenOutputDir);
            Controls.Add(DtpickEnd);
            Controls.Add(DtpickStart);
            Controls.Add(btnQRread);
            Controls.Add(BtnShowQRForm);
            Controls.Add(BtnFieldNamAlias);
            Controls.Add(BtnOutpuToxlsx);
            Controls.Add(btnImputExcelFile);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label4);
            Controls.Add(label1);
            Controls.Add(TextBoxOutputFileName);
            Controls.Add(textBoxInputFilename);
            KeyPreview = true;
            Name = "FrmExcelImpoerter";
            Text = "Excelファイルインポーター";
            KeyDown += FrmExcelImpoerter_KeyDown;
            KeyUp += FrmExcelImpoerter_KeyUp;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxInputFilename;
        private Label label1;
        private Button btnImputExcelFile;
        private Button BtnOutpuToxlsx;
        private DateTimePicker DtpickStart;
        private DateTimePicker DtpickEnd;
        private Label label2;
        private Label label3;
        private Button BtnFieldNamAlias;
        private TextBox TextBoxOutputFileName;
        private Label label4;
        private Button BtnOpenOutputDir;
        private Button BtnUnsetOutputFlag;
        private Button BtnShowQRForm;
        private Button btnQRread;
    }
}
