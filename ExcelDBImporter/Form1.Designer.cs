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
            DtpickStart.Location = new Point(158, 62);
            DtpickStart.Name = "DtpickStart";
            DtpickStart.Size = new Size(231, 23);
            DtpickStart.TabIndex = 3;
            // 
            // DtpickEnd
            // 
            DtpickEnd.Format = DateTimePickerFormat.Custom;
            DtpickEnd.Location = new Point(158, 91);
            DtpickEnd.Name = "DtpickEnd";
            DtpickEnd.Size = new Size(231, 23);
            DtpickEnd.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(75, 68);
            label2.Name = "label2";
            label2.Size = new Size(67, 15);
            label2.TabIndex = 1;
            label2.Text = "出力開始日";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(75, 97);
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
            // FrmExcelImpoerter
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(863, 253);
            Controls.Add(DtpickEnd);
            Controls.Add(DtpickStart);
            Controls.Add(BtnFieldNamAlias);
            Controls.Add(BtnOutpuToxlsx);
            Controls.Add(btnImputExcelFile);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBoxInputFilename);
            Name = "FrmExcelImpoerter";
            Text = "Excelファイルインポーター";
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
    }
}
